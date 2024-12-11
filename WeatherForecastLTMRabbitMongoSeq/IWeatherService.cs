namespace WeatherForecastLTMRabbitMongoSeq
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days);
    }
}
