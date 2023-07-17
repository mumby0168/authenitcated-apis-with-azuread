using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;

namespace WebApp.Services;

class DownstreamApiService : IDownstreamApiService
{
    private const string BearerHeader = "Bearer";
    private readonly HttpClient _client;
    private readonly ILogger<DownstreamApiService> _logger;
    private readonly DefaultAzureCredential _credential;
    private readonly IConfiguration _configuration;

    public DownstreamApiService(
        HttpClient client,
        ILogger<DownstreamApiService> logger,
        DefaultAzureCredential credential,
        IConfiguration configuration)
    {
        _client = client;
        _logger = logger;
        _credential = credential;
        _configuration = configuration;
    }
    
    public async Task<string> CallWebApiForUserAsync()
    {
        try
        {
            var result = await AuthenticatedRequest(() => _client.GetAsync("api/v1/default"));
        
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            return $"Failed to call downstream api status code: {result.StatusCode} and response body: {await result.Content.ReadAsStringAsync()}";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling downstream api for a basic user message");
            throw;
        }
    }

    public async Task<string> CallWebApiForReaderAsync()
    {
        try
        {
            var result = await AuthenticatedRequest(() => _client.GetAsync("api/v1/reader"));
        
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            return $"Failed to call downstream api status code: {result.StatusCode} and response body: {await result.Content.ReadAsStringAsync()}";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling downstream api for a reader message");
            throw;
        }
    }

    public async Task<string> CallWebApiForContributorAsync()
    {
        try
        {
            var result = await AuthenticatedRequest(() => _client.GetAsync("api/v1/contributor"));
        
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            return $"Failed to call downstream api status code: {result.StatusCode} and response body: {await result.Content.ReadAsStringAsync()}";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling downstream api for a contributor message");
            throw;
        }
    }

    public async Task<string> CallWebApiForOwnerAsync()
    {
        try
        {
            var result = await AuthenticatedRequest(() => _client.GetAsync("api/v1/owner"));
        
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            return $"Failed to call downstream api status code: {result.StatusCode} and response body: {await result.Content.ReadAsStringAsync()}";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error calling downstream api for a owner message");
            throw;
        }
    }

    internal async Task<HttpResponseMessage> AuthenticatedRequest(Func<Task<HttpResponseMessage>> execute)
    {
        string [] scopes = {
            _configuration.GetValue<string>("DownstreamApi:Scope") ??
            throw new InvalidOperationException("Please provide a downstream api scope")
        };

        var accessToken = await _credential.GetTokenAsync(new TokenRequestContext(scopes));

        // this should NEVER be done in a real production app, for demo purposes only.
        if (_configuration.GetValue<bool>("DownstreamApi:IsTokenLoggingEnabled"))
        {
            _logger.LogInformation("Service token for downstream api: {Token}", accessToken.Token);
        }
        
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerHeader, accessToken.Token);

        return await execute.Invoke();
    }
    
}