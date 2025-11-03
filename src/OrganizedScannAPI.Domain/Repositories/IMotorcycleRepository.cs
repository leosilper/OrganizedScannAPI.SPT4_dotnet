using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrganizedScannApi.Domain.Entities;

namespace OrganizedScannApi.Domain.Repositories
{
    public interface IMotorcycleRepository
    {
        Task<IEnumerable<Motorcycle>> GetAllAsync();
        Task<Motorcycle?> GetByIdAsync(long id);
        Task AddAsync(Motorcycle motorcycle);
        Task UpdateAsync(Motorcycle motorcycle);
        Task DeleteAsync(long id);
        Task<IEnumerable<Motorcycle>> SearchAsync(Expression<Func<Motorcycle, bool>> predicate);
    }
}
