
namespace WeatherForecastLTMRabbitMongoSeq
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days)
        {
            _logger.LogInformation("Generating weather forecast for {Days} days", days);

            try
            {
                var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).ToList();

                _logger.LogInformation("Successfully generated {Count} weather forecasts", forecasts.Count);
                return forecasts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating weather forecast");
                throw;
            }
        }

    }
}
