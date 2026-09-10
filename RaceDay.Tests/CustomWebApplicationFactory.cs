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
    // IMPORTANT: this name must be generated ONCE per factory instance, outside
    // the AddDbContext configuration delegate below. AddDbContext's options
    // delegate is re-evaluated once per request (its lifetime defaults to
    // Scoped), so putting Guid.NewGuid() directly inside that delegate gives
    // every single HTTP request its own empty database - which breaks any
    // test that registers a user and then logs in, since "login" hits a
    // different database than "register" did.
    private readonly string _dbName = "RaceDayTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));

            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<RaceDayContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}
