using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Controllers
{
    /// <summary>
    /// Main controller for the subscription tracker application
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Home page with summary of subscriptions
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Calendar view showing subscriptions by month
        /// </summary>
        public IActionResult Calendar(int? month, int? year)
        {
            // Default to current month and year if not specified
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;
            
            // Get first day of the month
            var firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);
            
            // Get days in month
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            
            // Get subscriptions for the month
            var subscriptions = _context.Subscriptions.ToList();
            
            // Create dictionary mapping day of month to subscriptions due that day
            var subscriptionsByDay = new Dictionary<int, List<Subscription>>();
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                var dueThatDay = subscriptions.Where(s => s.PaymentDate.Day == day).ToList();
                if (dueThatDay.Any())
                {
                    subscriptionsByDay[day] = dueThatDay;
                }
            }
            
            ViewBag.Month = currentMonth;
            ViewBag.Year = currentYear;
            ViewBag.MonthName = firstDayOfMonth.ToString("MMMM");
            ViewBag.FirstDayOfMonth = (int)firstDayOfMonth.DayOfWeek;
            ViewBag.DaysInMonth = daysInMonth;
            ViewBag.SubscriptionsByDay = subscriptionsByDay;
            ViewBag.TotalMonthlyCost = subscriptions.Sum(s => s.Cost);
            
            return View();
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
    }
}
