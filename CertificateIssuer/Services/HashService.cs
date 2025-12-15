using System.Security.Cryptography;

namespace CertificateIssuer.Services;
public static class HashService
{
    public static string Sha256Hex(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
