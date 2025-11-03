
using System;

namespace OrganizedScannApi.Application.DTOs
{
    public class MotorcycleDTO
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string Rfid { get; set; } = null!;
        public string PortalName { get; set; } = null!;
        public string? ProblemDescription { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime AvailabilityForecast { get; set; }
    }
}