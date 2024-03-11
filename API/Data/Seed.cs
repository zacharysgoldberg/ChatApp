using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
	// public static async Task ClearConnections(DataContext context)
	// {
	// 	context.Connections.RemoveRange(context.Connections);
	// 	await context.SaveChangesAsync();
	// }

	public static async Task SeedUsers(UserManager<AppUser> userManager,
		RoleManager<IdentityRole<int>> roleManager)
	{
		if (await userManager.Users.AnyAsync())
			return;

		var roles = new List<IdentityRole<int>>
		{
			new("Admin"),
			new("Member"),
			new("Moderator"),
		};

		foreach (var role in roles)
		{
			await roleManager.CreateAsync(role);
		}


		var user = new AppUser()
		{

			Email = "zach@test.com",
			UserName = "User",
			SecurityStamp = Guid.NewGuid().ToString()

		};

		await userManager.CreateAsync(user, "Password123");
		await userManager.AddToRoleAsync(user, "Member");

		var admin = new AppUser()
		{

			Email = "zachgoldberg29@gmail.com",
			UserName = "Admin",
			SecurityStamp = Guid.NewGuid().ToString()
		};

		await userManager.CreateAsync(admin, "Password123");
		await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
	}
}
