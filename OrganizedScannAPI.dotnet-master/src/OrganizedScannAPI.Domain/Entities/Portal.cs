using System.ComponentModel.DataAnnotations;
using OrganizedScannApi.Domain.Enums;

namespace OrganizedScannApi.Domain.Entities
{
    public class Portal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public PortalType Type { get; set; }

        [Required]
        public string Name { get; set; } = null!;
    }
}
