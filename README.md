# PasswordManager

Zero-knowledge mimarili, kişisel bir şifre yöneticisi projesi. Asıl amaç uygulamayı kullanmak değil — **C# ve .NET ekosistemini kurumsal seviyede (Clean Architecture, SOLID, Design Patterns) derinlemesine öğrenmek.**

## Tech Stack

- **Backend:** C# / .NET 10 Web API (Controller tabanlı)
- **Veritabanı:** PostgreSQL + Entity Framework Core
- **Client (planlanan):** .NET MAUI (masaüstü + mobil, tek kod tabanı)
- **Kimlik doğrulama:** JWT (elle yazılmış `IAuthService`, ASP.NET Core Identity kullanılmıyor — öğrenme amaçlı)
- **Test:** xUnit + Moq + FluentAssertions (`tests/PasswordManager.Application.Tests`)

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
- **Envelope encryption:** Vault içeriği, rastgele üretilen bir AES-256 **Vault Key** ile şifrelenir. Vault Key'in kendisi de EncryptionKey ile "wrap" edilip `WrappedVaultKey` (+ `WrappedVaultKeyNonce`) olarak saklanır. Master password değişince sadece bu wrap'lenmiş anahtar yeniden şifrelenir, vault içeriğine dokunulmaz.
- Sunucuya gelen `AuthKey` bile çıplak saklanmaz — bir kez daha hashlenip `AuthHash` olarak veritabanına yazılır.
- **Email enumeration koruması:** Var olmayan bir email için `GetSaltAsync`, gerçek bir kullanıcıymış gibi davranıp deterministik bir "sahte salt" döner (`HMAC-SHA256(pepper, context + email)`), böylece response'un varlığından/şeklinden email'in kayıtlı olup olmadığı anlaşılamaz. Login'de de aynı ilke geçerli: yanlış şifre ve var olmayan email, aynı `InvalidCredentialsException`'ı fırlatır.

## Alınan mimari kararlar

- **Client-side KDF:** Argon2id, 19 MiB bellek / 2 iterasyon / 1 paralellik.
- **Sunucu tarafı rehash:** Argon2id, 9 MiB bellek / 4 iterasyon / 1 paralellik (`AuthHash` bu profille üretilir — client'ın kullandığı profilden bilerek farklı, sunucu maliyeti client'tan bağımsız ayarlanabilsin diye).
- **JWT:** HS256, access token 30 dakika, refresh token 7 gün. Refresh token JWT değil — 64 byte rastgele, Base64 opak string; sunucuda SHA-256 hash'i saklanır.
- **Refresh token rotasyonu:** Her `RefreshAsync` çağrısında eski token iptal edilir (`RevokedAt` set edilir), yepyeni bir access+refresh çifti üretilir (OAuth2 RFC pratiği).
- **Register enumeration:** Login'in aksine, kayıt sırasında email zaten kayıtlıysa bunu açıkça bildiriyoruz (`EmailAlreadyExistsException`, 409) — Bitwarden gibi ürünlerdeki yaygın pratikle tutarlı bir tercih.
- **Application katmanı, `Microsoft.Extensions.Configuration`'dan bağımsız:** Config değerleri (`Auth:EmailHmacPepper` gibi) `IOptions<T>` deseniyle enjekte ediliyor — Dependency Inversion, ve test'te mock'lamayı kolaylaştırıyor.

## Yol Haritası

### Domain Katmanı
- [x] `BaseEntity` (Id, CreatedAt, ModifiedAt)
- [x] `User` (Email, AuthHash, AuthSalt, EncryptionSalt, WrappedVaultKey, WrappedVaultKeyNonce, KdfIterations, KdfMemorySize, KdfParallelism)
- [x] `VaultItem` (EncryptedData, Nonce)
- [x] `RefreshToken` (TokenHash, ExpiresAt, RevokedAt)

### Application Katmanı
- [x] `IUserRepository`
- [x] `IVaultItemRepository`
- [x] `IRefreshTokenRepository`
- [x] Auth DTO'ları: `RegisterRequest`, `LoginRequest`, `SaltResponse`, `AuthResponse`
- [x] `IAuthService` (register/login/refresh/salt akışlarının orkestrasyonu)
- [x] Vault item DTO'ları ve `IVaultItemService`
- [x] `AuthService` implementasyonu — 4 metot da tamamlandı ve **tam test kapsamında (12/12 unit test, xUnit+Moq+FluentAssertions)**
- [ ] `VaultItemService` implementasyonu — henüz başlanmadı

### Infrastructure Katmanı
- [x] `AppDbContext` (EF Core) + entity konfigürasyonları (Fluent API)
- [x] Repository implementasyonları (`UserRepository`, `VaultItemRepository`, `RefreshTokenRepository`)
- [x] `Argon2PasswordHasher`, `JwtTokenGenerator`
- [x] PostgreSQL kurulumu + bağlantı (`dotnet user-secrets` ile connection string)
- [ ] İlk migration (`dotnet ef migrations add` + `dotnet ef database update`)
- [ ] Dependency Injection kayıtları (`Program.cs` — `AddDbContext`, repository/servis kayıtları; şu an sadece `AuthOptions` binding'i var)

### API Katmanı
- [ ] `AuthController` (`POST /auth/register`, `POST /auth/login`, `GET /auth/salt`, `POST /auth/refresh`)
- [ ] `VaultController` (CRUD)
- [ ] JWT Bearer authentication middleware
- [ ] Global exception handling middleware (`InvalidCredentialsException`→401, `EmailAlreadyExistsException`→409)
- [ ] `dotnet user-secrets` ile JWT signing key yönetimi (connection string zaten user-secrets'ta)

### Test
- [x] `tests/PasswordManager.Application.Tests` projesi (xUnit + Moq + FluentAssertions, Central Package Management ile)
- [x] `AuthService` unit testleri — 12/12 yeşil (salt/login/register/refresh, enumeration koruması ve token rotasyonu dahil)
- [ ] `VaultItemService` unit testleri (servis yazılınca)
- [ ] `Argon2PasswordHasher` / `JwtTokenGenerator` için Infrastructure katmanı testleri (kapsam dışı bırakıldı, ileride ayrı bir konu)

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
- Test projesi `FluentAssertions` v8+ kullanıyor — bireysel/ticari-olmayan kullanım için ücretsiz (Xceed Community License), ticari kullanım ayrı bir lisans gerektiriyor.
