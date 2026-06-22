using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using WorkflowManagement.Application.Interfaces;

namespace WorkflowManagement.API.Services
{
    public class CustomClaimsTransformation : IClaimsTransformation
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly TokenService _tokenService;

        public CustomClaimsTransformation(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            TokenService tokenService)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _tokenService = tokenService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return principal;
            }

            string email = await _tokenService.GetUserEmailFromToken();

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user != null && user.IsActive)
                {
                    // Add local db_user_id claim if not already present
                    if (!identity.HasClaim(c => c.Type == "db_user_id"))
                    {
                        identity.AddClaim(new Claim("db_user_id", user.UserId.ToString()));
                    }

                    // Retrieve roles for the user
                    var roles = await _userRoleRepository.GetRolesForUserAsync(user.UserId);

                    foreach (var role in roles)
                    {
                        if (!string.IsNullOrEmpty(role.RoleName) && !identity.HasClaim(ClaimTypes.Role, role.RoleName))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName, ClaimValueTypes.String));
                        }
                    }
                }
            }

            return principal;
        }
    }
}
