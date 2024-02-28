using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
	public static IServiceCollection AddIdentityServices(this IServiceCollection services,
			IConfiguration config)
	{
		services.AddIdentity<AppUser, IdentityRole<int>>()
						.AddEntityFrameworkStores<DataContext>()
						.AddDefaultTokenProviders();

		services.Configure<IdentityOptions>(options =>
				{
					// Default User settings.
					options.User.AllowedUserNameCharacters =
									"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
					options.User.RequireUniqueEmail = true;

					// Default Password settings.
					options.Password.RequireUppercase = true;
					options.Password.RequireLowercase = true;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireDigit = true;
					options.Password.RequiredUniqueChars = 1;
					options.Password.RequiredLength = 6;

				});

		services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.
										UTF8.GetBytes(config["JWT:TokenKey"])),
					ValidateIssuerSigningKey = true,
					ValidateAudience = false,
					ValidateIssuer = false,
					ValidIssuer = config["JWT:ValidIssuer"],
					ValidAudience = config["JWT:ValidAudience"],
					ClockSkew = TimeSpan.Zero   // Need this for JWTs to expire
				};

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						var accessToken = context.Request.Query["access_token"];

						var path = context.HttpContext.Request.Path;

						if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
							context.Token = accessToken;

						return Task.CompletedTask;
					}
				};
			});

		services.AddAuthorization(opt =>
		{
			opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
			opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
		});

		return services;
	}
}
