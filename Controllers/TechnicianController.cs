
using IncidentManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Controllers
{
    [Authorize(Roles = "Technician")]
    public class TechnicianController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IncidentManagement.Models.ApplicationUser>
            _userManager;

        public TechnicianController(
            ApplicationDbContext context,
            UserManager<IncidentManagement.Models.ApplicationUser>
                userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var technicianId = _userManager.GetUserId(User);

            if (technicianId == null)
                return Challenge();


            var assignments = await _context.TicketAssignments
                .Include(a => a.Ticket)
                    .ThenInclude(t => t!.Category)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t!.Updates)
                .Where(a => a.TechnicianId == technicianId &&
                            a.UnassignedAt == null)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

            return View(assignments);

        }
    }
}
