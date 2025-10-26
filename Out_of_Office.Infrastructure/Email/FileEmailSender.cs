using Microsoft.Extensions.Options;
using Out_of_Office.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Email;

internal sealed class FileEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;
    public FileEmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_opt.PickupDirectory);

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");
        var safeTo = to.Replace("@", "_at_").Replace(".", "_");
        var pathHtml = Path.Combine(_opt.PickupDirectory, $"{stamp}_{safeTo}.html");

        var content = new StringBuilder()
            .AppendLine($"<h3>TO: {to}</h3>")
            .AppendLine($"<h4>SUBJECT: {subject}</h4>")
            .AppendLine(htmlBody)
            .ToString();

        await File.WriteAllTextAsync(pathHtml, content, Encoding.UTF8, ct);
    }
}