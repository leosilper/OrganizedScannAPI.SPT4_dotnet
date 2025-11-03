using Swashbuckle.AspNetCore.Filters;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Domain.Enums;
using System;

namespace OrganizedScannApi.Api.Swagger
{
    public class MotorcycleExample : IExamplesProvider<Motorcycle>
    {
        public Motorcycle GetExamples() => new Motorcycle
        {
            Id = 1,
            LicensePlate = "ABC1D23",
            Rfid = "RFID-0001",
            ProblemDescription = "Troca de óleo",
            PortalId = 1,
            EntryDate = DateTime.UtcNow,
            AvailabilityForecast = DateTime.UtcNow.AddDays(2),
            Brand = "Honda",
            Year = 2022
        };
    }

    public class PortalExample : IExamplesProvider<Portal>
    {
        public Portal GetExamples() => new Portal
        {
            Id = 1,
            Name = "Manutenção Rápida",
            Type = PortalType.QUICK_MAINTENANCE
        };
    }

    public class UserExample : IExamplesProvider<User>
    {
        public User GetExamples() => new User
        {
            Id = 1,
            Email = "admin@example.com",
            Password = "Secret@123",
            Role = UserRole.ADMIN
        };
    }
}
