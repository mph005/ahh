using System;
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

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on specified ports
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

// Add services to the container.
builder.Services.AddControllers();

// Add EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => {
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
builder.Services.ConfigureApplicationCookie(options =>
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
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITherapistRepository, TherapistRepository>();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ISoapNoteRepository, SoapNoteRepository>();

// Add services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ITherapistService, MassageBooking.API.Services.TherapistService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAdminService, AdminService>();
// Add other core services as needed
// builder.Services.AddScoped<ISoapNoteService, SoapNoteService>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Configure Authentication (e.g., JWT Bearer)
builder.Services.AddAuthentication()
    .AddJwtBearer(options => // Add options for JWT Bearer validation
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add CORS
builder.Services.AddCors(options =>
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    
    // Create and seed the database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            // context.Database.EnsureCreated(); // Replace with Migrations
            // Apply migrations automatically (optional, consider explicit update instead)
            // context.Database.Migrate(); 
            SeedData.Initialize(services);
            await SeedDatabaseAsync(app);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating/seeding the database.");
        }
    }
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
app.MapControllers();

app.Run();

async Task SeedDatabaseAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>(); // Get configuration

        logger.LogInformation("Starting database seeding...");

        string adminRole = "Admin";
        string therapistRole = "Therapist";
        string clientRole = "Client"; // Assuming you might want a Client role too

        // Ensure roles exist
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));
            logger.LogInformation($"Role '{adminRole}' created.");
        }
        if (!await roleManager.RoleExistsAsync(therapistRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(therapistRole));
             logger.LogInformation($"Role '{therapistRole}' created.");
        }
         if (!await roleManager.RoleExistsAsync(clientRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(clientRole));
             logger.LogInformation($"Role '{clientRole}' created.");
        }

        // --- Ensure default Admin User exists ---
        // **IMPORTANT**: Get credentials from configuration, NOT hardcoded!
        var adminEmail = configuration["DefaultAdminUser:Email"];
        var adminPassword = configuration["DefaultAdminUser:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            logger.LogWarning("Default Admin User credentials not found in configuration (DefaultAdminUser:Email, DefaultAdminUser:Password). Skipping admin user creation.");
        }
        else
        {
             var adminUser = await userManager.FindByEmailAsync(adminEmail);
             if (adminUser == null)
             {
                 adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true }; // Assuming EmailConfirmed for simplicity
                 var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
                 if (createUserResult.Succeeded)
                 {
                     logger.LogInformation($"Admin user '{adminEmail}' created successfully.");
                     // Assign Admin role
                     var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                     if(addToRoleResult.Succeeded)
                     {
                        logger.LogInformation($"User '{adminEmail}' added to role '{adminRole}'.");
                     }
                     else
                     {
                         logger.LogError($"Error adding user '{adminEmail}' to role '{adminRole}': {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                     }
                 }
                 else
                 {
                     logger.LogError($"Error creating admin user '{adminEmail}': {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                 }
             }
             else
             {
                 // Ensure existing admin user has the Admin role
                 if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                 {
                     var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                     if(addToRoleResult.Succeeded)
                     {
                        logger.LogInformation($"Existing user '{adminEmail}' added to role '{adminRole}'.");
                     }
                      else
                     {
                         logger.LogError($"Error adding existing user '{adminEmail}' to role '{adminRole}': {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                     }
                 }
                  else
                 {
                    logger.LogInformation($"User '{adminEmail}' already exists and has role '{adminRole}'.");
                 }
             }
        }
         logger.LogInformation("Database seeding finished.");
    }
} 