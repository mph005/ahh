using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MassageBooking.API.Data;
using MassageBooking.API.Data.Repositories;
using MassageBooking.API.Middleware;
using MassageBooking.API.Models;
using MassageBooking.API.Services;

namespace MassageBooking.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Add EF Core
            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add ASP.NET Core Identity
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => {
                // Password settings (example - adjust as needed)
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedAccount = false; // Set to true if email confirmation is needed
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure cookie authentication behavior for API vs UI redirects
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Default login path (adjust if needed)
                options.LogoutPath = "/Account/Logout"; // Default logout path (adjust if needed)
                options.AccessDeniedPath = "/Account/AccessDenied"; // Default access denied path (adjust if needed)
                options.Events.OnRedirectToLogin = context =>
                {
                    // If the request is for an API endpoint, return 401 instead of redirecting
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    // If the request is for an API endpoint, return 403 instead of redirecting
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }
                    return Task.CompletedTask;
                };
            });

            // Add repositories
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<ITherapistRepository, TherapistRepository>();
            services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<ISoapNoteRepository, SoapNoteRepository>();

            // Add services
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<ITherapistService, Services.TherapistService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IAdminService, AdminService>();
            // Add other core services as needed
            // services.AddScoped<ISoapNoteService, SoapNoteService>();

            // Add logging
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            });

            // Configure Authentication (e.g., JWT Bearer)
            services.AddAuthentication()
                .AddJwtBearer(options => // Add options for JWT Bearer validation
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]
                            ?? throw new InvalidOperationException("JWT Key not configured")))
                    };
                });

            // Add AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowLocalhost3000");

            // Add Authentication middleware BEFORE Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();
            
            // Use UseEndpoints instead of MapControllers
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
} 