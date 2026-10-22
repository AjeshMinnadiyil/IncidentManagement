
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace IncidentManagement.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RequestType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Ticket> Tickets { get; set; }
            = new List<Ticket>();
    }
}
