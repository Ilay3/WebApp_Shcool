using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebAppMain.Data;
using WebAppMain.Models;

namespace WebAppMain.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Получаем текущего пользователя
            var currentUser = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (currentUser == null)
            {
                // Если пользователь не найден, вернуть ошибку или перенаправить на страницу входа
                return NotFound();
            }

            // Загружаем все уведомления для текущего пользователя
            var notifications = _context.Notification
                .Include(n => n.User)
                .Where(n => n.UserId == currentUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            // Вычисляем количество непрочитанных уведомлений
            var unreadNotificationCount = notifications.Count(n => !n.IsRead);

            // Передаем количество непрочитанных уведомлений в представление
            ViewBag.UnreadNotificationCount = unreadNotificationCount;

            return View(notifications);
        }


        public IActionResult MarkAsRead(int id)
        {
            var notification = _context.Notification.FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public void CreateAssignmentNotification(int classId)
        {
            var students = _context.ApplicationUser
                .Where(u => u.Id_Class == classId)
                .ToList();

            var notifications = students.Select(student => new Notification
            {
                UserId = student.Id,
                Message = "Добавлено новое задание в вашем классе",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _context.Notification.AddRange(notifications);
            _context.SaveChanges();
        }


        public void CreateEstimationNotification(int journalPredmetId)
        {
            var journalPredmet = _context.JournalPredmet
                .Include(jp => jp.Journal)
                .FirstOrDefault(jp => jp.Id == journalPredmetId);

            if (journalPredmet == null)
            {
                return;
            }

            var students = _context.ApplicationUser
                .Where(u => u.Id == journalPredmet.Journal.UserId)
                .ToList();

            var notifications = students.Select(student => new Notification
            {
                UserId = student.Id,
                Message = "Вам выставили новую оценку!",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _context.Notification.AddRange(notifications);
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var notification = _context.Notification.FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            _context.Notification.Remove(notification);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }

    
}
