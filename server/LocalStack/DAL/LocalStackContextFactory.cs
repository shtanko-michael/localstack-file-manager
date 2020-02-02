using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LocalStack.DAL {
    public class LocalStackContextFactory : IDesignTimeDbContextFactory<LocalStackContext> {
        public LocalStackContext CreateDbContext(string[] args) {

            IConfigurationRoot configuration = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LocalStack"))
              .AddJsonFile("appsettings.Development.json")
              .Build();

            var builder = new DbContextOptionsBuilder<LocalStackContext>();
            builder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));

            return new LocalStackContext(builder.Options);
        }
    }
}
