using CinemaGo.Infrastructure.Data;
namespace CinemaGo.API.Services;

public class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _configuration;

    public DatabaseInitializerHostedService(IServiceScopeFactory scopes, IConfiguration configuration)
    { _scopes = scopes; _configuration = configuration; }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        await DbSeeder.InitializeAsync(scope.ServiceProvider,
            _configuration["SeedData:SuperAdminEmail"], _configuration["SeedData:SuperAdminPassword"], ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}