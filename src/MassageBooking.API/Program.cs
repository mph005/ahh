using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassageBooking.API.Data;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Services;
using MassageBooking.API.Middleware;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MassageBooking.API.Models;

namespace MassageBooking.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // Create and seed the database in development
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var env = services.GetRequiredService<IHostEnvironment>();
                
                if (env.IsDevelopment())
                {
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        SeedData.Initialize(services);
                        SeedDatabaseAsync(host).Wait();
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating/seeding the database.");
                    }
                }
            }
            
            host.Run();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:5000", "https://localhost:5001");
                });
                
        private static async Task SeedDatabaseAsync(IHost app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = services.GetRequiredService<IConfiguration>(); // Get configuration

                logger.LogInformation("Starting database seeding...");

                // Create default roles if they don't exist
                string[] roles = { "Admin", "Therapist", "Client" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                        logger.LogInformation($"Created role: {role}");
                    }
                }

                // Create a default admin user if no admin users exist
                if (!await userManager.Users.AnyAsync(u => u.Email == "admin@example.com"))
                {
                    var user = new ApplicationUser
                    {
                        UserName = "admin@example.com",
                        Email = "admin@example.com",
                        EmailConfirmed = true
                    };

                    // Get admin password from configuration
                    var adminPassword = configuration["DefaultAdminPassword"] ?? "Admin123!";
                    var result = await userManager.CreateAsync(user, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                        logger.LogInformation("Created default admin user");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError($"Could not create default admin: {errors}");
                    }
                }

                logger.LogInformation("Database seeding completed");
            }
        }
    }
} 