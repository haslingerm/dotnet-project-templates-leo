using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeoGRpcApi.Api.Persistence.Util;

public static class PersistenceSetup
{
    private const string ConnectionStringName = "Postgres";
    private const string MigrationHistoryTable = "__EFMigrationsHistory";

    public static void ConfigurePersistence(this IServiceCollection services,
                                            IConfigurationManager configurationManager,
                                            bool isDev)
    {
        ConfigureDatabase(services, configurationManager, isDev);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionProvider, UnitOfWork>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration,
                                          bool isDev)
    {
        string connectionString = configuration.GetConnectionString(ConnectionStringName)
                                  ?? throw new InvalidOperationException("Connection string not found");
        services.AddDbContext<DatabaseContext>(optionsBuilder =>
        {
            ConfigureDatabaseContextOptions(optionsBuilder, connectionString, isDev);
        });
    }

    public static void ConfigureDatabaseContextOptions(DbContextOptionsBuilder optionsBuilder, string connectionString,
                                                       bool sensitiveDataLogging)
    {
        optionsBuilder.UseNpgsql(connectionString,
                                 options => options
                                            .UseNodaTime()
                                            .MigrationsHistoryTable(MigrationHistoryTable,
                                                                    DatabaseContext.SchemaName))
                      .ConfigureWarnings(warnings =>
                                             warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
                      .UseSnakeCaseNamingConvention();

        if (sensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging()
                          .EnableDetailedErrors();
        }
    }
}
