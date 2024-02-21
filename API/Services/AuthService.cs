using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;

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

		bool createdUser = await _userRepository.CreateUserAsync(user, registerDTO.Password);

		if (!createdUser)
			return (null, "User registration failed");

		if (!await _roleManager.RoleExistsAsync(role))
			await _roleManager.CreateAsync(new IdentityRole<int>(role));

		if (await _roleManager.RoleExistsAsync(UserRolesDTO.Member))
		{
			bool addedRole = await _userRepository.AddRoleToUserAsync(user, role);
			if (!addedRole)
				return (null, "Failed to add role to user");
		}

		string accessToken = await _tokenService.GenerateAccessToken(user);
		string refreshToken = _tokenService.GenerateRefreshToken();
		bool parsedValidRefreshToken = int.TryParse(_config["JWT:RefreshTokenValidityInDays"],
																								out int refreshTokenValidityInDays);

		if (!parsedValidRefreshToken)
			return (null, "Failed to convert refresh token validity into a 32-bit signed integer");

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

		bool updatedUser = await _userRepository.UpdateUserAsync(user);

		if (!updatedUser)
			return (null, "Failed to update user");

		return (new UserDTO
		{
			Username = user.UserName,
			AccessToken = accessToken,
			RefreshToken = refreshToken
		}, "User created successfully");
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
		bool parsedValidRefreshToken = int.TryParse(_config["JWT:RefreshTokenValidityInDays"],
																								out int refreshTokenValidityInDays);

		if (!parsedValidRefreshToken)
			return (null, "Failed to convert refresh token validity into a 32-bit signed integer");

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
		user.LastActive = DateTime.UtcNow;

		bool updatedUser = await _userRepository.UpdateUserAsync(user);

		if (!updatedUser)
			return (null, "Failed to update user");

		return (new UserDTO
		{
			Username = user.UserName,
			AccessToken = accessToken,
			RefreshToken = refreshToken
		}, "User signed in successfully");
	}

	public async Task<(UserDTO, string)> RefreshToken(UserDTO userDTO)
	{
		string accessToken = userDTO.AccessToken;
		string refreshToken = userDTO.RefreshToken;
		ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
		string usernameOrEmail = principal.Identity.Name;

		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null ||
				user.RefreshToken != refreshToken ||
				user.RefreshTokenExpiryTime <= DateTime.Now)
			return (null, "Invalid access token or refresh token");

		List<Claim> claims = principal.Claims.ToList();
		string newAccessToken = await _tokenService.GenerateAccessToken(user, claims);
		string newRefreshToken = _tokenService.GenerateRefreshToken();
		user.RefreshToken = newRefreshToken;

		bool updatedUser = await _userRepository.UpdateUserAsync(user);

		if (!updatedUser)
			return (null, "Failed to update user");

		return (new UserDTO()
		{
			Username = user.UserName,
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		}, "Refreshed tokens successfully");
	}
}