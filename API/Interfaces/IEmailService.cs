using API.Helpers;
using MimeKit;

namespace API.Interfaces;

public interface IEmailService
{
	Task SendEmailAsync(EmailMessage message);
	Task SendAsync(MimeMessage mailMessage);
	MimeMessage CreateEmailMessage(EmailMessage message);
}