using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CertificateIssuer.Services;

public class IpfsService
{
    private readonly HttpClient _http = new();

    // artık IFormFile alıyor (API uyumlu)
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var jwt = Environment.GetEnvironmentVariable("PINATA_JWT")
                  ?? throw new Exception("PINATA_JWT yok (.env kontrol et)");

        using var form = new MultipartFormDataContent();

        // IFormFile içeriğini belleğe al
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;
        var fileContent = new ByteArrayContent(ms.ToArray());
        form.Add(fileContent, "file", file.FileName);

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var resp = await _http.PostAsync("https://api.pinata.cloud/pinning/pinFileToIPFS", form);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("IpfsHash").GetString()!;
    }
}
