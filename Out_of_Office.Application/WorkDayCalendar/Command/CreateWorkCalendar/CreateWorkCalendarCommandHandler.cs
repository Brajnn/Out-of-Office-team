using MediatR;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Command.CreateWorkCalendar
{
    public class CreateWorkCalendarCommandHandler : IRequestHandler<CreateWorkCalendarCommand>
    {
        private readonly IWorkCalendarRepository _calendarRepository;

        public CreateWorkCalendarCommandHandler(IWorkCalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(CreateWorkCalendarCommand request, CancellationToken cancellationToken)
        {
            var exists = await _calendarRepository.ExistsAsync(request.Year);
            if (exists)
                throw new InvalidOperationException("Calendar for this year already exists.");

            var days = request.Days.Select(d => new WorkCalendarDay
            {
                Date = d.Date,
                IsHoliday = d.IsHoliday,
                Description = d.Description
            }).ToList();

            await _calendarRepository.AddCalendarAsync(request.Year, days);

            return Unit.Value;
        }
    }
}
