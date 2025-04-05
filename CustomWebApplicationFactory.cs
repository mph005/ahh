using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MassageBooking.API.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace MassageBooking.API.Tests
{
    // Use the actual Program class from the API project as TEntryPoint
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace DbContext with an in-memory database for tests
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    // Fix: Use SQL Server with a test connection string for testing
                    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MassageBookingTest;Trusted_Connection=True;");
                });

                // --- Mock Authentication Setup --- 
                // Remove existing authentication
                var authDescriptors = services.Where(d => 
                    d.ServiceType == typeof(IAuthenticationService) || 
                    d.ServiceType.Name.Contains("AuthenticationHandler")).ToList();
                foreach (var d in authDescriptors) services.Remove(d);
                
                // Add our test authentication handler
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme) // Use the constant scheme name
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => {});

                // Ensure Application Insights is disabled or mocked for tests
                var appInsightsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration));
                if (appInsightsDescriptor != null)
                {
                    // Remove it or replace with a mock if necessary to avoid sending test telemetry
                    // services.Remove(appInsightsDescriptor);
                }
            });

            builder.UseEnvironment("Development"); // Or "Testing" if you have specific settings
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // We can further customize the host here if needed
            var host = base.CreateHost(builder);
            
            // Example: Seed database for tests
            // using (var scope = host.Services.CreateScope())
            // {
            //     var services = scope.ServiceProvider;
            //     var context = services.GetRequiredService<ApplicationDbContext>();
            //     // Seed data
            // }

            return host;
        }
    }
} 