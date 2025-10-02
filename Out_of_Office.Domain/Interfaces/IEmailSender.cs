using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
