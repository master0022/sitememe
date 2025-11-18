using System.Net.Http;

namespace BlazorApp1;

/// <summary>
/// Background task that keeps the Render app warm by pinging a configured URL.
/// </summary>
public class KeepAliveService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KeepAliveService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public KeepAliveService(
        IHttpClientFactory httpClientFactory,
        ILogger<KeepAliveService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Do not run in Development to avoid noisy logs while working locally.
        if (_environment.IsDevelopment())
        {
            return;
        }

        var targetUrl = GetKeepAliveUrl();
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            _logger.LogInformation("Keep-alive URL not configured. Skipping ping job.");
            return;
        }

        // Ping roughly every 5 minutes to satisfy Render's free tier timeouts.
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PingAsync(targetUrl, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private string? GetKeepAliveUrl()
    {
        // Allow overrides via configuration or environment variable.
        return _configuration["KeepAlive:Url"]
               ?? Environment.GetEnvironmentVariable("KEEPALIVE_URL")
               ?? "https://sitememe.onrender.com/";
    }

    private async Task PingAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(nameof(KeepAliveService));
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Keep-alive ping responded with status {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed keep-alive ping to {Url}", url);
        }
    }
}
