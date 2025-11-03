using System.Collections.Generic;
using System.Threading.Tasks;
using OrganizedScannApi.Domain.Entities;

namespace OrganizedScannApi.Domain.Repositories
{
    public interface IPortalRepository
    {
        Task<IEnumerable<Portal>> GetAllAsync();
        Task<Portal?> GetByIdAsync(long id);
        Task AddAsync(Portal portal);
        Task UpdateAsync(Portal portal);
        Task DeleteAsync(long id);
    }
}
