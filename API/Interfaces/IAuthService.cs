using API.DTOs;

namespace API.Interfaces;

public interface IAuthService
{
	Task<(UserDTO, string)> Register(RegisterDTO registerDTO, string role);
	Task<(UserDTO, string)> Authenticate(LoginDTO loginDTO);
	Task<(UserDTO, string)> RefreshToken(UserDTO userDTO);
}