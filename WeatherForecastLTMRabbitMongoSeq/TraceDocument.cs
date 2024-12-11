namespace WeatherForecastLTMRabbitMongoSeq
{
    public class TraceDocument
    {
        public string TraceId { get; set; }
        public string SpanId { get; set; }
        public string ServiceName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string TraceData { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
    }
}
