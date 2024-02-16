using System.Security.Claims;

namespace API.Interfaces;

public interface ITokenService
{
    public string GenerateAccessToken(string username, IEnumerable<Claim> claims = null);
    public string GenerateRefreshToken();
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
