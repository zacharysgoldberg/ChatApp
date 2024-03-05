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

		// Contacts //
		builder.Entity<Contact>()
				.HasOne(c => c.AppUser)
				.WithMany(u => u.Contacts)
				.HasForeignKey(c => c.UserId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

		// Messages //
		builder.Entity<Message>()
				.HasOne(u => u.Sender)
				.WithMany(m => m.MessagesSent)
				.HasForeignKey(u => u.SenderId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Message>()
				.HasOne(u => u.Recipient)
				.WithMany(m => m.MessagesReceived)
				.HasForeignKey(u => u.RecipientId)
				.OnDelete(DeleteBehavior.Restrict);

		// Group Messages //
		builder.Entity<GroupMessage>()
				.HasOne(u => u.Sender)
				.WithMany()
				.HasForeignKey(u => u.SenderId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

		// Notifications //
		builder.Entity<Notification>()
				.HasOne(n => n.Sender)
				.WithMany(u => u.NotificationsSent)
				.HasForeignKey(n => n.SenderId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>()
				.HasOne(n => n.Recipient)
				.WithMany(u => u.NotificationsReceived)
				.HasForeignKey(n => n.RecipientId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>()
				.HasOne(n => n.Message)
				.WithMany(m => m.Notifications)
				.HasForeignKey(n => n.MessageId)
				.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Notification>()
				.HasOne(n => n.GroupMessage)
				.WithMany(gm => gm.Notifications)
				.HasForeignKey(n => n.GroupMessageId)
				.OnDelete(DeleteBehavior.Restrict);
	}
}
