
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [StringLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public ICollection<TicketAssignment> Assignments { get; set; }
            = new List<TicketAssignment>();

        public ICollection<TicketUpdate> Updates { get; set; }
            = new List<TicketUpdate>();
    }
}
