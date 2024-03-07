using MimeKit;

namespace API.Helpers;

public class EmailMessage
{
	public List<MailboxAddress> To { get; set; }
	public string Subject { get; set; }
	public string Content { get; set; }

	public EmailMessage(Dictionary<string, string> to, string subject, string content)
	{
		To = new List<MailboxAddress>();

		To.AddRange(to.Select(x => new MailboxAddress(x.Key, x.Value)));

		Subject = subject;
		Content = content;
	}
}