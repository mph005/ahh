using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Models;

namespace MassageBooking.API.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>>());

            // Check if the database already has data
            if (context.Therapists.Any() && context.Services.Any() && context.Clients.Any())
            {
                return; // Database has been seeded
            }

            // Seed services
            var services = new List<Service>
            {
                new Service
                {
                    ServiceId = Guid.NewGuid(),
                    Name = "Swedish Massage",
                    Description = "A gentle form of massage that uses long strokes, kneading, and circular movements on superficial muscles.",
                    Duration = 60,
                    Price = 80.00M,
                    IsActive = true
                },
                new Service
                {
                    ServiceId = Guid.NewGuid(),
                    Name = "Deep Tissue Massage",
                    Description = "A massage therapy that focuses on realigning deeper layers of muscles and connective tissue.",
                    Duration = 60,
                    Price = 90.00M,
                    IsActive = true
                },
                new Service
                {
                    ServiceId = Guid.NewGuid(),
                    Name = "Hot Stone Massage",
                    Description = "Massage therapy that uses smooth, heated stones placed at key points on the body.",
                    Duration = 90,
                    Price = 110.00M,
                    IsActive = true
                },
                new Service
                {
                    ServiceId = Guid.NewGuid(),
                    Name = "Aromatherapy Massage",
                    Description = "Massage therapy that uses essential oils for added therapeutic benefit.",
                    Duration = 60,
                    Price = 95.00M,
                    IsActive = true
                }
            };
            context.Services.AddRange(services);
            context.SaveChanges();

            // Seed therapists
            var therapists = new List<Therapist>
            {
                new Therapist
                {
                    TherapistId = Guid.NewGuid(),
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@example.com",
                    Phone = "555-123-4567",
                    Title = "Senior Massage Therapist",
                    Bio = "Sarah has over 10 years of experience specializing in Swedish and Deep Tissue massage.",
                    ImageUrl = "https://randomuser.me/api/portraits/women/44.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Therapist
                {
                    TherapistId = Guid.NewGuid(),
                    FirstName = "Michael",
                    LastName = "Brown",
                    Email = "michael.brown@example.com",
                    Phone = "555-987-6543",
                    Title = "Massage Therapist",
                    Bio = "Michael specializes in sports massage and rehabilitation therapy.",
                    ImageUrl = "https://randomuser.me/api/portraits/men/32.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Therapist
                {
                    TherapistId = Guid.NewGuid(),
                    FirstName = "Emily",
                    LastName = "Davis",
                    Email = "emily.davis@example.com",
                    Phone = "555-456-7890",
                    Title = "Holistic Massage Therapist",
                    Bio = "Emily practices a blend of Eastern and Western massage techniques with a focus on wellness.",
                    ImageUrl = "https://randomuser.me/api/portraits/women/22.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            context.Therapists.AddRange(therapists);
            context.SaveChanges();

            // Seed therapist services (which services each therapist offers)
            var therapistServices = new List<TherapistService>();
            
            // Sarah offers all services
            foreach (var service in services)
            {
                therapistServices.Add(new TherapistService
                {
                    TherapistId = therapists[0].TherapistId,
                    ServiceId = service.ServiceId
                });
            }
            
            // Michael offers Swedish and Deep Tissue
            therapistServices.Add(new TherapistService
            {
                TherapistId = therapists[1].TherapistId,
                ServiceId = services[0].ServiceId // Swedish
            });
            therapistServices.Add(new TherapistService
            {
                TherapistId = therapists[1].TherapistId,
                ServiceId = services[1].ServiceId // Deep Tissue
            });
            
            // Emily offers Hot Stone and Aromatherapy
            therapistServices.Add(new TherapistService
            {
                TherapistId = therapists[2].TherapistId,
                ServiceId = services[2].ServiceId // Hot Stone
            });
            therapistServices.Add(new TherapistService
            {
                TherapistId = therapists[2].TherapistId,
                ServiceId = services[3].ServiceId // Aromatherapy
            });
            
            context.TherapistServices.AddRange(therapistServices);
            context.SaveChanges();

            // Seed clients
            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Phone = "555-111-2222",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    Address = "123 Main St, Anytown, USA",
                    Notes = "Prefers evening appointments",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    ClientId = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Phone = "555-333-4444",
                    DateOfBirth = new DateTime(1990, 3, 22),
                    Address = "456 Oak Ave, Anytown, USA",
                    Notes = "Has lower back issues",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    ClientId = Guid.NewGuid(),
                    FirstName = "Robert",
                    LastName = "Williams",
                    Email = "robert.williams@example.com",
                    Phone = "555-555-6666",
                    DateOfBirth = new DateTime(1978, 11, 30),
                    Address = "789 Pine St, Anytown, USA",
                    Notes = "First-time client, gift certificate",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            context.Clients.AddRange(clients);
            context.SaveChanges();

            // Seed therapist availability (default working hours)
            var now = DateTime.UtcNow;
            var availabilities = new List<Availability>();
            
            // Set up availability for each therapist (Mon-Fri, 9am-5pm)
            foreach (var therapist in therapists)
            {
                for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                {
                    availabilities.Add(new Availability
                    {
                        TherapistId = therapist.TherapistId,
                        DayOfWeek = day,
                        IsAvailable = true,
                        StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                        EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                        BreakStartTime = new TimeSpan(12, 0, 0), // 12:00 PM
                        BreakEndTime = new TimeSpan(13, 0, 0), // 1:00 PM
                        Notes = "Regular working hours",
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }
            
            context.Availabilities.AddRange(availabilities);
            context.SaveChanges();

            // Seed appointments (sample future appointments)
            var startDate = DateTime.Today.AddDays(1); // Start from tomorrow
            
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = clients[0].ClientId, // John
                    TherapistId = therapists[0].TherapistId, // Sarah
                    ServiceId = services[0].ServiceId, // Swedish
                    StartTime = startDate.AddHours(10), // 10:00 AM
                    EndTime = startDate.AddHours(11), // 11:00 AM (60 min)
                    Status = AppointmentStatus.Scheduled,
                    Notes = "First appointment",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = clients[1].ClientId, // Jane
                    TherapistId = therapists[1].TherapistId, // Michael
                    ServiceId = services[1].ServiceId, // Deep Tissue
                    StartTime = startDate.AddHours(14), // 2:00 PM
                    EndTime = startDate.AddHours(15), // 3:00 PM (60 min)
                    Status = AppointmentStatus.Scheduled,
                    Notes = "Focus on lower back",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = clients[2].ClientId, // Robert
                    TherapistId = therapists[2].TherapistId, // Emily
                    ServiceId = services[2].ServiceId, // Hot Stone
                    StartTime = startDate.AddDays(1).AddHours(11), // Next day, 11:00 AM
                    EndTime = startDate.AddDays(1).AddHours(12).AddMinutes(30), // Next day, 12:30 PM (90 min)
                    Status = AppointmentStatus.Scheduled,
                    Notes = "Gift certificate redemption",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = clients[0].ClientId, // John
                    TherapistId = therapists[0].TherapistId, // Sarah
                    ServiceId = services[0].ServiceId, // Swedish
                    StartTime = DateTime.Today.AddDays(-1).AddHours(15), // Yesterday 3:00 PM
                    EndTime = DateTime.Today.AddDays(-1).AddHours(16), // Yesterday 4:00 PM
                    Status = AppointmentStatus.Completed,
                    Notes = "Regular client",
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                },
                new Appointment
                {
                    AppointmentId = Guid.NewGuid(),
                    ClientId = clients[1].ClientId, // Jane
                    TherapistId = therapists[1].TherapistId, // Michael
                    ServiceId = services[1].ServiceId, // Deep Tissue
                    StartTime = DateTime.Today.AddDays(-2).AddHours(9), // Two days ago 9:00 AM
                    EndTime = DateTime.Today.AddDays(-2).AddHours(10), // Two days ago 10:00 AM
                    Status = AppointmentStatus.Cancelled,
                    Notes = "Client cancelled last minute",
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-3)
                }
            };
            
            context.Appointments.AddRange(appointments);
            context.SaveChanges();
        }
    }
} 