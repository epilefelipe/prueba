using Microsoft.EntityFrameworkCore;
using TicketManager.Domain.Entities;

namespace TicketManager.Infrastructure.Data
{
    public class TicketDbContext : DbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options) { }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
                entity.HasOne(e => e.Ticket)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(e => e.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
