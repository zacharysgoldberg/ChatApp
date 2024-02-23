using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services,
			IConfiguration config)
	{
		string connectionString = config.GetConnectionString("SqlConnectionString");

		// System.Console.WriteLine($"---------\n\n{connectionString}\n\n---------" );
		// ADO.NET (SQL authentication) - UseSqlServer
		services.AddDbContext<DataContext>(opt =>
		{
			opt.UseSqlite(connectionString);
		});

		services.AddCors();

		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ITokenService, TokenService>();
		services.AddScoped<IPhotoService, PhotoService>();
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IContactRepository, ContactRepository>();
		services.AddScoped<IMessageRepository, MessageRespository>();
		services.AddScoped<IGroupMessageRepository, GroupMessageRepository>();

		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

		return services;
	}
}
