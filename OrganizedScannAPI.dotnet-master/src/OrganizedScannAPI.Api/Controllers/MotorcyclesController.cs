using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannAPI.Application.Pagination; // I maiúsculo
using OrganizedScannAPI.Application.Hateoas;    // I maiúsculo
using OrganizedScannApi.Application.UseCases.Motorcycles;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizedScannApi.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/motorcycles")]
    public class MotorcycleController : ControllerBase
    {
        private readonly MotorcycleService _service;
        private readonly ApplicationDbContext _context;

        public MotorcycleController(MotorcycleService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        /// <summary>
        /// Lista paginada de motocicletas (filtros opcionais por marca e ano).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HateoasResponse<PaginatedResponse<Motorcycle>>>> GetAll(
            [FromQuery] PaginatedRequest pagination,
            [FromQuery] string? brand = null,
            [FromQuery] int? year = null)
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

            var response = new PaginatedResponse<Motorcycle>
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                Total = total,
                Items = items
            };

            var hateoas = new HateoasResponse<PaginatedResponse<Motorcycle>>(response);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetAll), values: new { pagination.PageNumber, pagination.PageSize, brand, year }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "create", Href = Url.ActionLink(nameof(Create)) ?? string.Empty, Method = "POST" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Obtém uma motocicleta por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HateoasResponse<Motorcycle>>> GetById(int id)
        {
            var mc = await _service.GetByIdAsync(id);
            if (mc == null) return NotFound();

            var hateoas = new HateoasResponse<Motorcycle>(mc);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "update", Href = Url.ActionLink(nameof(Update), values: new { id }) ?? string.Empty, Method = "PUT" });
            hateoas.Links.Add(new HateoasLink { Rel = "delete", Href = Url.ActionLink(nameof(Delete), values: new { id }) ?? string.Empty, Method = "DELETE" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Cria uma nova motocicleta.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Motorcycle>> Create([FromBody] Motorcycle motorcycle)
        {
            var created = await _service.CreateAsync(motorcycle);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Atualiza uma motocicleta existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Motorcycle motorcycle)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.UpdateAsync(id, motorcycle);
            return NoContent();
        }

        /// <summary>
        /// Remove uma motocicleta.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
