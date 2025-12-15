using CertificateIssuer.Services;
using DotEnv.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// .env dosyasını yükle (.env projenin kök dizininde olmalı)
new EnvLoader().Load();

// Servisleri DI konteynerine ekle
builder.Services.AddSingleton<IpfsService>();
builder.Services.AddSingleton<BlockchainService>();

// CORS ayarı (frontend localhost:5173 erişimi için)
builder.Services.AddCors(opt => opt.AddDefaultPolicy(
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

var app = builder.Build();
app.UseCors();

// ✅ 0) Basit sağlık kontrolü
app.MapGet("/", () => Results.Ok("✅ CertificateIssuer API ayakta (/.net minimal api)"));

// ✅ 1) Admin: PDF yükle + blockchain'e ekle
app.MapPost("/api/issue", async (HttpRequest req, IpfsService ipfs, BlockchainService chain) =>
{
    var form = await req.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null) return Results.BadRequest("file missing");

    var student = form["student"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(student))
        return Results.BadRequest("student required");

    // 1️⃣ Dosya hash hesapla
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var shaHex = CertificateIssuer.Services.HashService.Sha256Hex(ms.ToArray());

    // 2️⃣ IPFS’e yükle
    var pdfCid = await ipfs.UploadFileAsync(file);
    var tokenUri = $"ipfs://{pdfCid}";

    // 3️⃣ Blockchain'e yaz
    // description alanına IPFS URI'yi koyuyoruz (tezde güzel anlatılır)
    var tx = await chain.IssueAsync(student, tokenUri, shaHex);
    Console.WriteLine($"Yeni sertifika eklendi → {tx}");

    return Results.Ok(new
    {
        tx,
        sha256 = shaHex,
        pdfCid,
        tokenUri
    });
});

// ✅ 2) Admin: revoke (iptal etme) – şimdilik implement edilmedi
app.MapPost("/api/revoke", (long tokenId, string reason) =>
{
    return Results.StatusCode(501); // Not Implemented
});

// ✅ 3) Kullanıcı: verify (hash ile doğrulama)
app.MapGet("/api/verify", async (string hash, BlockchainService chain) =>
{
    try
    {
        var result = await chain.VerifyDetailedAsync(hash);

        if (result is null || !result.Exists)
        {
            Console.WriteLine($"Doğrulama → kayıt bulunamadı. hash:{hash}");
            return Results.Ok(new
            {
                exists = false,
                issuer = (string?)null,
                issuedAt = (long?)null,
                studentName = (string?)null,
                description = (string?)null
            });
        }

        Console.WriteLine($"Doğrulama → exists:{result.Exists}, issuer:{result.Issuer}, student:{result.StudentName}");

        long issuedAtUnix = (long)result.IssuedAt;

        return Results.Ok(new
        {
            exists = result.Exists,
            issuer = result.Issuer,
            issuedAt = issuedAtUnix,
            studentName = result.StudentName,
            description = result.Description
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ /api/verify hata: {ex.Message}");
        return Results.Problem("Blockchain verify sırasında hata oluştu: " + ex.Message);
    }
});

// API’yi başlat
app.Run();
