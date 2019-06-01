using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DotNetCoreRepository.Data
{
    public class SAPDbContextFactory : IDesignTimeDbContextFactory<SAPDbContext>
    {
        // For unit tests, create instances of DbContext by reading from appsettings.json files in the same directory as the DLLs.
        public SAPDbContext Create()
        {
            var environmentName =
                    Environment.GetEnvironmentVariable(
                    "ASPNETCORE_ENVIRONMENT"
                );

            var basePath = AppContext.BaseDirectory;

            return CreateDbContext(new string[] { basePath, environmentName });
        }

        // Called by the “dotnet ef database update” command when deploying migrations.
        // It assumes appsettings.json file will be near the code and the csproj file.
        // The important thing is that it allows for AVOIDING marking appsettings.json as Copy Always in the project.
        public SAPDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(args.Length > 0 ? args[0] : Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("ProductionConnection");

            if (String.IsNullOrWhiteSpace(connectionString) == true)
            {
                throw new InvalidOperationException(
                    "Could not find a connection string named 'ProductionConnection'."
                );
            }
            else
            {
                return Create(connectionString);
            }
        }

        private SAPDbContext Create(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException(
                    $"{nameof(connectionString)} is null or empty.",
                    nameof(connectionString)
                );

            var optionsBuilder = new DbContextOptionsBuilder<SAPDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new SAPDbContext(optionsBuilder.Options);
        }
    }
}
