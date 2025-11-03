using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Application.UseCases.Motorcycles;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannApi.Domain.Entities;
using Xunit;

namespace OrganizedScannAPI.Tests
{
    public class MotorcycleServiceTests
    {
        [Fact]
        public async Task GetPagedAsync_Should_Return_Paginated_List()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("motorcycles_db_" + System.Guid.NewGuid())
                .Options;

            using var ctx = new ApplicationDbContext(options);
            ctx.Motorcycles.AddRange(
                new Motorcycle { LicensePlate = "AAA1A11", Rfid = "RF1", PortalId = 1, ProblemDescription = "Revisão geral", Brand = "Honda", Year = 2022, EntryDate = System.DateTime.UtcNow, AvailabilityForecast = System.DateTime.UtcNow },
                new Motorcycle { LicensePlate = "BBB2B22", Rfid = "RF2", PortalId = 1, ProblemDescription = "Troca de pneus", Brand = "Yamaha", Year = 2021, EntryDate = System.DateTime.UtcNow, AvailabilityForecast = System.DateTime.UtcNow }
            );
            await ctx.SaveChangesAsync();

            var service = new MotorcycleService(ctx);
            var pagination = new OrganizedScannAPI.Application.Pagination.PaginatedRequest { PageNumber = 1, PageSize = 1 };
            var paged = await service.GetAllAsync(pagination);

            Assert.Single(paged.Items);
            Assert.Equal(2, paged.Total);
            Assert.Equal(1, paged.PageNumber);
            Assert.Equal(1, paged.PageSize);
        }
    }
}
