using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Query.GetAvailableCalendarYears
{
    public class GetAvailableCalendarYearsQuery:IRequest<List<int>>
    {
    }
}
