using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) { }

	public DbSet<Contact> Contacts { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<GroupMessage> GroupMessages { get; set; }

	// public DbSet<Entities.Notification>      Notification {get; set;}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// builder.Entity<GroupMessage>()
		// 		.HasKey(uc => new { uc.AppUserId, uc.ContactId });

		// builder.Entity<GroupMessage>()
		// 		.HasOne(uc => uc.AppUser)
		// 		.WithMany(u => u.UserContacts)
		// 		.HasForeignKey(uc => uc.AppUserId)
		// 		.OnDelete(DeleteBehavior.Cascade);

		// builder.Entity<UserContact>()
		// 		.HasOne(uc => uc.Contact)
		// 		.WithMany()
		// 		.HasForeignKey(uc => uc.ContactId);

		builder.Entity<Contact>()
				.HasOne(c => c.AppUser)
				.WithMany(u => u.Contacts)
				.HasForeignKey(c => c.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<Message>()
				.HasOne(u => u.Recipient)
				.WithMany(m => m.MessagesReceived)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Message>()
				.HasOne(u => u.Sender)
				.WithMany(m => m.MessagesSent)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<GroupMessage>()
				.HasOne(gm => gm.Sender)
				.WithMany()  // Assuming a channel can have multiple group messages
				.HasForeignKey(gm => gm.SenderId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
	}
}
