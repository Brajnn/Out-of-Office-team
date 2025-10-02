using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Out_of_Office.Domain.Interfaces;
using System.Threading.Tasks;
namespace Out_of_Office.Infrastructure.Extensions;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {

        var smtpSettings = _configuration.GetSection("EmailSettings");
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            smtpSettings["FromName"],
            smtpSettings["FromEmail"]));

        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlMessage };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpSettings["SmtpServer"], int.Parse(smtpSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);


        await client.AuthenticateAsync(
            smtpSettings["SmtpUser"],
            smtpSettings["SmtpPass"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
