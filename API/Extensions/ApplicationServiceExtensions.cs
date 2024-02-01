using API.Data;
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
        services.AddDbContext<Data.ApplicationDbContext>(opt => 
        { 
            opt.UseSqlite(connectionString);
        });

        services.AddCors();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }
}
