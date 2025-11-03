using OrganizedScannApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Repositories
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
