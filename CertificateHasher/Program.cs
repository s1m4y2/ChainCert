using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Dosya yolunu gir:");
var path = Console.ReadLine();

if (File.Exists(path))
{
    using var sha = SHA256.Create();
    using var stream = File.OpenRead(path);
    var hash = sha.ComputeHash(stream);
    var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    Console.WriteLine($"SHA-256 hash: {hex}");
}
else
{
    Console.WriteLine("Dosya bulunamadı.");
}
