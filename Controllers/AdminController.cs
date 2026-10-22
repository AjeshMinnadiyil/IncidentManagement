using IncidentManagement.Data;
using IncidentManagement.Models;
using IncidentManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        public async Task<IActionResult> Index(
            string? searchString,
            string? status)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(t =>
                    t.RequestNumber.Contains(searchString) ||
                    t.Title.Contains(searchString) ||
                    t.Description.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(t => t.Status == status);
            }


            var tickets = await query
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var activeAssignments = await _context.TicketAssignments
                .Where(a => a.UnassignedAt == null)
                .ToListAsync();

            ViewBag.ActiveTicketIds = activeAssignments
                .Select(a => a.TicketId)
                .ToHashSet();

            return View(tickets);

        }


        [HttpGet]
        public async Task<IActionResult> Assign(int id)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            await LoadTechniciansAsync();

            return View(new AssignTechnicianViewModel
            {
                TicketId = ticket.Id
            });
        }

        private async Task LoadTechniciansAsync()
        {
            var technicians = await _userManager
                .GetUsersInRoleAsync("Technician");

            ViewBag.Technicians = technicians
                .Select(t => new SelectListItem
                {
                    Value = t.Id,
                    Text = $"{t.FullName} ({t.Email})"
                })
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(
            AssignTechnicianViewModel model)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == model.TicketId);

            if (ticket == null)

                return NotFound();


            var alreadyAssigned = await _context.TicketAssignments
                .AnyAsync(a => a.TicketId == model.TicketId
                            && a.UnassignedAt == null);

            if (alreadyAssigned)
            {
                TempData["ErrorMessage"] =
                    "This ticket is already assigned to a technician.";

                return RedirectToAction(nameof(Index));
            }


            var technician = await _userManager
                .FindByIdAsync(model.TechnicianId);

            if (technician == null ||
                !await _userManager.IsInRoleAsync(
                    technician, "Technician"))
            {
                ModelState.AddModelError(
                    nameof(model.TechnicianId),
                    "Please select a valid technician.");
            }

            if (!ModelState.IsValid)
            {
                await LoadTechniciansAsync();
                return View(model);
            }

            var adminId = _userManager.GetUserId(User);

            if (adminId == null)
                return Challenge();

            // Close any previous active assignment.
            var activeAssignments = await _context.TicketAssignments
                .Where(a => a.TicketId == ticket.Id &&
                            a.UnassignedAt == null)
                .ToListAsync();

            foreach (var assignment in activeAssignments)
            {
                assignment.UnassignedAt = DateTime.UtcNow;
            }

            var newAssignment = new TicketAssignment
            {
                TicketId = ticket.Id,
                TechnicianId = technician!.Id,
                AssignedByUserId = adminId,
                AssignedAt = DateTime.UtcNow
            };

            _context.TicketAssignments.Add(newAssignment);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Ticket {ticket.RequestNumber} assigned to {technician.FullName}.";

            return RedirectToAction(nameof(Index));
        }

    }
}