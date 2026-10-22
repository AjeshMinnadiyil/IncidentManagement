
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Models
{
    public class TicketUpdate
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        public Ticket? Ticket { get; set; }

        [Required]
        public string UpdatedByUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public bool IsVisibleToUser { get; set; } = false;

        [StringLength(30)]
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
