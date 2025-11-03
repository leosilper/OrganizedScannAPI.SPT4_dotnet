using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Application.UseCases.Motorcycles;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannApi.Domain.Entities;
using Xunit;

public class MotorcycleServiceTests
{
    [Fact]
    public async Task GetPagedAsync_Should_Return_Paginated_List()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("motorcycles_db")
            .Options;

        using var ctx = new ApplicationDbContext(options);
        ctx.Motorcycles.AddRange(
            new Motorcycle { LicensePlate = "AAA1A11", Rfid = "RF1", PortalId = 1, Brand = "Honda", Year = 2022, EntryDate = System.DateTime.UtcNow, AvailabilityForecast = System.DateTime.UtcNow },
            new Motorcycle { LicensePlate = "BBB2B22", Rfid = "RF2", PortalId = 1, Brand = "Yamaha", Year = 2021, EntryDate = System.DateTime.UtcNow, AvailabilityForecast = System.DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var service = new MotorcycleService(ctx);
        var paged = await service.GetPagedAsync(pageNumber: 1, pageSize: 1);

        paged.Items.Should().HaveCount(1);
        paged.Total.Should().Be(2);
        paged.TotalPages.Should().Be(2);
    }
}
