using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizedScannApi.Infrastructure.Data;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannAPI.Application.Pagination; // I maiúsculo
using OrganizedScannAPI.Application.Hateoas;    // I maiúsculo
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizedScannApi.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/users")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context) => _context = context;

        /// <summary>
        /// Lista paginada de usuários.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HateoasResponse<PaginatedResponse<User>>>> GetAll([FromQuery] PaginatedRequest pagination)
        {
            pagination.Normalize();

            var query = _context.Users.AsNoTracking().AsQueryable();
            var total = await query.CountAsync();
            var items = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                   .Take(pagination.PageSize)
                                   .ToListAsync();

            var response = new PaginatedResponse<User>
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                Total = total,
                Items = items
            };

            var hateoas = new HateoasResponse<PaginatedResponse<User>>(response);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetAll), values: new { pagination.PageNumber, pagination.PageSize }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "create", Href = Url.ActionLink(nameof(Create)) ?? string.Empty, Method = "POST" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Obtém um usuário por ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HateoasResponse<User>>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var hateoas = new HateoasResponse<User>(user);
            hateoas.Links.Add(new HateoasLink { Rel = "self", Href = Url.ActionLink(nameof(GetById), values: new { id }) ?? string.Empty });
            hateoas.Links.Add(new HateoasLink { Rel = "update", Href = Url.ActionLink(nameof(Update), values: new { id }) ?? string.Empty, Method = "PUT" });
            hateoas.Links.Add(new HateoasLink { Rel = "delete", Href = Url.ActionLink(nameof(Delete), values: new { id }) ?? string.Empty, Method = "DELETE" });
            return Ok(hateoas);
        }

        /// <summary>
        /// Cria um usuário.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> Create([FromBody] User user)
        {
            // Garantir que CreatedAt seja preenchido se não foi fornecido
            if (user.CreatedAt == default)
            {
                user.CreatedAt = DateTime.UtcNow;
            }
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Atualiza um usuário.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return NotFound();

            user.Id = id;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove um usuário.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
