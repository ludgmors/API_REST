using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WebApi.Data;

/// <summary>
/// Fabrica usada por las herramientas de EF Core ("dotnet ef") en tiempo de
/// diseno. Permite que las migraciones se generen sin levantar toda la
/// aplicacion y respetando el proveedor configurado en appsettings.json
/// (clave "DatabaseProvider": "Sqlite" o "SqlServer").
///
/// Uso:
///   dotnet ef migrations add InitialCreate        (genera en la carpeta Migrations/)
///   dotnet ef database update
///
/// IMPORTANTE: las migraciones son especificas del proveedor. Si cambias de
/// Sqlite a SqlServer (o viceversa) debes eliminar la carpeta Migrations/ y
/// volver a generarlas con el proveedor activo. Ver docs/BASE_DE_DATOS.md.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("SqlServerConnection"),
                b => b.MigrationsAssembly(migrationsAssembly));
        }
        else
        {
            optionsBuilder.UseSqlite(
                configuration.GetConnectionString("SqliteConnection"),
                b => b.MigrationsAssembly(migrationsAssembly));
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
