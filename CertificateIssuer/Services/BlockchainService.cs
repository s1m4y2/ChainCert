using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace CertificateIssuer.Services
{
    // --- Kontrat fonksiyon modelleri ---

    [Function("issueCertificate")]
    public class IssueCertificateFunction : FunctionMessage
    {
        [Parameter("bytes32", "hash", 1)]
        public byte[] Hash { get; set; } = Array.Empty<byte>();

        [Parameter("string", "studentName", 2)]
        public string StudentName { get; set; } = string.Empty;

        [Parameter("string", "description", 3)]
        public string Description { get; set; } = string.Empty;
    }

    [Function("verifyCertificate", typeof(VerifyCertificateOutput))]
    public class VerifyCertificateFunction : FunctionMessage
    {
        [Parameter("bytes32", "hash", 1)]
        public byte[] Hash { get; set; } = Array.Empty<byte>();
    }

    [FunctionOutput]
    public class VerifyCertificateOutput : IFunctionOutputDTO
    {
        [Parameter("bool", "exists", 1)]
        public bool Exists { get; set; }

        [Parameter("address", "issuer", 2)]
        public string Issuer { get; set; } = string.Empty;

        [Parameter("uint256", "issuedAt", 3)]
        public BigInteger IssuedAt { get; set; }

        [Parameter("string", "studentName", 4)]
        public string StudentName { get; set; } = string.Empty;

        [Parameter("string", "description", 5)]
        public string Description { get; set; } = string.Empty;
    }

    [Function("isRegistered", "bool")]
    public class IsRegisteredFunction : FunctionMessage
    {
        [Parameter("bytes32", "hash", 1)]
        public byte[] Hash { get; set; } = Array.Empty<byte>();
    }

    public class BlockchainService
    {
        private readonly Web3 _web3;
        private readonly string _contractAddress;

        public BlockchainService()
        {
            var pk = Environment.GetEnvironmentVariable("PRIVATE_KEY")
                     ?? throw new Exception("PRIVATE_KEY yok");
            var rpc = Environment.GetEnvironmentVariable("RPC_URL")
                      ?? throw new Exception("RPC_URL yok");
            _contractAddress = Environment.GetEnvironmentVariable("CONTRACT_ADDRESS")
                               ?? throw new Exception("CONTRACT_ADDRESS yok");

            var account = new Account(pk);
            _web3 = new Web3(account, rpc);
        }

        private static byte[] HexToBytes32(string hashHex)
        {
            if (hashHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hashHex = hashHex[2..];

            if (hashHex.Length != 64)
                throw new ArgumentException("Hash uzunluğu 32 byte (64 hex karakter) olmalı.", nameof(hashHex));

            return Convert.FromHexString(hashHex);
        }

        // 🔹 1) Sertifika kaydet (issueCertificate)
        public async Task<string> IssueAsync(string studentName, string description, string sha256Hex)
        {
            var function = new IssueCertificateFunction
            {
                Hash = HexToBytes32(sha256Hex),
                StudentName = studentName,
                Description = description
            };

            var handler = _web3.Eth.GetContractTransactionHandler<IssueCertificateFunction>();
            var txHash = await handler.SendRequestAsync(_contractAddress, function);

            return txHash;
        }

        // 🔹 2) Detaylı doğrulama (verifyCertificate)
        public async Task<VerifyCertificateOutput?> VerifyDetailedAsync(string sha256Hex)
        {
            var function = new VerifyCertificateFunction
            {
                Hash = HexToBytes32(sha256Hex)
            };

            var handler = _web3.Eth.GetContractQueryHandler<VerifyCertificateFunction>();

            // Generic metodu DOĞRU kullanıyoruz:
            var result =
                await handler.QueryDeserializingToObjectAsync<VerifyCertificateOutput>(
                    function, _contractAddress);

            return result;
        }

        // 🔹 3) Sadece kayıtlı mı? (isRegistered)
        public async Task<bool> IsRegisteredAsync(string sha256Hex)
        {
            var function = new IsRegisteredFunction
            {
                Hash = HexToBytes32(sha256Hex)
            };

            var handler = _web3.Eth.GetContractQueryHandler<IsRegisteredFunction>();
            var exists = await handler.QueryAsync<bool>(_contractAddress, function);
            return exists;
        }
    }
}
