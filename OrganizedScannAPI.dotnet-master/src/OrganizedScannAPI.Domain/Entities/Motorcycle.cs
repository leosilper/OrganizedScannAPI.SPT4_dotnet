using System;

namespace OrganizedScannApi.Domain.Entities
{
    public class Motorcycle
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string Rfid { get; set; } = null!;
        public string ProblemDescription { get; set; } = null!;
        public int PortalId { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime AvailabilityForecast { get; set; }
        public string? Brand { get; set; }
        public int? Year { get; set; }

        public Portal? Portal { get; set; }
    }
}
