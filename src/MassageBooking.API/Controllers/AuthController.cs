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

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
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
    }
} 