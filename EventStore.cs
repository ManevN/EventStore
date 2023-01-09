namespace WebApp
{
    using Dapper;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Npgsql;
    using System.Data;
    using WebApp.Tools;

    public class EventStore : DbContext
    {
        private readonly NpgsqlConnection dbConnection;

        public EventStore(DbContextOptions<EventStore> options, NpgsqlConnection dbConnection) : base(options)
       {
            this.dbConnection = dbConnection;
        }

        public void Init()
        {
            CreateStreamsTable();
            CreateEventsTable();
            CreateAppendEventFunction();
        }

        public async Task<IReadOnlyList<object>> GetEventsAsync(
            Guid streamId,
            long? atStreamVersion = null,
            DateTime? atTimeStamp = null,
            CancellationToken cancellationToken = default
            )
        {

            string atStreamSqlCondition = atStreamVersion != null ? "AND version <= @atStreamVersion" : string.Empty;
            string atTimeStampSqlCondition = atTimeStamp != null ? "AND created <= @atTimeStamp" : string.Empty;

            var getStreamSql = @$"SELECT id, data, stream_id, type, version, created
                                  FROM events
                                  WHERE sream_id = @stramId
                                  {atStreamSqlCondition}
                                  {atTimeStampSqlCondition}
                                  ORDER BY version";

            var evetns = await dbConnection.QueryAsync<dynamic>(getStreamSql, new { streamId, atStreamVersion, atTimeStamp });

            return evetns.Select(@event =>
                    JsonConvert.DeserializeObject(
                        @event.data,
                        Type.GetType(@event.type)
                    ))
                .ToList();
        }

        public Task AppendEventAsync<TStream>(
            Guid streamId,
            IEnumerable<object> events,
            long? expectedVersion = null,
            CancellationToken cancellationToken = default
            ) where TStream : notnull =>
                dbConnection.InTransaction(async () =>
                {
                    foreach (var @event in events)
                    {
                        var succeeded = await dbConnection.QuerySingleAsync<bool>
                        (
                            "SELECT append_event(@Id, @Data::jsonb, @Type, @StreamId, @StreamType, @ExpectationVersion)",
                            new
                            {
                                id = Guid.NewGuid(),
                                Date = JsonConvert.SerializeObject(@event),
                                Type = @event.GetType().AssemblyQualifiedName,
                                StreamId = streamId,
                                StreamType = typeof(TStream).AssemblyQualifiedName,
                                ExpectedVersion = expectedVersion++
                            },
                            commandType: CommandType.Text
                        );

                        if (!succeeded)
                        {
                            throw new InvalidOperationException("Expected version did not match the stream version!");
                        }
                    }
                },
                cancellationToken
                );                  
 
        private void CreateAppendEventFunction()
        {
            const string createAppendEventFunctionSql =
                @"CREATE OR REPLACE FUNCTION append_event(
                        id uuid,
                        data jsonb,
                        type text,
                        stream_id uuid,
                        stream_type text,
                        expected_stream_version bigint default null
                        ) RETURNS boolean
                            LANGUAGE plpqsql
                            AS $$
                            DECLARE
                                stream_version int;
                            BEGIN


                        -- get stream version 
                        select 
                            version into stream_version
                        from stream as s
                        where 
                        s.id = stream_id for update


                        -- if stream doesn't exist - create new one with version 0 
                        IF stream_version IS NULL THEN
                            stream_version := 0;

                        INSERT INTO streams
                            (id, type, version)
                        VALUES
                            (stream_id, stream_type, stream_version)
                        END IF;


                        -- check optimistic concurrency
                        IF expected_stream_version IS NULL NULL AND stream_version != expected_stream_version THEN
                            RETURN FALSE;
                        END IF;


                        -- increment event_version 
                        stream_version := stream_version + 1;


                         -- append event
                        insert into events 
                            (id, data, stream_id, type, version)
                        values 
                            (id, data::jsonb, stream_id, type, stream_version);


                        -- update stream verson 
                        UPDATE stream as s
                            SET version = strem_version 
                        WHERE
                            s.Id = stream_id
                        
                            RETURN TRUE;
                        END;
                        $$;";

            dbConnection.Execute(createAppendEventFunctionSql);
        }

        private void CreateEventsTable()
        {
            const string createEventsTableSql =
                @"CREATE TABLE IF NOT EXISTS Events
                    (
                        Id              UUID                        NOT NULL PRIMARY KEY,
                        Data            JSONB                       NOT NULL,
                        StreamFK        UUID                        NOT NULL,
                        Type            TEXT                        NOT NULL,
                        Version         BIGINT                      NOT NULL,                                             
                        Created         timestamp with time zone    NOT NULL default(now()),
                        FOREIGN KEY(StreamFK) REFERENCES Streams(Id),
                        CONSTRAINT events_stream_and_version UNIQUE(StreamFK, Version)
                    )";

            dbConnection.Execute(createEventsTableSql);
        }

        private void CreateStreamsTable()
        {
            const string createStreamsTableSql =
                @"CREATE TABLE IF NOT EXISTS Streams
                    (
                        Id              UUID                        NOT NULL PRIMARY KEY,                                      
                        Type            TEXT                        NOT NULL,
                        Version         BIGINT                      NOT NULL
                    )";

            dbConnection.Execute(createStreamsTableSql);
        }
    }
}
