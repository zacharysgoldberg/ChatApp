using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    public DbSet<Entities.AppUser>      Users {get; set;}
    // public DbSet<Entities.Message>      Messages {get; set;}
    // public DbSet<Entities.GroupMessage> GroupMessages {get; set;}
}
