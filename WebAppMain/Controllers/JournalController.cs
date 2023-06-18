using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebAppMain.Data;
using WebAppMain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAppMain.Controllers
{
    public class JournalController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JournalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string id, [FromQuery] int? quarter)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var journal = await _context.Journal
                .Include(j => j.JournalPredmets)
                    .ThenInclude(jp => jp.Predmet)
                .Include(j => j.JournalPredmets)
                    .ThenInclude(jp => jp.Estimations)
                .FirstOrDefaultAsync(j => j.UserId == id);

            if (journal == null)
            {
                return NotFound();
            }
            if (!quarter.HasValue)
            {
                quarter = 1;
            }
            // Фильтрация оценок по четверти
            if (quarter.HasValue)
            {
                int year = 2022;
                var startDate = GetStartDateForQuarter(quarter.Value);
                var endDate = GetEndDateForQuarter(quarter.Value);

                journal.JournalPredmets = journal.JournalPredmets.Select(jp =>
                {
                    jp.Estimations = jp.Estimations.Where(e => e.Date >= startDate && e.Date <= endDate).ToList();
                    return jp;
                }).ToList();
            }
            int selectedQuarter = quarter ?? 1; // Устанавливаем значение по умолчанию (1 четверть), если четверть не указан
            ViewBag.SelectedQuarter = selectedQuarter; // Сохраняем выбранное значение в состоянии ViewBag

            var viewModel = new JournalViewModel
            {
                User = user,
                Journal = journal,
                AverageGradePerSubject = new Dictionary<string, double>()
            };

            foreach (var jp in viewModel.Journal.JournalPredmets)
            {
                double sumGrades = 0;
                int countGrades = 0;

                foreach (var est in jp.Estimations)
                {
                    sumGrades += est.Value;
                    countGrades++;
                }

                double averageGrade = countGrades > 0 ? sumGrades / countGrades : 0;

                viewModel.AverageGradePerSubject[jp.Predmet.Title_Predmet] = averageGrade;
            }

            return View(viewModel);
        }

        private DateTime GetStartDateForQuarter(int quarter)
        {
            int year = 2022;

            switch (quarter)
            {
                case 1:
                    return new DateTime(year, 9, 1);
                case 2:
                    return new DateTime(year, 11, 7);
                case 3:
                    return new DateTime(year + 1, 1, 10);
                case 4:
                    return new DateTime(year + 1, 3, 27);
                default:
                    return DateTime.MinValue;
            }
        }

        private DateTime GetEndDateForQuarter(int quarter)
        {
            int year = 2022;

            switch (quarter)
            {
                case 1:
                    return new DateTime(year, 10, 28);
                case 2:
                    return new DateTime(year, 12, 30);
                case 3:
                    return new DateTime(year + 1, 3, 6);
                case 4:
                    return new DateTime(year + 1, 5, 29);
                default:
                    return DateTime.MaxValue;
            }
        }


        public async Task<IActionResult> AddPredmet(string id)
        {
            var viewModel = new AddPredmetViewModel
            {
                UserId = id,
                Predmets = await _context.Predmet.ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Journal/AddPredmet

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPredmet(AddPredmetViewModel viewModel, string id)
        {
            if (ModelState.IsValid)
            {
                var journal = await _context.Journal
                    .Include(j => j.JournalPredmets)
                    .FirstOrDefaultAsync(j => j.UserId == id);

                if (journal == null)
                {
                    return Content("не наследуется");
                }

                var predmet = await _context.Predmet.FindAsync(viewModel.PredmetId);

                if (journal.JournalPredmets.Any(jp => jp.PredmetId == viewModel.PredmetId))
                {
                    TempData["SuccessMessage"] = "Такой предмет уже добавлен.";
                    viewModel.Predmets = await _context.Predmet.ToListAsync();
                    return View(viewModel);
                }

                var journalPredmet = new JournalPredmet
                {
                    Journal = journal,
                    Predmet = predmet
                };

                _context.JournalPredmet.Add(journalPredmet);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id = viewModel.UserId });
            }

            viewModel.Predmets = await _context.Predmet.ToListAsync();
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult AddEstimation(int journalPredmetId)
        {
            Response.Cookies.Append("PreviousPageUrl", Request.Headers["Referer"].ToString());

            var viewModel = new AddEstimationViewModel
            {
                JournalPredmetId = journalPredmetId,
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEstimation(AddEstimationViewModel viewModel, string userId)
        {
            if (ModelState.IsValid)
            {
                var journalPredmet = await _context.JournalPredmet.FindAsync(viewModel.JournalPredmetId);

                if (viewModel.Value < 2 || viewModel.Value > 5)
                {
                    TempData["SuccessMessage"] = "Оценка должна быть от 2 до 5.";
                    return View(viewModel);
                }
                if (journalPredmet == null)
                {
                    return NotFound();
                }

                int year = 2022;
                int quarter = GetQuarterFromDate(viewModel.Date, year);

                if (quarter == 0)
                {
                    TempData["ErrorMessage"] = "Дата выходит за рамки дат четвертей. Сейчас каникулы.";
                    return View(viewModel);
                }

                var estimation = new Estimation
                {
                    Value = viewModel.Value,
                    Date = viewModel.Date,
                    JournalPredmet = journalPredmet
                };


                _context.Estimation.Add(estimation);
                await _context.SaveChangesAsync();

                var notificationController = new NotificationController(_context, _userManager);
                notificationController.CreateEstimationNotification(journalPredmet.Id);

                // получаем URL предыдущей страницы из куки
                if (Request.Cookies.TryGetValue("PreviousPageUrl", out string previousPageUrl))
                {
                    // удаляем куки с URL предыдущей страницы
                    Response.Cookies.Delete("PreviousPageUrl");
                    // возвращаем пользователя на предыдущую страницу
                    return Redirect(previousPageUrl);
                }

                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        private int GetQuarterFromDate(DateTime date, int year)
        {
            if (date >= new DateTime(year, 9, 1) && date <= new DateTime(year, 10, 28))
            {
                return 1;
            }
            else if (date >= new DateTime(year, 11, 7) && date <= new DateTime(year, 12, 30))
            {
                return 2;
            }
            else if (date >= new DateTime(year, 12, 9) && date <= new DateTime(year + 1, 3, 24))
            {
                return 3;
            }
            else if (date >= new DateTime(year + 1, 3, 27) && date <= new DateTime(year + 1, 5, 29))
            {
                return 4;
            }
            else
            {
                return 0; // 0 означает, что дата не входит в диапазон дат четвертей
            }
        }

        [HttpGet]
        public async Task<IActionResult> Estimations(int journalPredmetId, int quarter)
        {
            var journalPredmet = await _context.JournalPredmet
                .Include(jp => jp.Journal)
                    .ThenInclude(g => g.User)
                .Include(jp => jp.Predmet)
                .Include(jp => jp.Estimations)
                .FirstOrDefaultAsync(jp => jp.Id == journalPredmetId);

            if (journalPredmet == null)
            {
                return NotFound();
            }
            var startDate = GetStartDateForQuarter(quarter);
            var endDate = GetEndDateForQuarter(quarter);

            // Фильтруем оценки по дате, чтобы получить только те, которые попадают в диапазон выбранной четверти
            var estimations = journalPredmet.Estimations.Where(e => e.Date >= startDate && e.Date <= endDate).ToList();

            // Создаем новый объект модели представления и передаем ему отфильтрованные оценки
            var model = new EstimationsViewModel
            {
                JournalPredmet = journalPredmet,
                Estimations = estimations
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditEstimation(Estimation estimation)
        {
            if (!ModelState.IsValid)
            {
                return View(estimation);
            }

            var journalPredmet = await _context.JournalPredmet.FindAsync(estimation.JournalPredmetId);

            if (journalPredmet == null)
            {
                return NotFound();
            }

            _context.Estimation.Update(estimation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Estimations", "Journal", new { journalPredmetId = estimation.JournalPredmetId });
        }

        [HttpGet]
        public async Task<IActionResult> EditEstimation(int estimationId)
        {
            var estimation = await _context.Estimation.FindAsync(estimationId);

            if (estimation == null)
            {
                return NotFound();
            }

            return View(estimation);
        }
    }
}
