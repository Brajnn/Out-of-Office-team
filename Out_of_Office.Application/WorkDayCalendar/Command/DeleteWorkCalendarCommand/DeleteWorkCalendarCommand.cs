using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar.Command.DeleteWorkCalendarCommand
{
    public class DeleteWorkCalendarCommand:IRequest
    {
        public int Year { get; set; }
    }
}
