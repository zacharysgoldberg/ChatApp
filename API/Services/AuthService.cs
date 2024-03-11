using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;

namespace API.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepository;
	private readonly RoleManager<IdentityRole<int>> _roleManager;
	private readonly ITokenService _tokenService;
	private readonly IConfiguration _config;

	public AuthService(IUserRepository userRepository, RoleManager<IdentityRole<int>> roleManager,
		ITokenService tokenService, IConfiguration config)
	{
		_userRepository = userRepository;
		_roleManager = roleManager;
		_tokenService = tokenService;
		_config = config;
	}

	public async Task<(UserDTO, string)> Register(RegisterDTO registerDTO, string role)
	{
		var user = new AppUser()
		{
			Email = registerDTO.Email.ToLower(),
			UserName = registerDTO.Email.ToLower(),
			SecurityStamp = Guid.NewGuid().ToString(),
		};

		IdentityResult createUserResult = await _userRepository.CreateUserAsync(user, registerDTO.Password);

		if (!createUserResult.Succeeded)
			return (null, "Registration Failed");

		if (!await _roleManager.RoleExistsAsync(role))
			await _roleManager.CreateAsync(new IdentityRole<int>(role));

		if (await _roleManager.RoleExistsAsync(UserRoles.Member))
		{
			IdentityResult addRoleToUserResult = await _userRepository.AddRoleToUserAsync(user, role);

			if (!addRoleToUserResult.Succeeded)
				return (null, "Role Assignment Failed");
		}

		string accessToken = await _tokenService.GenerateAccessToken(user);
		string refreshToken = _tokenService.GenerateRefreshToken();
		string refreshTokenValidityString = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_VALIDITY");
		bool castValidRefreshToken = int.TryParse(refreshTokenValidityString, out int
																							refreshTokenValidityInt);

		if (!castValidRefreshToken)
			return (null, "Failed to convert refresh token validity into a 32-bit signed integer");

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInt);
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		if (!updateUserResult.Succeeded)
			return (null, "User Update Failed");

		return (new UserDTO
		{
			Username = user.UserName,
			AccessToken = accessToken,
			RefreshToken = refreshToken
		}, null);
	}

	public async Task<(UserDTO, string)> Authenticate(LoginDTO loginDTO)
	{
		AppUser user = await _userRepository.GetUserAsync(loginDTO.Username);

		if (user == null)
			return (null, "Invalid Username or Password");

		if (!await _userRepository.PasswordMatchesAsync(user, loginDTO.Password))
			return (null, "Invalid Username or Password");

		string accessToken = await _tokenService.GenerateAccessToken(user);
		string refreshToken = _tokenService.GenerateRefreshToken();
		string refreshTokenValidityString = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_VALIDITY");
		bool castValidRefreshToken = int.TryParse(refreshTokenValidityString, out int
																							refreshTokenValidityInt);

		if (!castValidRefreshToken)
			return (null, "Failed to convert refresh token validity into a 32-bit signed integer");

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenValidityInt);
		user.LastActive = DateTime.UtcNow;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		if (!updateUserResult.Succeeded)
			return (null, "User Update Failed");

		return (new UserDTO
		{
			Username = user.UserName,
			AccessToken = accessToken,
			RefreshToken = refreshToken
		}, null);
	}

	public async Task<(UserDTO, string)> RefreshToken(UserDTO userDTO)
	{
		string accessToken = userDTO.AccessToken;
		string refreshToken = userDTO.RefreshToken;
		ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(ref accessToken);
		AppUser user = await _userRepository.GetUserAsync(principal.Identity.Name);

		if (user == null ||
				user.RefreshToken != refreshToken ||
				user.RefreshTokenExpiryTime <= DateTime.UtcNow)
			return (null, "Invalid access token or refresh token");

		List<Claim> claims = principal.Claims.ToList();
		string newAccessToken = await _tokenService.GenerateAccessToken(user, claims);
		string newRefreshToken = _tokenService.GenerateRefreshToken();
		user.RefreshToken = newRefreshToken;
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		if (!updateUserResult.Succeeded)
			return (null, "User Update Failed");

		return (new UserDTO()
		{
			Username = user.UserName,
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		}, null);
	}
}