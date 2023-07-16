namespace Api.Extensions;

public static class ConfigurationExtensions
{
    public static string GetDockerImage(
        this IConfiguration configuration) =>
        configuration.GetValue<string>("DOCKER_CUSTOM_IMAGE_NAME") ?? "No Docker Image Name Specified";
}