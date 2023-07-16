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
    
    public Task<string> CallWebApiForUserAsync(
        string accessToken,
        string downstreamApi)
    {
        throw new NotImplementedException();
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