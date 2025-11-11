using EventManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Api.Data
{
    public class EventManagerDbContext : DbContext
    {
        public EventManagerDbContext(DbContextOptions<EventManagerDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<QrCode> QrCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relationships
            modelBuilder.Entity<Event>()
                .HasMany(e => e.QrCodes)
                .WithOne(q => q.Event)
                .HasForeignKey(q => q.EventId);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Guests)
                .WithOne()
                .HasForeignKey(g => g.EventId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Events)
                .WithOne(e => e.Planner)
                .HasForeignKey(e => e.PlannerId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Event)
                .WithMany()
                .HasForeignKey(t => t.EventId);

            // Unique Indexes
            modelBuilder.Entity<QrCode>()
                .HasIndex(q => new { q.EventId, q.QrCodeValue })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Reference)
                .IsUnique()
                .HasFilter("[Reference] IS NOT NULL");

            // No HasData to avoid determinism issues
        }
    }
}