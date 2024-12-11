using MongoDB.Bson;

namespace WeatherForecastLTMRabbitMongoSeq
{
    public class TraceLog
    {
        public ObjectId Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
    }
}
