using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrganizedScannApi.Api.Controllers
{
    /// <summary>
    /// Endpoint para verificação de saúde da API
    /// </summary>
    [ApiController]
    [Route("api/v1/health")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Verifica o status de saúde da API
        /// </summary>
        /// <returns>Status da API, banco de dados e memória</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetHealth()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = healthReport.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = healthReport.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            };

            return healthReport.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
        }
    }
}

