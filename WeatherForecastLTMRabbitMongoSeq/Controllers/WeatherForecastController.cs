using DnsClient;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading;

namespace WeatherForecastLTMRabbitMongoSeq.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly TraceLogService _traceLogService;

        public WeatherForecastController(
            IWeatherService weatherService,
            ILogger<WeatherForecastController> logger,
            TraceLogService traceLogService)
        {
            _weatherService = weatherService;
            _logger = logger;
            _traceLogService = traceLogService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecast(CancellationToken cancellationToken)
        {
            await _traceLogService.LogTraceAsync("Weather forecast endpoint called", LogLevel.Information, cancellationToken);
            var forecast = await _weatherService.GetForecastAsync(5);
            return forecast;
        }
    }
}
