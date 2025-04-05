using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MassageBooking.API.Tests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";
        public const string DefaultUserId = "00000000-0000-0000-0000-000000000001";
        public const string DefaultUserName = "test.user@example.com";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for a specific header or claim to determine the user/roles for the test
            if (Context.Request.Headers.TryGetValue("X-Test-User-Role", out var roleValue))
            {
                // User is authenticated with specified roles
                var roles = roleValue.ToString().Split(',');
                var claims = new List<Claim> { 
                    new Claim(ClaimTypes.NameIdentifier, DefaultUserId), 
                    new Claim(ClaimTypes.Name, DefaultUserName) 
                };
                
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                }

                var identity = new ClaimsIdentity(claims, AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            // If no role header, treat as anonymous (authentication failed)
            return Task.FromResult(AuthenticateResult.Fail("No test role header found"));
        }
    }
} 