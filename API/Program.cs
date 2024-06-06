using API.Data;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DotNetEnv;
using Microsoft.AspNetCore.Rewrite;

Env.Load();
Env.TraversePath().Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add startup services to the container.

builder.Services.AddControllers();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

string connectionString = "";

if (builder.Environment.IsDevelopment())
	connectionString = builder.Configuration.GetConnectionString("SqlConnectionString");
else
{
	string pgHost = Environment.GetEnvironmentVariable("PG_HOST");
	string pgPort = Environment.GetEnvironmentVariable("PG_PORT");
	string pgUser = Environment.GetEnvironmentVariable("PG_USER");
	string pgPassword = Environment.GetEnvironmentVariable("PG_PASSWORD");
	string pgDb = Environment.GetEnvironmentVariable("PG_DATABASE");

	connectionString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPassword};Database={pgDb};";
}

builder.Services.AddDbContext<DataContext>(opt =>
{
	opt.UseNpgsql(connectionString);
});

// Use Middleware
WebApplication app = builder.Build();

// Confugure the HTTP request pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseCors(builder => builder
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials()
		.AllowAnyOrigin()
		.WithOrigins("https://localhost:4200")
		);

app.UseAuthentication();
app.UseAuthorization();

// URL rewriting to handle Angular routes (ie. /reset-password)
app.UseRewriter(new RewriteOptions()
						.AddRewrite(@"^reset-password.*", "/index.html", skipRemainingRules: true));

app.UseDefaultFiles();
app.UseStaticFiles();

IAntiforgery antiforgery = app.Services.GetRequiredService<IAntiforgery>();

app.Use((context, next) =>
{
	string requestPath = context.Request.Path.Value;

	if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(requestPath, "/index.html", StringComparison.OrdinalIgnoreCase))
	{
		AntiforgeryTokenSet tokenSet = antiforgery.GetAndStoreTokens(context);
		context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
				new CookieOptions { HttpOnly = false });
	}

	return next(context);
});

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapHub<GroupMessageHub>("hubs/group-message");
app.MapFallbackToController("Index", "Fallback");

// Seed database
using IServiceScope scope = app.Services.CreateScope();
IServiceProvider services = scope.ServiceProvider;

try
{
	DataContext context = services.GetRequiredService<DataContext>();
	UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
	RoleManager<IdentityRole<int>> roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

	await context.Database.MigrateAsync();
	await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
	ILogger<Program> logger = services.GetService<ILogger<Program>>();
	logger.LogError(ex, "An error occurred during migration");
}

app.Run();
