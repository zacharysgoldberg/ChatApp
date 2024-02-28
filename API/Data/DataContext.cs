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
	public DbSet<Notification> Notifications { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// builder.Entity<UserContact>()
		// 		.HasKey(uc => new { uc.AppUserId, uc.ContactId });

		// builder.Entity<UserContact>()
		// 		.HasOne(uc => uc.AppUser)
		// 		.WithMany(u => u.Contacts)
		// 		.HasForeignKey(uc => uc.AppUserId)
		// 		.OnDelete(DeleteBehavior.Cascade);

		// builder.Entity<UserContact>()
		// 		.HasOne(uc => uc.Contact)
		// 		.WithMany()
		// 		.HasForeignKey(uc => uc.ContactId);

		// Contacts //

		builder.Entity<Contact>()
				.HasOne(c => c.AppUser)
				.WithMany(u => u.Contacts)
				.HasForeignKey(c => c.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

		// Messages //

		builder.Entity<Message>()
				.HasOne(u => u.Recipient)
				.WithMany(m => m.MessagesReceived)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Message>()
				.HasOne(u => u.Sender)
				.WithMany(m => m.MessagesSent)
				.OnDelete(DeleteBehavior.Restrict);

		// Group Messages //

		builder.Entity<GroupMessage>()
				.HasOne(gm => gm.Sender)
				.WithMany()
				.HasForeignKey(gm => gm.SenderId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

		// Notifications //

		builder.Entity<Notification>()
				.HasOne(n => n.Recipient)
				.WithMany(u => u.NotificationsReceived)
				.HasForeignKey(n => n.RecipientId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>()
				.HasOne(n => n.Sender)
				.WithMany(u => u.NotificationsSent)
				.HasForeignKey(n => n.SenderId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>()
				.HasOne(n => n.Channel)
				.WithMany(gm => gm.Notifications)
				.HasForeignKey(n => n.ChannelId)
				.OnDelete(DeleteBehavior.Restrict);
	}
}
