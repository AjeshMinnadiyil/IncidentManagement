
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
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();

            return View(new CreateTicketViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTicketViewModel model)
        {
            if (model.CategoryId.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.Id == model.CategoryId.Value &&
                        c.IsActive);

                if (category == null)
                {
                    ModelState.AddModelError(
                        nameof(model.CategoryId),
                        "Please select a valid category.");
                }
                else if (!string.Equals(
                    category.RequestType,
                    model.RequestType,
                    StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        nameof(model.CategoryId),
                        "The category does not match the request type.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.RequestType);
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var ticket = new Ticket
            {
                RequestNumber =
                    $"IM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 1000)}",

                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                CategoryId = model.CategoryId!.Value,
                CreatedByUserId = user.Id,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Ticket {ticket.RequestNumber} created successfully.";

            return RedirectToAction(nameof(MyTickets));
        }

        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = _userManager.GetUserId(User);

            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> Update(int id)
        {
            var ticket = await _context.Tickets
      .Include(t => t.Updates)
      .FirstOrDefaultAsync(t => t.Id == id);


            if (ticket == null)
                return NotFound();

            var reopenReason = ticket.Updates
    .Where(u => u.Status == "Reopened")
    .OrderByDescending(u => u.CreatedAt)
    .Select(u => u.Message)
    .FirstOrDefault();

            ViewBag.ReopenReason = reopenReason;

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            if (User.IsInRole("Technician"))
            {
                var isAssigned = await _context.TicketAssignments
                    .AnyAsync(a =>
                        a.TicketId == id &&
                        a.TechnicianId == userId &&
                        a.UnassignedAt == null);

                if (!isAssigned)
                    return Forbid();
            }

            var model = new TicketUpdateViewModel
            {
                TicketId = ticket.Id,
                Status = ticket.Status,
                Message = string.Empty,
                IsVisibleToUser = true
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> Update(
            TicketUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
              
                return View(model);
            }

            var allowedStatuses = new[]
     {
    "In Progress",
    "Resolved"
};

            if (!allowedStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Invalid ticket status.");

          
                return View(model);
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == model.TicketId);

            if (ticket == null)
                return NotFound();

            if (ticket.Status == "Resolved" ||
    ticket.Status == "Closed")
            {
                return Forbid();
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

          
            if (User.IsInRole("Technician"))
            {
                var isAssigned = await _context.TicketAssignments
                    .AnyAsync(a =>
                        a.TicketId == ticket.Id &&
                        a.TechnicianId == userId &&
                        a.UnassignedAt == null);

                if (!isAssigned)
                    return Forbid();
            }

            ticket.Status = model.Status;

            var update = new TicketUpdate
            {
                TicketId = ticket.Id,
                UpdatedByUserId = userId,
                Message = model.Message.Trim(),
                IsVisibleToUser = model.IsVisibleToUser,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketUpdates.Add(update);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Ticket {ticket.RequestNumber} updated successfully.";

            if (User.IsInRole("Technician"))
            {
                return RedirectToAction("Index", "Technician");
            }

            return RedirectToAction("Index", "Admin");

            


        }
    

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Updates)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.CreatedByUserId == userId);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Updates = ticket.Updates
                .Where(u => u.IsVisibleToUser)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            return View(ticket);
        }

        private async Task LoadCategoriesAsync(
            string? requestType = null)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.RequestType)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.RequestType} - {c.Name}",
                    Selected = requestType != null &&
                        c.RequestType.Equals(
                            requestType,
                            StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var userId = _userManager.GetUserId(User);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.CreatedByUserId == userId);

            if (ticket == null)
                return NotFound();

            if (ticket.Status != "Resolved")
                return BadRequest("Only resolved tickets can be closed.");

            ticket.Status = "Closed";
            ticket.ClosedAt = DateTime.UtcNow;

            _context.TicketUpdates.Add(new TicketUpdate
            {
                TicketId = ticket.Id,
                UpdatedByUserId = userId!,
                Message = "Employee confirmed the resolution and closed the ticket.",
                IsVisibleToUser = true,
                Status = "Closed",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket closed successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id, string reason)
        {
            var userId = _userManager.GetUserId(User);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.CreatedByUserId == userId);

            if (ticket == null)
                return NotFound();

            if (ticket.Status != "Resolved")
                return BadRequest("Only resolved tickets can be reopened.");

            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length < 3 ||
                reason.Trim().Length > 2000)
            {
                TempData["ErrorMessage"] =
                    "Please provide a reason between 3 and 2000 characters.";

                return RedirectToAction(nameof(Details), new { id });
            }

            ticket.Status = "Reopened";

            var activeAssignments = await _context.TicketAssignments
    .Where(a => a.TicketId == ticket.Id &&
                a.UnassignedAt == null)
    .ToListAsync();

            foreach (var assignment in activeAssignments)
            {
                assignment.UnassignedAt = DateTime.UtcNow;
            }
            ticket.ClosedAt = null;

            _context.TicketUpdates.Add(new TicketUpdate
            {
                TicketId = ticket.Id,
                UpdatedByUserId = userId!,
                Message = reason.Trim(),
                IsVisibleToUser = true,
                Status = "Reopened",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket reopened successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
