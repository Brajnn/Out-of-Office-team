using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Out_of_Office.Application.Common.Interfaces;
using MailKit.Net.Smtp;


namespace Out_of_Office.Infrastructure.Email;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;
    public SmtpEmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_opt.From));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        var secure = _opt.Smtp.UseStartTls ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.Auto;
        await client.ConnectAsync(_opt.Smtp.Host, _opt.Smtp.Port, secure, ct);

        if (!string.IsNullOrWhiteSpace(_opt.Smtp.User))
            await client.AuthenticateAsync(_opt.Smtp.User, _opt.Smtp.Pass, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}