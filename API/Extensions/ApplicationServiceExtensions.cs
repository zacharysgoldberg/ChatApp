using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using DotNetEnv;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services,
			IConfiguration config)
	{
		// string connectionString = config.GetConnectionString("SqlConnectionString");

		// System.Console.WriteLine($"---------\n\n{connectionString}\n\n---------" );
		// ADO.NET (SQL authentication) - UseSqlServer
		// services.AddDbContext<DataContext>(opt =>
		// {
		//  opt.UseSqlite(connectionString);
		// });

		services.AddCors();

		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ITokenService, TokenService>();
		services.AddScoped<IPhotoService, PhotoService>();
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IContactRepository, ContactRepository>();
		services.AddScoped<IMessageRepository, MessageRespository>();
		services.AddScoped<IGroupMessageRepository, GroupMessageRepository>();
		services.AddScoped<INotificationRepository, NotificationRepository>();
		services.AddScoped<IEmailService, EmailService>();

		services.AddSignalR();
		services.AddSingleton<PresenceTracker>();

		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		// services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
		services.Configure<CloudinarySettings>(options =>
		{
			options.CloudName = Env.GetString("CLOUDINARY_CLOUD_NAME");
			options.ApiKey = Env.GetString("CLOUDINARY_API_KEY");
			options.ApiSecret = Env.GetString("CLOUDINARY_API_SECRET");
		});

		services.Configure<FormOptions>(opt =>
		{
			opt.ValueLengthLimit = int.MaxValue;
			opt.MultipartBodyLengthLimit = int.MaxValue;
			opt.MemoryBufferThreshold = int.MaxValue;
		});
		// var emailConfig = config.GetSection("EmailConfiguration").Get<EmailConfiguration>();
		EmailConfiguration emailConfig = new()
		{
			From = Environment.GetEnvironmentVariable("EMAIL_CONFIG_FROM"),
			SmtpServer = Environment.GetEnvironmentVariable("EMAIL_CONFIG_SMTP_SERVER"),
			Port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_CONFIG_PORT")),
			UserName = Environment.GetEnvironmentVariable("EMAIL_CONFIG_USERNAME"),
			Password = Environment.GetEnvironmentVariable("EMAIL_CONFIG_PASSWORD")
		};

		services.AddSingleton(emailConfig);


		return services;
	}
}
