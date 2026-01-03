// Sample C# code to test the new upload endpoint

using System.Net.Http;
using System.Net.Http.Headers;

public class TransactionUploadTest
{
    public static async Task TestUpload()
    {
        var portfolioId = "3FA85F64-5717-4562-B3FC-2C963F66AFA6"; // Replace with your actual portfolio ID
        var excelFilePath = @"C:\path\to\your\transactions.xlsx";
        
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7061");
        
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(excelFilePath);
        using var fileContent = new StreamContent(fileStream);
        
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(fileContent, "file", Path.GetFileName(excelFilePath));
        
        var response = await httpClient.PostAsync($"/api/portfolio/{portfolioId}/transactions/upload", form);
        var result = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {result}");
    }
}

/*
Expected Response:
{
  "message": "Successfully processed 10 new transactions.",
  "totalProcessed": 15,
  "newTransactions": 10,
  "duplicatesSkipped": 5
}
*/
