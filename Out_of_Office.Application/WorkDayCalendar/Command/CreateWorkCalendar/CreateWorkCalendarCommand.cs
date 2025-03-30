using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Command.CreateWorkCalendar
{
    public class CreateWorkCalendarCommand:IRequest
    {
        public int Year { get; set; }
        public List<WorkCalendarDayDto> Days { get; set; }
        public CreateWorkCalendarCommand()
        {
               Days=new List<WorkCalendarDayDto>();
        }
    }
}
