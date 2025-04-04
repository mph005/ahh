using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Therapist> Therapists { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<SoapNote> SoapNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Therapist)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<SoapNote>()
                .HasOne(s => s.Appointment)
                .WithOne()
                .HasForeignKey<SoapNote>(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<SoapNote>()
                .HasOne(s => s.Therapist)
                .WithMany()
                .HasForeignKey(s => s.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<SoapNote>()
                .HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Therapist)
                .WithMany()
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 