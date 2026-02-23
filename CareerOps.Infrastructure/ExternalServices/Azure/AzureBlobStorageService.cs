using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CareerOps.Domain.Exceptions;
using CareerOps.Domain.Interfaces;
using CareerOps.Infrastructure.Externalservices.Azure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CareerOps.Infrastructure.Externalservices.Azure;

public class AzureBlobStorageService : IStorageService
{
    private readonly AzureOptions _configuration;
    public AzureBlobStorageService(IOptions<AzureOptions> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task<byte[]> GetFileBytesAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) throw new ExternalServiceException("URL do arquivo vazia.");

        var blobUri = new Uri(fileUrl);
        var segments = blobUri.AbsolutePath.TrimStart('/').Split('/', 2);
        var blobName = segments[1];

        var blobClient = new BlobServiceClient(_configuration.ConnectionString)
            .GetBlobContainerClient(_configuration.ContainerName)
            .GetBlobClient(blobName);

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToArray();
    }

    public async Task<string> UploadFileAsync(Stream fileContent, string fileName, string contentType = "application/pdf")
    {
        var blobServiceCliente = new BlobServiceClient(_configuration.ConnectionString);

        var container = blobServiceCliente.GetBlobContainerClient(_configuration.ContainerName);

        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension)) throw new ExternalServiceException("Extensão do arquivo inválida.");

        var newFileName = Guid.NewGuid().ToString() + extension;

        var clientFile = container.GetBlobClient(newFileName);

        var headerBlob = new BlobHttpHeaders { ContentType = contentType };

        await clientFile.UploadAsync(fileContent, new BlobUploadOptions { HttpHeaders = headerBlob });

        return clientFile.Uri.ToString();
    }
}
