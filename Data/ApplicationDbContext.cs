
using IncidentManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Data
{
    public class ApplicationDbContext
      : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Ticket> Tickets { get; set; } = null!;

        public DbSet<TicketAssignment> TicketAssignments
        { get; set; } = null!;

        public DbSet<TicketUpdate> TicketUpdates
        { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.RequestNumber)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAssignment>()
                .HasOne(a => a.Ticket)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketUpdate>()
                .HasOne(u => u.Ticket)
                .WithMany(t => t.Updates)
                .HasForeignKey(u => u.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
