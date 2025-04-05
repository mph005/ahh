using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MassageBooking.API.DTOs;
using MassageBooking.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MassageBooking.API.Services;
using MassageBooking.API.Data.Repositories;

namespace MassageBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IClientRepository _clientRepository;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IClientRepository clientRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _clientRepository = clientRepository;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", loginDto.Email);
                return Unauthorized(new LoginResponseDTO { Success = false, Message = "Invalid email or password." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new LoginResponseDTO
                {
                    Success = true,
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    Roles = roles.ToList(),
                    Message = "Login successful."
                });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out for email: {Email}", loginDto.Email);
                return Unauthorized(new LoginResponseDTO { Success = false, Message = "Account locked out." });
            }
            if (result.IsNotAllowed)
            {
                 _logger.LogWarning("Login not allowed for email: {Email} (e.g., requires confirmation)", loginDto.Email);
                return Unauthorized(new LoginResponseDTO { Success = false, Message = "Login not allowed. Please confirm your email if required." });
            }
            _logger.LogWarning("Invalid password attempt for email: {Email}", loginDto.Email);
            return Unauthorized(new LoginResponseDTO { Success = false, Message = "Invalid email or password." });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok(new { Message = "Logout successful." });
        }

        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not found or not authenticated." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", registerDto.Email);
                // Return a generic error to avoid revealing which emails are registered
                return Conflict(new { Message = "An account with this email already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email, // Use email as username by default
                Email = registerDto.Email,
                EmailConfirmed = true // Auto-confirm for now, can add email verification later
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully.", registerDto.Email);

                // Assign the "Client" role by default
                // Make sure the "Client" role exists (seeded in Program.cs)
                try
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    _logger.LogInformation("Assigned 'Client' role to user {Email}.", registerDto.Email);

                    // Create a client record in the database
                    var client = new Client
                    {
                        ClientId = user.Id, // Use same GUID as the Identity user
                        Email = registerDto.Email,
                        FirstName = registerDto.FirstName,
                        LastName = registerDto.LastName,
                        Phone = registerDto.Phone,
                        DateOfBirth = registerDto.DateOfBirth,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _clientRepository.AddAsync(client);
                    _logger.LogInformation("Created client record for user {Email} with ID {ClientId}.", registerDto.Email, client.ClientId);
                }
                catch (Exception ex)
                {
                    // Log error if role assignment or client creation fails
                    _logger.LogError(ex, "Failed during client setup for user {Email}.", registerDto.Email);
                    // Consider cleanup if this is critical - for now proceed but log the error
                }

                return Ok(new { Message = "Registration successful." });
            }

            // Log errors if creation failed
            foreach (var error in result.Errors)
            {
                _logger.LogError("User registration error: {Code} - {Description}", error.Code, error.Description);
            }
            return BadRequest(new { Message = "User registration failed.", Errors = result.Errors.Select(e => e.Description) });
        }
    }
} 