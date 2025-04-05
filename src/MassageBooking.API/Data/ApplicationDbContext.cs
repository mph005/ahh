using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Models;
using System;

namespace MassageBooking.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Therapist> Therapists { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Availability> Availabilities { get; set; } = null!;
        public DbSet<TherapistService> TherapistServices { get; set; } = null!;
        public DbSet<SoapNote> SoapNotes { get; set; } = null!;
        // Remove the AuditLog DbSet as the model doesn't exist
        // public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure relationships
            builder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Appointment>()
                .HasOne(a => a.Therapist)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<SoapNote>()
                .HasOne(s => s.Appointment)
                .WithOne()
                .HasForeignKey<SoapNote>(s => s.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<SoapNote>()
                .HasOne(s => s.Therapist)
                .WithMany(t => t.SoapNotes)
                .HasForeignKey(s => s.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<SoapNote>()
                .HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Availability>()
                .HasOne(a => a.Therapist)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite key for TherapistService
            builder.Entity<TherapistService>()
                .HasKey(ts => new { ts.TherapistId, ts.ServiceId });

            builder.Entity<TherapistService>()
                .HasOne(ts => ts.Therapist)
                .WithMany(t => t.Services)
                .HasForeignKey(ts => ts.TherapistId);

            builder.Entity<TherapistService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.Therapists)
                .HasForeignKey(ts => ts.ServiceId);

            // Potential configuration for decimal precision (if needed)
            builder.Entity<Service>()
               .Property(s => s.Price)
               .HasColumnType("decimal(18, 2)");

            // Rename Identity tables if desired (optional)
            // builder.Entity<ApplicationUser>(entity => { entity.ToTable(name: "Users"); });
            // builder.Entity<IdentityRole<Guid>>(entity => { entity.ToTable(name: "Roles"); });
            // builder.Entity<IdentityUserRole<Guid>>(entity => { entity.ToTable("UserRoles"); });
            // builder.Entity<IdentityUserClaim<Guid>>(entity => { entity.ToTable("UserClaims"); });
            // builder.Entity<IdentityUserLogin<Guid>>(entity => { entity.ToTable("UserLogins"); });
            // builder.Entity<IdentityRoleClaim<Guid>>(entity => { entity.ToTable("RoleClaims"); });
            // builder.Entity<IdentityUserToken<Guid>>(entity => { entity.ToTable("UserTokens"); });
            
            // Add any other custom model configurations here
        }
    }
} 