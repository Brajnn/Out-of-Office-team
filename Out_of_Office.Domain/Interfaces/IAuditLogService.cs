using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string details, CancellationToken cancellationToken = default);
    }
}
