using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.Tests;

// Spins up the real API in-memory for testing, but swaps the real SQL Server
// DbContext for an EF Core InMemory database so tests don't need a live SQL Server.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));

            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<RaceDayContext>(options =>
            {
                options.UseInMemoryDatabase("RaceDayTestDb_" + Guid.NewGuid());
            });
        });
    }
}
