namespace WebApp.Services;

public interface IDownstreamApiService
{
    Task<string> CallWebApiForUserAsync(string accessToken, string downstreamApi);
}