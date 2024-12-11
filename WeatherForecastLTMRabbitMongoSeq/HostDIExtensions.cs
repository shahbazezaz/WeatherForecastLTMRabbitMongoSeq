using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MassTransit;

namespace WeatherForecastLTMRabbitMongoSeq
{
    public static class HostDIExtensions
    {
        public static IServiceCollection AddWebHostInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services
                .AddMongoDb(configuration)
                .AddMessageQueue(configuration)
                .AddHostOpenTelemetry();

            services
                .AddEndpointsApiExplorer()
                .AddSwaggerGen();

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<MongoDbContext>();
            });

            services.Configure<JsonOptions>(opt =>
            {
                opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            return services;
        }

        public static void AddHostLogging(this WebApplicationBuilder builder)
        {
            //builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                // Read configuration settings
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    // Add file logging
                    .WriteTo.File(
                        path: Path.Combine(logDirectory, "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB per file
                        retainedFileCountLimit: 7, // Keep 7 days of logs
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    // Optionally, add console logging if not already configured
                    .WriteTo.Console();
            });
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");
            var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

            mongoClientSettings.ClusterConfigurator = c => c.Subscribe(
                new DiagnosticsActivityEventSubscriber(
                    new InstrumentationOptions
                    {
                        CaptureCommandText = true
                    }));

            var pack = new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String)
        };

            ConventionRegistry.Register("EnumStringConvention", pack, _ => true);

            services.AddSingleton<IMongoClient>(new MongoClient(mongoClientSettings));
            services.AddSingleton<MongoDbContext>();

            return services;
        }

        private static IServiceCollection AddMessageQueue(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqConfiguration = configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>()!;

            services.AddMassTransit(busConfig =>
            {
                busConfig.SetKebabCaseEndpointNameFormatter();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(rabbitMqConfiguration.Host), h =>
                    {
                        h.Username(rabbitMqConfiguration.Username);
                        h.Password(rabbitMqConfiguration.Password);
                    });

                    cfg.UseMessageRetry(r => r.Exponential(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        private static IServiceCollection AddHostOpenTelemetry(this IServiceCollection services)
        {
            services
                .AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("ShippingService"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                        .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");

                    tracing.AddOtlpExporter();
                });

            return services;
        }
    }
}
