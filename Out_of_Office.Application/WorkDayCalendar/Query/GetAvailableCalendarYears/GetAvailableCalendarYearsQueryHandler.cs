using MediatR;
using Out_of_Office.Application.WorkDayCalendar.Query.GetAvailableCalendarYears;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Query.GetAvailableCalendarYears
{
    public class GetAvailableCalendarYearsQueryHandler : IRequestHandler<GetAvailableCalendarYearsQuery, List<int>>
    {
        private readonly IWorkCalendarRepository _repository;

        public GetAvailableCalendarYearsQueryHandler(IWorkCalendarRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<int>> Handle(GetAvailableCalendarYearsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAvailableYearsAsync();
        }
    }

}

