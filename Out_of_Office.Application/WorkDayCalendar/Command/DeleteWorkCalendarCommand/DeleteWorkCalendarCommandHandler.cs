using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Command.DeleteWorkCalendarCommand
{
    public class DeleteWorkCalendarCommandHandler:IRequestHandler<DeleteWorkCalendarCommand>
    {
        private readonly IWorkCalendarRepository _workCalendarRepository;
        public DeleteWorkCalendarCommandHandler(IWorkCalendarRepository workCalendarRepository)
        {
            _workCalendarRepository = workCalendarRepository;
        }


        public async Task<Unit> Handle(DeleteWorkCalendarCommand request, CancellationToken cancellationToken)
        {
            await _workCalendarRepository.DeleteCalendarAsync(request.Year);
            return Unit.Value;
        }
    }
}
