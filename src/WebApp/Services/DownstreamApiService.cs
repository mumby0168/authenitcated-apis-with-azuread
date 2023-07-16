using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;

namespace WebApp.Services;

class DownstreamApiService : IDownstreamApiService
{
    private const string BearerHeader = "Bearer";
    private readonly HttpClient _client;
    private readonly DefaultAzureCredential _credential;
    private readonly IConfiguration _configuration;

    public DownstreamApiService(
        HttpClient client,
        DefaultAzureCredential credential,
        IConfiguration configuration)
    {
        _client = client;
        _credential = credential;
        _configuration = configuration;
    }
    
    public async Task<string> CallWebApiForUserAsync()
    {
        var result = await AuthenticatedRequest(() => _client.GetAsync("api/v1/default"));
        
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }

        return $"Failed to call downstream api status code: {result.StatusCode} and response body: {await result.Content.ReadAsStringAsync()}";
    }
    
    internal async Task<HttpResponseMessage> AuthenticatedRequest(Func<Task<HttpResponseMessage>> execute)
    {
        string [] scopes = {
            _configuration.GetValue<string>("DownstreamApi:Scope") ??
            throw new InvalidOperationException("Please provide a downstream api scope")
        };

        var accessToken = await _credential.GetTokenAsync(new TokenRequestContext(scopes));
        
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerHeader, accessToken.Token);

        return await execute.Invoke();
    }
    
}