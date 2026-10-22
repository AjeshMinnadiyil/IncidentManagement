
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Models
{
    public class TicketAssignment
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        public Ticket? Ticket { get; set; }

        [Required]
        public string TechnicianId { get; set; } = string.Empty;

        [Required]
        public string AssignedByUserId { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UnassignedAt { get; set; }
    }
}
