using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Query.GetWorkCalendarByYear
{
    public class GetWorkCalendarByYearQueryHandler : IRequestHandler<GetWorkCalendarByYearQuery, List<WorkCalendarDayDto>>
    {
        private readonly IWorkCalendarRepository _repository;

        public GetWorkCalendarByYearQueryHandler(IWorkCalendarRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WorkCalendarDayDto>> Handle(GetWorkCalendarByYearQuery request, CancellationToken cancellationToken)
        {
            var days = await _repository.GetByYearAsync(request.Year);

            return days.Select(d => new WorkCalendarDayDto
            {
                Date = d.Date,
                IsHoliday = d.IsHoliday,
                Description = d.Description
            }).ToList();
        }
    }

}
