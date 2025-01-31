﻿namespace WeatherForecastLTMRabbitMongoSeq
{
    public record RabbitMQConfiguration
    {
        public required string Host { get; init; }

        public required string Username { get; init; }

        public required string Password { get; init; }
    }
}
