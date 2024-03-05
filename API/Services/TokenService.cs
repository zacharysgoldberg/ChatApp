using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
	private readonly SymmetricSecurityKey _key;
	private readonly UserManager<AppUser> _userManager;


	public TokenService(IConfiguration config, UserManager<AppUser> userManager)
	{
		_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:TokenKey"]));
		_userManager = userManager;
	}

	// Create a JWT and return it as a string
	public async Task<string> GenerateAccessToken(AppUser user, List<Claim> claims = null)
	{
		if (claims == null)
		{
			claims = new List<Claim>()
						{
								new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
								new Claim("id", user.Id.ToString()),
								new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
						};
		}

		var roles = await _userManager.GetRolesAsync(user);

		claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

		int minutes = roles.Contains("Admin") ? 60 : 5;

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.Now.AddMinutes(minutes),
			NotBefore = DateTime.Now,
			SigningCredentials = creds,
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

		return tokenHandler.WriteToken(token);
	}

	// Create and return a random string of characters in base 64
	public string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	// Validate the incomming expired token and return it's principal claim
	public ClaimsPrincipal GetPrincipalFromExpiredToken(ref string token)
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

		SecurityToken securityToken;

		var tokenHandler = new JwtSecurityTokenHandler();
		ClaimsPrincipal principal = tokenHandler.ValidateToken(token,
																													 tokenValidationParameters,
																													 out securityToken);

		JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
		bool isSecurityTokenHmacSha512 = jwtSecurityToken.Header.Alg.Equals(
																						SecurityAlgorithms.HmacSha512,
																						StringComparison.InvariantCultureIgnoreCase
																						);

		if (!isSecurityTokenHmacSha512 || jwtSecurityToken == null)
			throw new SecurityTokenException("Invalid token");

		return principal;
	}
}
