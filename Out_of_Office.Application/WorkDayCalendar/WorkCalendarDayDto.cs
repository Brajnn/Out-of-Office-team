using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.WorkDayCalendar
{
    public class WorkCalendarDayDto
    {
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
        public string? Description { get; set; }
    }
}
