using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Controllers
{
    /// <summary>
    /// Controller for subscription reporting and analytics
    /// </summary>
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays a summary report of all subscriptions
        /// </summary>
        public async Task<IActionResult> Index(string sortOrder = "daysAsc")
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DaysSortParm"] = sortOrder == "daysAsc" ? "daysDesc" : "daysAsc";
            ViewData["NameSortParm"] = sortOrder == "nameAsc" ? "nameDesc" : "nameAsc";
            ViewData["CostSortParm"] = sortOrder == "costAsc" ? "costDesc" : "costAsc";
            ViewData["DateSortParm"] = sortOrder == "dateAsc" ? "dateDesc" : "dateAsc";

            var subscriptions = await _context.Subscriptions.ToListAsync();
            
            // Calculate the total monthly cost of all subscriptions
            decimal totalMonthlyCost = subscriptions.Sum(s => s.Cost);
            ViewData["TotalMonthlyCost"] = totalMonthlyCost;
            
            // Sort subscriptions based on the selected sort order
            switch (sortOrder)
            {
                case "daysDesc":
                    subscriptions = subscriptions.OrderByDescending(s => s.DaysRemaining).ToList();
                    break;
                case "nameAsc":
                    subscriptions = subscriptions.OrderBy(s => s.Name).ToList();
                    break;
                case "nameDesc":
                    subscriptions = subscriptions.OrderByDescending(s => s.Name).ToList();
                    break;
                case "costAsc":
                    subscriptions = subscriptions.OrderBy(s => s.Cost).ToList();
                    break;
                case "costDesc":
                    subscriptions = subscriptions.OrderByDescending(s => s.Cost).ToList();
                    break;
                case "dateAsc":
                    subscriptions = subscriptions.OrderBy(s => s.PaymentDate.Day).ToList();
                    break;
                case "dateDesc":
                    subscriptions = subscriptions.OrderByDescending(s => s.PaymentDate.Day).ToList();
                    break;
                default: // "daysAsc"
                    subscriptions = subscriptions.OrderBy(s => s.DaysRemaining).ToList();
                    break;
            }

            return View(subscriptions);
        }
    }
}