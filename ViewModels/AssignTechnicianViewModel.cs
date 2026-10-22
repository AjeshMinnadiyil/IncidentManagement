
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.ViewModels
{
    public class AssignTechnicianViewModel
    {
        [Required]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Please select a technician.")]
        public string TechnicianId { get; set; } = string.Empty;
    }
}
