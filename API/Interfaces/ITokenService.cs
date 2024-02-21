using System.Security.Claims;
using API.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessToken(AppUser user, List<Claim> claims = null);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
