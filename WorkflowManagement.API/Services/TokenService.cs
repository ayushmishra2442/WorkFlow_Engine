using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Services
{
    public class TokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetUserEmailFromToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return Task.FromResult(string.Empty);
                }

                // Extract bearer token from Authorization header
                if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    return Task.FromResult(string.Empty);
                }

                var token = authHeader.ToString().Replace("Bearer ", "").Trim();
                if (token.Contains("&"))
                {
                    token = token.Split('&')[0];
                }

                if (string.IsNullOrEmpty(token))
                {
                    return Task.FromResult(string.Empty);
                }

                // Use JwtSecurityTokenHandler to read the token safely
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // Check if token is valid JWT format
                if (!tokenHandler.CanReadToken(token))
                {
                    return Task.FromResult(string.Empty);
                }

                // Read the token (already validated by authentication middleware)
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Extract standard email claims
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
                
                return Task.FromResult(email ?? string.Empty);
            }
            catch (Exception)
            {
                return Task.FromResult(string.Empty);
            }
        }
    }
}
