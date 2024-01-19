using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:TokenKey"]));
    }

    // Create a JWT and return it as a string
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(2),
            NotBefore = DateTime.Now,
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    // Create and return a random number in base 64
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Validate the incomming token and return principal owner of token
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = _key,
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            // Here we're saying that we don't care about the token's expiration date/time
            ValidateLifetime = false 
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken securityToken;

        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, 
                                                               tokenValidationParameters, 
                                                               out securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                                                StringComparison.InvariantCultureIgnoreCase) || jwtSecurityToken == null)
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
