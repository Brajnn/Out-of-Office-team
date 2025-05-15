using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;

using Out_of_Office.Infrastructure.Presistance;
namespace Out_of_Office.Infrastructure.Extensions
{
    public class AuditLogService : IAuditLogService
    {
        private readonly Out_of_OfficeDbContext _context;
        private readonly IUserContext _userContext;

        public AuditLogService(Out_of_OfficeDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task LogAsync(string action, string details, CancellationToken cancellationToken = default)
        {
            var userId = _userContext.GetCurrentApplicationUserId();

            if (string.IsNullOrEmpty(userId))
            {
                userId = "system"; 
            }
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
