using MediatR;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Application.WorkDayCalendar;
using Out_of_Office.Application.WorkDayCalendar.Command.CreateWorkCalendar;
using Out_of_Office.Application.WorkDayCalendar.Query.GetAvailableCalendarYears;
using Out_of_Office.Application.WorkDayCalendar.Query.GetWorkCalendarByYear;

namespace Out_of_Office.Controllers
{
    public class WorkCalendarController : Controller
    {
        private readonly IMediator _mediator;

        public WorkCalendarController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //GET: /WorkCalendar
       [HttpGet]
        public async Task<IActionResult> Index()
        {
            var years = await _mediator.Send(new GetAvailableCalendarYearsQuery());
            return View(years);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int year)
        {
            var days = await _mediator.Send(new GetWorkCalendarByYearQuery { Year = year });
            ViewData["Year"] = year;
            return View("Details", days);
        }


        // GET: /WorkCalendar/Create
        [HttpGet]
        public IActionResult Create(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            var command = new CreateWorkCalendarCommand
            {
                Year = selectedYear,
                Days = Enumerable.Range(0, DateTime.IsLeapYear(selectedYear) ? 366 : 365)
                    .Select(i =>
                    {
                        var date = new DateTime(selectedYear, 1, 1).AddDays(i);
                        return new WorkCalendarDayDto
                        {
                            Date = date,
                            IsHoliday = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                            Description = null
                        };
                    }).ToList()
            };
            Console.WriteLine($"GET Create loaded. Year: {command.Year}, Days: {command.Days?.Count}");
            return View(command);
        }

        // POST: /WorkCalendar/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Year,Days")] CreateWorkCalendarCommand command)
        {

            if (ModelState.IsValid)
            {
                await _mediator.Send(command);
                TempData["Success"] = "Calendar successfully created.";
                return RedirectToAction(nameof(Index));
            }

            int year = command.Year == 0 ? DateTime.Now.Year : command.Year;
            if (command.Days == null || command.Days.Count == 0)
            {
                command.Days = Enumerable.Range(0, DateTime.IsLeapYear(year) ? 366 : 365)
                    .Select(i =>
                    {
                        var date = new DateTime(year, 1, 1).AddDays(i);
                        return new WorkCalendarDayDto
                        {
                            Date = date,
                            IsHoliday = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                            Description = null
                        };
                    }).ToList();
            }

            return View(command);
        }

    }
}
