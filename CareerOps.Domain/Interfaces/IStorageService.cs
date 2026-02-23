namespace CareerOps.Domain.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileContent, string fileName, string contentType = "application/pdf");
    Task<byte[]> GetFileBytesAsync(string fileUrl);
}
