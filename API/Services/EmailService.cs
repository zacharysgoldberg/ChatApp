using MailKit.Net.Smtp;
using API.Helpers;
using API.Interfaces;
using MimeKit;
using MailKit.Security;


namespace API.Services;

public class EmailService : IEmailService
{
	private readonly EmailConfiguration _emailConfig;
	public EmailService(EmailConfiguration emailConfig)
	{
		_emailConfig = emailConfig;
	}

	public async Task SendEmailAsync(EmailMessage message)
	{
		var mailMessage = CreateEmailMessage(message);

		await SendAsync(mailMessage);
	}

	public async Task SendAsync(MimeMessage mailMessage)
	{
		using var client = new SmtpClient();

		{
			try
			{
				await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
				client.AuthenticationMechanisms.Remove("XOAUTH2");
				await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

				await client.SendAsync(mailMessage);
			}
			catch
			{
				//log an error message or throw an exception, or both.
				throw;
			}
			finally
			{
				await client.DisconnectAsync(true);
				client.Dispose();
			}
		}
	}

	public MimeMessage CreateEmailMessage(EmailMessage message)
	{
		var emailMessage = new MimeMessage();
		emailMessage.From.Add(new MailboxAddress(_emailConfig.UserName, _emailConfig.From));
		emailMessage.To.AddRange(message.To);
		emailMessage.Subject = message.Subject;
		emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
		{ Text = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };

		return emailMessage;
	}
}