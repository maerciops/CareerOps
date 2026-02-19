namespace CareerOps.Domain.Interfaces;

public interface IPdfParserService
{
    Task<string> ExtractTextFromPdfAsync(byte[] pdfFile);

}
