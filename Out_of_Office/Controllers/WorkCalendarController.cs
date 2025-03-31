using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Application.WorkDayCalendar;
using Out_of_Office.Application.WorkDayCalendar.Command.CreateWorkCalendar;
using Out_of_Office.Application.WorkDayCalendar.Command.DeleteWorkCalendarCommand;
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

        [HttpPost]
        public async Task<IActionResult> Delete(int year)
        {
            await _mediator.Send(new DeleteWorkCalendarCommand { Year = year });
            TempData["Success"] = $"Calendar for {year} was deleted.";
            return RedirectToAction("Index");
        }

        // GET: /WorkCalendar/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? year)
        {
            var usedYears = await _mediator.Send(new GetAvailableCalendarYearsQuery());
            var allYears = Enumerable.Range(DateTime.Now.Year, 6); 
            var availableYears = allYears.Except(usedYears).ToList();

            int selectedYear = year ?? availableYears.FirstOrDefault();

            if (!availableYears.Contains(selectedYear))
            {
                return RedirectToAction("Index");
            }

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

            ViewBag.AvailableYears = availableYears;
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
