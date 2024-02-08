using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserContact>()
            .HasKey(uc => new { uc.AppUserId, uc.ContactId });

        builder.Entity<UserContact>()
            .HasOne(uc => uc.AppUser)
            .WithMany(u => u.UserContacts)
            .HasForeignKey(uc => uc.AppUserId);

        builder.Entity<UserContact>()
            .HasOne(uc => uc.Contact)
            .WithMany()
            .HasForeignKey(uc => uc.ContactId);
    }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<UserContact> UserContacts { get; set; }
    
    // public DbSet<Entities.Message>           Message {get; set;}
    // public DbSet<Entities.Notification>      Notification {get; set;}
}
