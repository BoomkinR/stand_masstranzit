using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stand.Domain;

namespace Stand.Infrastructure;

public class DocumentContext: DbContext
{

public DocumentContext(DbContextOptions options) : base(options)
{
}
    public DbSet<Document> Documents { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Stand");

            modelBuilder.UseSerialColumns();

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();


            modelBuilder.Entity<Document>().HasKey(x => x.Id);
            modelBuilder.Entity<Document>()
                .Property(x => x.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Document>()
                .Property(x => x.IsComplete)
                .HasColumnName("IS_COMPLETE");
            modelBuilder.Entity<Document>()
                .Property(x => x.Name)
                .HasColumnName("NAME");

        }
}