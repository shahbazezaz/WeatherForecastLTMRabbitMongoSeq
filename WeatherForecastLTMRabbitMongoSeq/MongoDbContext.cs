using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace WeatherForecastLTMRabbitMongoSeq
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<TraceLog> _traceCollection;

        public MongoDbContext(IMongoClient mongoClient, IConfiguration configuration)
        {
            // Get database and collection names from configuration
            string databaseName = configuration["MongoDB:DatabaseName"] ?? "tracing";
            string collectionName = configuration["MongoDB:CollectionName"] ?? "traces";

            // Get the database
            _database = mongoClient.GetDatabase(databaseName);

            // Get the collection
            _traceCollection = _database.GetCollection<TraceLog>(collectionName);
        }

        public async Task AddTraceLogAsync(TraceLog log, CancellationToken cancellationToken = default)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            log.Timestamp = DateTime.UtcNow; // Ensure timestamp is set
            await _traceCollection.InsertOneAsync(log, cancellationToken: cancellationToken);
        }

        public async Task<List<TraceLog>> GetTraceLogsAsync(
        FilterDefinition<TraceLog> filter = null,
        SortDefinition<TraceLog> sortBy = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
        {
            var findFluent = _traceCollection.Find(filter ?? Builders<TraceLog>.Filter.Empty);

            if (sortBy != null)
            {
                findFluent = findFluent.Sort(sortBy);
            }

            if (limit.HasValue)
            {
                findFluent = findFluent.Limit(limit.Value);
            }

            return await findFluent.ToListAsync(cancellationToken);
        }
    }
}
