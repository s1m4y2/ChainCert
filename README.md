# ChainCert 🔗✨

## Blockchain Tabanlı Sertifika Doğrulama Sistemi (Blockchain-Based Certificate Verification)

ChainCert, **PDF sertifikaları merkeziyetsiz şekilde saklayan ve doğrulayan** bir Web3 uygulamasıdır.
Sistem; sertifika PDF’ini **IPFS’e (Pinata üzerinden)** yükler, belgenin **SHA-256 hash** değerini üretir ve **IPFS URI + hash + öğrenci bilgilerini** Ethereum tabanlı **CertificateRegistry** akıllı kontratına kaydeder.

> ✅ Amaç: Sertifika/diploma gibi belgelerde sahteciliği önlemek ve doğrulamayı **merkezi otorite olmadan**, şeffaf ve değiştirilemez şekilde yapmak.

---

## 🚀 Canlı Demo Akışı 

### 1) Issue (Sertifika Oluştur)

* PDF seç → öğrenci adını yaz → **Issue**
* Sistem:

  * PDF → IPFS’e yükler (**CID** üretir)
  * PDF bytes → **SHA-256** hash hesaplar
  * Hash + CID + öğrenci bilgileri → **Blockchain**’e yazar

### 2) Verify (Sertifika Doğrula)

* Elindeki sertifikanın **SHA-256 hash** değerini gir
* Sistem blockchain’den:

  * kayıt var mı?
  * issuer kim?
  * issuedAt ne zaman?
  * öğrenci adı / açıklama?

---

## 🧠 Sistem Mantığı (Kısa Özet)

* **PDF dosyasını blockchain’e koymuyoruz** (gas maliyeti çok yüksek).
* PDF **IPFS**’te saklanır → değiştirilemez içerik adresli yapı (**CID**).
* Blockchain’e yalnızca:

  * **hash (bytes32)** → belgenin dijital parmak izi
  * **IPFS URI** → belgeye erişim adresi
  * **metadata** → öğrenci adı, açıklama, zaman vb.
    kaydedilir.

---

## 🏗️ Mimari

```
[User Interface (HTML/CSS/JS)]
        |
        v
[ .NET 8 Minimal API ]
   |        |        |
   |        |        +--> SHA-256 Hash Service
   |        |
   |        +--> IPFS (Pinata) -> CID / ipfs://URI
   |
   +--> Ethereum Smart Contract (Solidity / EVM)
        |
        v
Immutable Certificate Records (Hash + URI + Metadata)
```

---

## ✨ Özellikler

* ✅ PDF sertifikayı IPFS’e yükleme (Pinata)
* ✅ SHA-256 hash üretme (belgenin dijital parmak izi)
* ✅ Hash + IPFS URI + öğrenci bilgilerini blockchain’e yazma
* ✅ Hash ile blockchain doğrulama (kayıt var mı? kim yazdı? ne zaman?)
* ✅ Modern Neon UI ile Issue & Verify ekranları

---

## 🧰 Kullanılan Teknolojiler

### Blockchain / Web3

* **Solidity** (Smart Contract)
* **Ethereum Virtual Machine (EVM)**
* **Remix IDE** (geliştirme & deploy)
* **MetaMask** (transaction imzalama)
* **Ganache** (local test ağı)

### Backend

* **.NET 8 Minimal API**
* **Nethereum** (C# → Ethereum etkileşimi)

### Depolama

* **IPFS** (decentralized storage)
* **Pinata** (IPFS pinning + API)

### Frontend

* **HTML / CSS / JavaScript** (Neon UI)

---

## 📂 Önerilen Proje Yapısı

```
ChainCert/
├── backend/                 # .NET 8 Minimal API (CertificateIssuer)
│   ├── Program.cs
│   ├── Services/
│   │   ├── BlockchainService.cs
│   │   ├── IpfsService.cs
│   │   └── HashService.cs
│   ├── Models/
│   │   └── CertMetadata.cs
│   └── appsettings.json
├── smart-contract/          # Solidity kontrat
│   ├── CertificateRegistry.sol
│   └── abi/
│       └── CertificateRegistry.abi
├── frontend/                # UI
│   ├── index.html
│   ├── verify.html          # (opsiyonel)
│   ├── style.css
│   └── script.js
├── screenshots/             # README/Sunum ekran görüntüleri
├── .env.example
├── .gitignore
└── README.md
```

---

## ✅ Kurulum (Local)

### 0) Gereksinimler

* Git
* .NET 8 SDK
* Node.js (isteğe bağlı — UI’ı servis etmek için)
* MetaMask eklentisi
* Ganache (desktop)
* Pinata hesabı (JWT için)

---

## 1) Ganache Başlat

Ganache çalıştır → RPC URL genelde:

* `http://127.0.0.1:7545`
* ChainId: `1337`

Ganache hesabını MetaMask’e import et:

* Ganache → Accounts → **Private Key**
* MetaMask → Import Account → private key yapıştır


---

## 2) Smart Contract Deploy (Remix)

1. Remix’te `CertificateRegistry.sol` aç
2. Solidity compiler: **0.8.x**
3. Deploy & Run:

   * Environment: **Injected Provider - MetaMask**
4. MetaMask’te ağ olarak **Ganache** seç
5. Deploy → sözleşme adresini al (**CONTRACT_ADDRESS**)

---

## 3) Backend `.env` Ayarla

`backend/` kök dizinine `.env` koy.

`.env` örneği:

```env
PINATA_JWT=your_pinata_jwt_here
PRIVATE_KEY=your_ganache_private_key_here
RPC_URL=http://127.0.0.1:7545
CONTRACT_ADDRESS=0xYourContractAddress
```


---

## 4) Backend Çalıştır

```bash
cd backend
dotnet restore
dotnet run
```

Varsayılan:

* API: `http://localhost:5000`

---

## 5) Frontend’i Aç

### Seçenek A) Direkt aç

`frontend/index.html` dosyasını çift tıkla.

### Seçenek B) Basit local server ile aç (önerilir)

```bash
cd frontend
python -m http.server 8000
```

Sonra:

* `http://127.0.0.1:8000`

UI içinde `API_BASE` olarak `http://localhost:5000` kullanılır.

---

## 🔌 API Endpoints

### POST `/api/issue`

**Form-data**

* `file`: PDF (`application/pdf`)
* `student`: öğrenci adı
* `(opsiyonel) to`: (bazı versiyonlarda)

**Response örneği**

```json
{
  "tx": "0x...",
  "sha256": "5dd19c...",
  "pdfCid": "Qm...",
  "tokenUri": "ipfs://Qm..."
}
```

### GET `/api/verify?hash=<sha256>`

**Response örneği**

```json
{
  "exists": true,
  "issuer": "0x...",
  "issuedAt": 1763387157,
  "studentName": "Simay Ayanoğlu",
  "description": "ipfs://Qm..."
}
```

---

## 🔐 Güvenlik Notları (GitHub’a Yüklemeden Önce)

Repo’ya **asla** şunları koyma:

* `.env`
* Private key / seed phrase
* Pinata JWT / API key

Bunun yerine:

* `.env.example` (placeholder’lı)
* README’de kurulum adımları

---

## 🧪 Sık Karşılaşılan Hatalar

### 1) `insufficient funds`

MetaMask’in bağlı olduğu Ganache hesabında test ETH var mı kontrol et.

### 2) `invalid opcode` / EVM uyumsuzluğu

Remix compiler sürümü ile environment EVM sürümü uyumlu olmalı.
Ganache + Remix için genelde **0.8.x** uygundur.

### 3) CORS

UI `localhost:8000`, API `localhost:5000` olacağı için CORS açık olmalı.
API’de `AllowAnyOrigin` (veya uygun origin whitelist) aktif olmalı.


---

## 📚 Kaynaklar

* Ethereum Documentation
* Solidity Documentation
* IPFS Documentation
* Pinata Documentation
* Nethereum Documentation
* MetaMask Developer Docs
* Remix IDE Documentation
* Ganache Documentation
