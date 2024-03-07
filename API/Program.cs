using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add startup services to the container.

builder.Services.AddControllers();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

// Use Middleware
var app = builder.Build();

// Confugure the HTTP request pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(builder => builder
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials()
		.WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();


var antiforgery = app.Services.GetRequiredService<IAntiforgery>();

app.Use((context, next) =>
{
	var requestPath = context.Request.Path.Value;

	if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(requestPath, "/index.html", StringComparison.OrdinalIgnoreCase))
	{
		var tokenSet = antiforgery.GetAndStoreTokens(context);
		context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
				new CookieOptions { HttpOnly = false });
	}

	return next(context);
});

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapHub<GroupMessageHub>("hubs/group-message");

// Seed database
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
	var context = services.GetRequiredService<DataContext>();
	var userManager = services.GetRequiredService<UserManager<AppUser>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

	await context.Database.MigrateAsync();
	await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
	var logger = services.GetService<ILogger<Program>>();
	logger.LogError(ex, "An error occurred during migration");
}

app.Run();
