using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace OrganizedScannApi.Api.Controllers
{
    /// <summary>
    /// Predições de manutenção usando ML.NET
    /// </summary>
    [ApiController]
    [Route("api/v1/predictions")]
    [Produces("application/json")]
    [Authorize]
    public class PredictionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MLContext _mlContext;
        private ITransformer? _model;

        public PredictionController(ApplicationDbContext context)
        {
            _context = context;
            _mlContext = new MLContext();
        }

        /// <summary>
        /// Prevê o tempo de manutenção de uma motocicleta baseado em características
        /// </summary>
        [HttpPost("maintenance-time")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MaintenanceTimePrediction>> PredictMaintenanceTime([FromBody] MaintenancePredictionRequest request)
        {
            try
            {
                // Carregar dados históricos para treinar
                var historicalData = _context.Motorcycles
                    .Where(m => m.Year.HasValue && !string.IsNullOrEmpty(m.Brand))
                    .Select(m => new MaintenanceData
                    {
                        Year = m.Year!.Value,
                        DaysInMaintenance = (float)(m.AvailabilityForecast - m.EntryDate).TotalDays,
                        BrandFactor = GetBrandFactor(m.Brand!)
                    })
                    .ToList();

                if (!historicalData.Any())
                {
                    return BadRequest(new { message = "Dados insuficientes para treinar o modelo" });
                }

                // Criar e treinar modelo simples de regressão
                var dataView = _mlContext.Data.LoadFromEnumerable(historicalData);
                var pipeline = _mlContext.Transforms
                    .Concatenate("Features", nameof(MaintenanceData.Year), nameof(MaintenanceData.BrandFactor))
                    .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(MaintenanceData.DaysInMaintenance)));

                _model = pipeline.Fit(dataView);

                // Fazer predição
                var input = new MaintenanceData
                {
                    Year = request.Year,
                    BrandFactor = GetBrandFactor(request.Brand)
                };

                var predictionEngine = _mlContext.Model.CreatePredictionEngine<MaintenanceData, MaintenanceTimePredictionResult>(_model);
                var prediction = predictionEngine.Predict(input);

                var result = new MaintenanceTimePrediction
                {
                    PredictedDays = Math.Round(prediction.PredictedDays, 2),
                    EstimatedCompletionDate = DateTime.UtcNow.AddDays(prediction.PredictedDays),
                    Confidence = CalculateConfidence(prediction.PredictedDays, historicalData)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro ao gerar predição: {ex.Message}" });
            }
        }

        /// <summary>
        /// Analisa padrões de manutenção usando clustering ML.NET
        /// </summary>
        [HttpPost("maintenance-patterns")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> AnalyzeMaintenancePatterns()
        {
            try
            {
                var data = _context.Motorcycles
                    .Where(m => m.Year.HasValue && m.Year.Value > 0 && !string.IsNullOrEmpty(m.Brand))
                    .Select(m => new ClusteringData
                    {
                        Year = (float)m.Year!.Value,
                        BrandFactor = GetBrandFactor(m.Brand!),
                        HasLongMaintenance = (m.AvailabilityForecast - m.EntryDate).TotalDays > 7
                    })
                    .ToList();

                if (!data.Any())
                {
                    return BadRequest(new { message = "Dados insuficientes" });
                }

                var dataView = _mlContext.Data.LoadFromEnumerable(data);
                var pipeline = _mlContext.Transforms
                    .Concatenate("Features", nameof(ClusteringData.Year), nameof(ClusteringData.BrandFactor))
                    .Append(_mlContext.Clustering.Trainers.KMeans(numberOfClusters: 3));

                var model = pipeline.Fit(dataView);
                var predictions = model.Transform(dataView);
                var metrics = _mlContext.Clustering.Evaluate(predictions, scoreColumnName: "Score", featureColumnName: "Features");

                return Ok(new
                {
                    clusters = 3,
                    daviesBouldinIndex = metrics.DaviesBouldinIndex,
                    averageDistance = metrics.AverageDistance,
                    message = "Análise de padrões de manutenção concluída"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erro na análise: {ex.Message}" });
            }
        }

        private float GetBrandFactor(string brand)
        {
            // Fator baseado na marca (pode ser baseado em dados reais de confiabilidade)
            return brand.ToUpper() switch
            {
                "HONDA" => 1.0f,
                "YAMAHA" => 1.1f,
                "SUZUKI" => 1.2f,
                "KAWASAKI" => 1.3f,
                "BMW" => 1.5f,
                _ => 1.4f
            };
        }

        private string CalculateConfidence(double predictedDays, List<MaintenanceData> historical)
        {
            var avgActual = historical.Average(h => h.DaysInMaintenance);
            var variance = historical.Average(h => Math.Pow(h.DaysInMaintenance - avgActual, 2));
            var stdDev = Math.Sqrt(variance);

            if (stdDev == 0) return "Alta";
            
            var zScore = Math.Abs(predictedDays - avgActual) / stdDev;
            return zScore < 1 ? "Alta" : zScore < 2 ? "Média" : "Baixa";
        }
    }

    public class MaintenancePredictionRequest
    {
        [Required]
        [Range(1950, 2100)]
        public int Year { get; set; }

        [Required]
        public string Brand { get; set; } = string.Empty;
    }

    public class MaintenanceData
    {
        [LoadColumn(0)]
        public float Year { get; set; }

        [LoadColumn(1)]
        public float BrandFactor { get; set; }

        [LoadColumn(2)]
        public float DaysInMaintenance { get; set; }
    }

    public class ClusteringData
    {
        public float Year { get; set; }
        public float BrandFactor { get; set; }
        public bool HasLongMaintenance { get; set; }
    }

    public class MaintenanceTimePredictionResult
    {
        [ColumnName("Score")]
        public float PredictedDays { get; set; }
    }

    public class MaintenanceTimePrediction
    {
        public double PredictedDays { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public string Confidence { get; set; } = string.Empty;
    }
}

