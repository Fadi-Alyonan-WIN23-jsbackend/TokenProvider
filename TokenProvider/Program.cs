using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenProvider.infrastructure.data.Context;
using TokenProvider.infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContextFactory<DataContext>(options =>
        {
            options.UseSqlServer(Environment.GetEnvironmentVariable("Token-db"));
        });

        services.AddScoped<CookieGenerator>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<TokenGenerator>();
    })
    .Build();

host.Run();
