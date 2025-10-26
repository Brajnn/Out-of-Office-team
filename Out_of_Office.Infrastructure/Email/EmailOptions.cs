using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Email;

public sealed class EmailOptions
{
    public string From { get; set; } = "";
    public bool UseFilePickup { get; set; } = false;
    public string PickupDirectory { get; set; } = "/app/Emails";

    public SmtpOptions Smtp { get; set; } = new();
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public bool UseStartTls { get; set; } = true;
    }
}
