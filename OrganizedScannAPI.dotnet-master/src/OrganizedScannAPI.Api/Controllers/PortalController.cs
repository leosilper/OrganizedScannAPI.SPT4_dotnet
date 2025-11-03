using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannAPI.Application.Pagination; // I maiúsculo
using OrganizedScannAPI.Application.Hateoas;    // I maiúsculo
using System.Linq;
using System.Threading.Tasks;

namespace OrganizedScannApi.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/portals")]
    public class PortalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista paginada de portais.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HateoasResponse<PaginatedResponse<Portal>>>> GetAll([FromQuery] PaginatedRequest pagination)
        {
            pagination.Normalize();

            var query = _context.Portals.AsNoTracking().AsQueryable();
            var total = await query.CountAsync();
            var items = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                   .Take(pagination.PageSize)
                                   .ToListAsync();

            var response = new PaginatedResponse<Portal>
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                Total = total,
                Items = items
            };

            var hateoas = new HateoasResponse<PaginatedResponse<Portal>>(response);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetAll), values: new { pagination.PageNumber, pagination.PageSize }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "create", Href = Url.ActionLink(nameof(Create)) ?? string.Empty, Method = "POST" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Obtém um portal por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HateoasResponse<Portal>>> GetById(int id)
        {
            var portal = await _context.Portals.FindAsync(id);
            if (portal == null) return NotFound();

            var hateoas = new HateoasResponse<Portal>(portal);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "update", Href = Url.ActionLink(nameof(Update), values: new { id }) ?? string.Empty, Method = "PUT" });
            hateoas.Links.Add(new HateoasLink { Rel = "delete", Href = Url.ActionLink(nameof(Delete), values: new { id }) ?? string.Empty, Method = "DELETE" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Cria um portal.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Portal>> Create([FromBody] Portal portal)
        {
            _context.Portals.Add(portal);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = portal.Id }, portal);
        }

        /// <summary>
        /// Atualiza um portal.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Portal portal)
        {
            var existing = await _context.Portals.FindAsync(id);
            if (existing == null) return NotFound();

            portal.Id = id;
            _context.Portals.Update(portal);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um portal.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var portal = await _context.Portals.FindAsync(id);
            if (portal == null) return NotFound();

            _context.Portals.Remove(portal);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
