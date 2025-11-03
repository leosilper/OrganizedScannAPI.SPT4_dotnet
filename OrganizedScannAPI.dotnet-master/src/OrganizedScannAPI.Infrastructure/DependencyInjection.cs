using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrganizedScannApi.Domain.Repositories;
using OrganizedScannApi.Infrastructure.Data;
//using OrganizedScannApi.Infrastructure.Repositories;

namespace OrganizedScannApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection") ?? config["ConnectionStrings:DefaultConnection"];
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseOracle(conn));
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
            //services.AddScoped<IPortalRepository, PortalRepository>();
            return services;
        }
    }
}
