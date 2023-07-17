namespace WebApp.Services;

public interface IDownstreamApiService
{
    Task<string> CallWebApiForUserAsync();
    Task<string> CallWebApiForReaderAsync();
    Task<string> CallWebApiForContributorAsync();
    Task<string> CallWebApiForOwnerAsync();
}