
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required]
        public string RequestType { get; set; } = "Software";

        [Required(ErrorMessage = "Please select a category.")]
        public int? CategoryId { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;
    }
}
