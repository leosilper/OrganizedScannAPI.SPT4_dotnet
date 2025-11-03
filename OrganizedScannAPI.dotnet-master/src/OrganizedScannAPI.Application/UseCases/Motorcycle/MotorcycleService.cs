using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannAPI.Application.Pagination; // <- I maiúsculo (onde está PaginatedRequest/Response)

namespace OrganizedScannApi.Application.UseCases.Motorcycles
{
    public class MotorcycleService
    {
        private readonly ApplicationDbContext _context;

        public MotorcycleService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna lista paginada de motocicletas com filtros opcionais por marca e ano.
        /// </summary>
        public async Task<PaginatedResponse<Motorcycle>> GetAllAsync(PaginatedRequest pagination, string? brand = null, int? year = null)
        {
            pagination.Normalize();

            IQueryable<Motorcycle> query = _context.Motorcycles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.Where(m => m.Brand == brand);

            if (year.HasValue)
                query = query.Where(m => m.Year == year.Value);

            var total = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Motorcycle>
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                Total = total,
                Items = items
            };
        }

        /// <summary>
        /// Busca por ID.
        /// </summary>
        public async Task<Motorcycle?> GetByIdAsync(int id)
        {
            return await _context.Motorcycles.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Cria motocicleta.
        /// </summary>
        public async Task<Motorcycle> CreateAsync(Motorcycle motorcycle)
        {
            _context.Motorcycles.Add(motorcycle);
            await _context.SaveChangesAsync();
            return motorcycle;
        }

        /// <summary>
        /// Atualiza motocicleta.
        /// </summary>
        public async Task UpdateAsync(int id, Motorcycle motorcycle)
        {
            motorcycle.Id = id;
            _context.Motorcycles.Update(motorcycle);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove motocicleta.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Motorcycles.FindAsync(id);
            if (entity != null)
            {
                _context.Motorcycles.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
