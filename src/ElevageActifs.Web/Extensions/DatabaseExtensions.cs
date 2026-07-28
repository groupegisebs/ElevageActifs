using ElevageActifs.Web.Configuration;
using ElevageActifs.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ElevageActifs.Web.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var schema = NormalizeSchema(dbOptions.Schema);
            var historyTable = "__EFMigrationsHistory";

            switch (dbOptions.Provider.Trim().ToUpperInvariant())
            {
                case "SQLSERVER":
                case "MSSQL":
                    options.UseSqlServer(connectionString, sql =>
                        sql.MigrationsHistoryTable(historyTable, schema));
                    break;

                case "POSTGRESQL":
                case "POSTGRES":
                case "NPGSQL":
                default:
                    options.UseNpgsql(connectionString, npgsql =>
                        npgsql.MigrationsHistoryTable(historyTable, schema));
                    break;
            }
        });

        return services;
    }

    internal static string NormalizeSchema(string? schema) =>
        string.IsNullOrWhiteSpace(schema) ? "public" : schema.Trim();
}
