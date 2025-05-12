using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Command.DeleteWorkCalendarCommand
{
    public class DeleteWorkCalendarCommandHandler:IRequestHandler<DeleteWorkCalendarCommand>
    {
        private readonly IWorkCalendarRepository _workCalendarRepository;
        private readonly IAuditLogService _auditLogService;
        public DeleteWorkCalendarCommandHandler(IWorkCalendarRepository workCalendarRepository, IAuditLogService auditLogService)
        {
            _workCalendarRepository = workCalendarRepository;
            _auditLogService = auditLogService;
        }


        public async Task<Unit> Handle(DeleteWorkCalendarCommand request, CancellationToken cancellationToken)
        {
            await _workCalendarRepository.DeleteCalendarAsync(request.Year);
            var details = JsonSerializer.Serialize(new
            {
                Year = request.Year
            });
            await _auditLogService.LogAsync("DeleteWorkCalendar", details, cancellationToken);

            return Unit.Value;
        }
    }
}
