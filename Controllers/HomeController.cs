using imcd_ticket_sync.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using imcd_ticket_sync;

namespace imcd_ticket_sync.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var listResults = await Program.ProcessDataAsync();
            //ViewBag.Incidents = listResults;
            return View(listResults);
        }
    }
}