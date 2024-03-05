using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalsExtensions
{
	public static string GetUsernameOrEmail(this ClaimsPrincipal user)
	{
		return user.FindFirst(ClaimTypes.Name)?.Value;
	}

	public static int GetUserId(this ClaimsPrincipal user)
	{
		return int.Parse(user.FindFirst("id").Value);
	}
}
