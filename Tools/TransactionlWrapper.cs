namespace WebApp.Tools
{
    using Npgsql;
    using System.Data;

    public static class TransactionWrapper
    {
        public static async Task InTransaction(
            this NpgsqlConnection dbConnection,
            Func<Task> callback,
            CancellationToken cancellationToken = default
            )
        {
            if(dbConnection.State == ConnectionState.Closed)
            {
                await dbConnection.OpenAsync(cancellationToken);
            }

            await using var trancation = await dbConnection.BeginTransactionAsync(cancellationToken);

            try
            {
                await callback();

                await trancation.CommitAsync(cancellationToken);
            }
            catch
            {
                await trancation.RollbackAsync(cancellationToken);
                throw;
            }
        }

    }
}
