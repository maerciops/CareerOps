namespace CareerOps.Infrastructure.Externalservices.Azure.Configuration;

public class AzureOptions
{
    public const string SectionName = "AzureStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
