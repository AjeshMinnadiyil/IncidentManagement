using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.ViewModels
{
    public class TicketUpdateViewModel
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public string Status { get; set; } = "In Progress";

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Message { get; set; } = string.Empty;

        public bool IsVisibleToUser { get; set; } = true;
    }
}