# PasswordManager

Zero-knowledge mimarili, kişisel bir şifre yöneticisi projesi. Asıl amaç uygulamayı kullanmak değil — **C# ve .NET ekosistemini kurumsal seviyede (Clean Architecture, SOLID, Design Patterns) derinlemesine öğrenmek.**

## Tech Stack

- **Backend:** C# / .NET 10 Web API (Controller tabanlı)
- **Veritabanı:** PostgreSQL + Entity Framework Core
- **Client (planlanan):** .NET MAUI (masaüstü + mobil, tek kod tabanı)
- **Kimlik doğrulama:** JWT (elle yazılmış `IAuthService`, ASP.NET Core Identity kullanılmıyor — öğrenme amaçlı)

## Mimari: Clean Architecture

```
PasswordManager.Domain          <- hiçbir katmana bağımlı değil (saf entity'ler)
        ^
PasswordManager.Application     <- sadece Domain'e bağımlı (interface'ler, DTO'lar, use-case'ler)
        ^
PasswordManager.Infrastructure  <- Application'ı implemente eder (EF Core, JWT, Argon2)
        ^
PasswordManager.API             <- HTTP katmanı (Controller'lar, middleware)
```

Bağımlılıklar hep içe doğru akar (Dependency Inversion) — Infrastructure, Application'daki soyut arayüzlere bağımlıdır, tam tersi değil.

## Zero-Knowledge Mimarisi

Sunucu **hiçbir zaman** kullanıcının ana şifresini (master password) ya da şifrelenmemiş vault verisini görmez.

- Master password, client'ta Argon2id ile iki farklı anahtar türetir: **AuthKey** (sunucuya gönderilir, kimlik doğrulama için) ve **EncryptionKey** (sunucuya asla gitmez, sadece client'ta kalır).
- **Envelope encryption:** Vault içeriği, rastgele üretilen bir AES-256 **Vault Key** ile şifrelenir. Vault Key'in kendisi de EncryptionKey ile "wrap" edilip `WrappedVaultKey` olarak saklanır. Master password değişince sadece bu wrap'lenmiş anahtar yeniden şifrelenir, vault içeriğine dokunulmaz.
- Sunucuya gelen `AuthKey` bile çıplak saklanmaz — bir kez daha hashlenip `AuthHash` olarak veritabanına yazılır.

## Yol Haritası

### Domain Katmanı
- [x] `BaseEntity` (Id, CreatedAt, ModifiedAt)
- [x] `User` (Email, AuthHash, AuthSalt, EncryptionSalt, WrappedVaultKey, KdfIterations)
- [x] `VaultItem` (EncryptedData, Nonce)
- [x] `RefreshToken` (TokenHash, ExpiresAt, RevokedAt)

### Application Katmanı
- [x] `IUserRepository`
- [x] `IVaultItemRepository`
- [x] Auth DTO'ları: `RegisterRequest`, `LoginRequest`, `SaltResponse`, `AuthResponse`
- [ ] `IRefreshTokenRepository`
- [ ] `IAuthService` (register/login/refresh/salt akışlarının orkestrasyonu)
- [ ] Vault item DTO'ları ve `IVaultItemService`

### Infrastructure Katmanı
- [ ] `AppDbContext` (EF Core) + entity konfigürasyonları (Fluent API)
- [ ] Repository implementasyonları (`UserRepository`, `VaultItemRepository`, `RefreshTokenRepository`)
- [ ] `AuthService` implementasyonu (Argon2/bcrypt server-side hash, JWT üretimi)
- [ ] PostgreSQL bağlantısı + ilk migration
- [ ] Dependency Injection kayıtları

### API Katmanı
- [ ] `AuthController` (`POST /auth/register`, `POST /auth/login`, `GET /auth/salt`, `POST /auth/refresh`)
- [ ] `VaultController` (CRUD)
- [ ] JWT Bearer authentication middleware
- [ ] Global exception handling middleware
- [ ] `dotnet user-secrets` ile sır yönetimi (connection string, JWT signing key)

### Client — .NET MAUI (henüz başlanmadı)
- [ ] Proje iskeleti
- [ ] Client-side Argon2id key derivation
- [ ] Register / Login ekranları
- [ ] `SecureStorage` ile token saklama
- [ ] Vault listeleme / ekleme / düzenleme ekranları
- [ ] AES-256-GCM ile vault item şifreleme/çözme

## Notlar

- Bu proje bir C# öğrenme sürecinin parçası olarak, adım adım ve her katmanın "neden" o şekilde tasarlandığı açıklanarak geliştiriliyor.
- `Microsoft.OpenApi` paketinde bilinen bir güvenlik açığı (NU1903) uyarısı mevcut, henüz güncellenmedi.
