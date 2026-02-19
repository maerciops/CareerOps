using CareerOps.Domain.Exceptions;
using CareerOps.Domain.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace CareerOps.Infrastructure.Externalservices.Parsers;

public class PdfParserService : IPdfParserService
{
    public Task<string> ExtractTextFromPdfAsync(byte[] pdfFile)
    {
        if(pdfFile == null || pdfFile.Length == 0) throw new ArgumentNullException(nameof(pdfFile));

        var sb = new StringBuilder();

        try
        {
            using var pdfDocument = PdfDocument.Open(pdfFile);

            for (int i = 1; i <= pdfDocument.NumberOfPages; i++)
            {
                var page = pdfDocument.GetPage(i);

                sb.AppendLine(page.Text);
            }
        }
        catch (Exception ex) 
        {
            throw new ExternalServiceException($"Falha ao processar o arquivo PDF {ex.Message}");
        };

        return Task.FromResult(sb.ToString());

    }
}
