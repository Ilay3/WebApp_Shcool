using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAppMain.Data;
using WebAppMain.Models;


namespace WebAppMain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }


        public async Task<IActionResult> Index(string userId, string code)
        {


            if (userId == null || code == null)
            {
                return View();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)

                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendFeedback(string message, string email, string name, string subject)
        {
            var emails = "ilyaplekhanoff@yandex.ru";
            var emailService = new EmailService();
            await emailService.SendEmailAsync(emails, subject, message + "<br><br>Предложение от пользователя: <br>" + email + " " + name);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        public async Task<IActionResult> UserJournal(string id, [FromQuery] int? quarter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var journal = await _context.Journal
                .Include(j => j.JournalPredmets)
                .ThenInclude(jp => jp.Predmet)
                .Include(j => j.JournalPredmets)
                .ThenInclude(jp => jp.Estimations)
                .FirstOrDefaultAsync(j => j.UserId == userId);

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
            int selectedQuarter = quarter ?? 1; // Устанавливаем значение по умолчанию (1st Quarter), если quarter не указан
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


        public async Task<IActionResult> DownloadPDFAsync(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var journal = await _context.Journal
                .Include(j => j.JournalPredmets)
                .ThenInclude(jp => jp.Predmet)
                .Include(j => j.JournalPredmets)
                .ThenInclude(jp => jp.Estimations)
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (journal == null)
            {
                return NotFound();
            }

            var viewModel = new JournalViewModel
            {
                User = user,
                Journal = journal,
                AverageGradePerSubject = new Dictionary<string, double>()
            };

            // Создание HTML-разметки для таблицы
            var html = new StringBuilder();
            html.Append("<html><head>");
            html.Append("<style>");
            html.Append("body { font-family: Arial, sans-serif; }");
            html.Append("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            html.Append("th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.Append("th { background-color: #f2f2f2; font-weight: bold; }");
            html.Append("td { color: #555; }");
            html.Append("h3 { margin-bottom: 10px; }");
            html.Append("</style>");
            html.Append("</head><body>");

            html.Append($"<h2>Журнал оценок: {viewModel.User.Surname} {viewModel.User.Name} {viewModel.User.Patronymic} </h2>");

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var startDate = GetStartDateForQuarter(quarter);
                var endDate = GetEndDateForQuarter(quarter);

                html.Append($"<h3>{GetQuarterTitle(quarter)}</h3>");

                html.Append("<table>");
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append("<th>Предмет</th>");
                html.Append("<th>Оценки</th>");
                html.Append("<th>Средний балл</th>");
                html.Append("</tr>");
                html.Append("</thead>");
                html.Append("<tbody>");

                foreach (var jp in viewModel.Journal.JournalPredmets)
                {
                    var filteredEstimations = jp.Estimations.Where(e => e.Date >= startDate && e.Date <= endDate).ToList();

                    html.Append("<tr>");
                    html.Append($"<td>{jp.Predmet.Title_Predmet}</td>");

                    var estimations = new StringBuilder();
                    foreach (var est in filteredEstimations)
                    {
                        estimations.Append($"{est.Value}, ");
                    }
                    html.Append($"<td>{estimations.ToString().TrimEnd(' ', ',')}</td>");

                    var sumGrades = filteredEstimations.Sum(est => est.Value);
                    var countGrades = filteredEstimations.Count;
                    var averageGrade = countGrades > 0 ? (double)sumGrades / countGrades : 0;

                    html.Append($"<td>{averageGrade.ToString("0.0")}</td>");

                    html.Append("</tr>");
                }

                html.Append("</tbody>");
                html.Append("</table>");
            }

            html.Append("</body></html>");


            // Создание PDF документа из HTML
            var renderer = new IronPdf.HtmlToPdf();
            var pdf = renderer.RenderHtmlAsPdf(html.ToString());

            // Генерация уникального имени файла PDF на основе текущей даты и времени
            var fileName = $"оценки_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            // Получение содержимого PDF в виде массива байтов
            var fileContent = pdf.BinaryData;

            // Возврат скачиваемого файла в браузере
            return File(fileContent, "application/pdf", fileName);
        }

        private string GetQuarterTitle(int quarter)
        {
            // Замените этот код соответствующей логикой для определения заголовка четверти на основе даты
            // В данном примере заголовки четвертей заданы жестко
            switch (quarter)
            {
                case 1:
                    return "Первая четверть";
                case 2:
                    return "Вторая четверть";
                case 3:
                    return "Третья четверть";
                case 4:
                    return "Четвертая четверть";
                default:
                    return string.Empty;
            }
        }
    }
}
