namespace WebApp;

using Microsoft.EntityFrameworkCore;
using WebApp.Entities;
public partial class EventStoreContext : DbContext
{
    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Stream> Streams { get; set; }


    public EventStoreContext()
    {
    }

    public EventStoreContext(DbContextOptions<EventStoreContext> options)
        : base(options)
    {
    }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.ToTable("events");

            entity.HasIndex(e => new { e.Streamfk, e.Version }, "events_stream_and_version").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasColumnName("created");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Streamfk).HasColumnName("streamfk");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasOne(d => d.StreamfkNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.Streamfk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("events_streamfk_fkey");
        });

        modelBuilder.Entity<Stream>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("streams_pkey");

            entity.ToTable("streams");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Version).HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
