using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Query.GetWorkCalendarByYear
{
    public class GetWorkCalendarByYearQuery:IRequest<List<WorkCalendarDayDto>>
    {
        public int Year { get; set; }
    }
}
