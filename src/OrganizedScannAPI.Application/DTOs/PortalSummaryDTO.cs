using System.Collections.Generic;

namespace OrganizedScannApi.Application.DTOs
{
    public class PortalSummaryDTO
    {
        public string PortalName { get; set; } = null!;
        public int MotorcycleCount { get; set; }
        public List<string> MotorcyclePlates { get; set; } = new List<string>();
    }
}