using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
string connectionString = builder.Configuration.GetConnectionString("SqlConnectionString");
// System.Console.WriteLine($"---------\n\n{connectionString}\n\n---------" );
// ADO.NET (SQL authentication) - UseSqlServer
builder.Services.AddDbContext<API.Data.DataContext>(opt => 
    opt.UseSqlite(connectionString)
);

builder.Services.AddCors();

var app = builder.Build();

// Confugure the HTTP request pipeline
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
