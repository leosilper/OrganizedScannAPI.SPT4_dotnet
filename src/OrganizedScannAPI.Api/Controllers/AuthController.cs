using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrganizedScannApi.Api.Controllers
{
    /// <summary>
    /// Autenticação usando JWT Bearer Token
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            // Verificação de email usando CountAsync para evitar problemas com Oracle e booleanos
            var emailCount = await _context.Users
                .Where(u => u.Email == request.Email)
                .CountAsync();
            
            if (emailCount > 0)
            {
                return BadRequest(new { message = "Email já está em uso" });
            }

            // Nota: A tabela USERS não possui coluna PASSWORD no banco de dados
            // Se você precisar de autenticação por senha, adicione a coluna PASSWORD na tabela
            var user = new User
            {
                Name = request.Name, // NAME é obrigatório (NOT NULL) na tabela
                Email = request.Email,
                Role = (Domain.Enums.UserRole)request.Role,
                CreatedAt = DateTime.UtcNow // Se o default não funcionar, definimos manualmente
            };

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("ORA-00001") == true || 
                                                ex.InnerException?.Message?.Contains("unique constraint") == true)
            {
                return BadRequest(new { message = "Email já está em uso" });
            }

            var token = GenerateJwtToken(user);
            return CreatedAtAction(nameof(Register), new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        /// <summary>
        /// Realiza login e obtém JWT token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            // Nota: A tabela USERS não possui coluna PASSWORD no banco de dados
            // Por enquanto, o login é feito apenas por email (sem validação de senha)
            // Se você precisar de autenticação por senha, adicione a coluna PASSWORD na tabela
            if (user == null)
            {
                return Unauthorized(new { message = "Email não encontrado" });
            }

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        /// <summary>
        /// Gera JWT token
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiration = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(150, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(0, 3)]
        public int Role { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

