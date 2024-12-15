using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatatZaak.Models;

namespace PatatZaak.Controllers
{
    public class HomeController : Controller
    {
        // Admin Dashboard
        [Authorize(Roles = "admin")]
        public IActionResult Dashboard()
        {
            // Fetch any data relevant to the admin and pass to the view
            return View(); // Return Admin Dashboard View
        }

        // Worker Dashboard
        [Authorize(Roles = "worker")]
        public IActionResult WorkerDashboard()
        {
            // Fetch any data relevant to workers and pass to the view
            return View(); // Return Worker Dashboard View
        }

        // For users (or any non-admin roles)
        [Authorize(Roles = "user")]
        public IActionResult UserDashboard()
        {
            // Fetch data relevant to users and pass to the view
            return View(); // Return User Dashboard View
        }

        // Default view for all logged-in users
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
