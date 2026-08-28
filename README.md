# PasswordManager

Zero-knowledge mimarili, kişisel bir şifre yöneticisi projesi. Asıl amaç uygulamayı kullanmak değil — **C# ve .NET ekosistemini kurumsal seviyede (Clean Architecture, SOLID, Design Patterns) derinlemesine öğrenmek.**

## Tech Stack

- **Backend:** C# / .NET 10 Web API (Controller tabanlı)
- **Veritabanı:** PostgreSQL + Entity Framework Core
- **Client (geliştiriliyor):** .NET MAUI — Windows masaüstü + Android (iOS/MacCatalyst hedeflenmiyor), `CommunityToolkit.Mvvm` ile MVVM
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

PasswordManager.Contracts       <- hiçbir şeye bağımlı değil (paylaşılan DTO'lar)
        ^                    ^
PasswordManager.Application   PasswordManager.Client
```

Bağımlılıklar hep içe doğru akar (Dependency Inversion) — Infrastructure, Application'daki soyut arayüzlere bağımlıdır, tam tersi değil. `Contracts`, MAUI client'ın backend'in `Application` katmanına (ve onun iş mantığı arayüzlerine) doğrudan referans vermemesi için ayrı çıkarıldı — hem `Application` hem `Client`, DTO'lar için sadece `Contracts`'a bağımlı, derleme zamanında garantili tek doğruluk kaynağı.

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
- **Global exception handling:** Bilinmeyen (500) hatalarda exception'ın `.Message`'ı client'a asla dönmez — sabit, genel bir mesaj döner ve gerçek detay sadece sunucu tarafında `ILogger` ile loglanır (iç implementasyon detaylarının sızmasını önlemek için). Bilinen/mapped exception'larda (`InvalidCredentialsException`→401, `EmailAlreadyExistsException`→409, `NotFoundException`→404) ise kasıtlı yazılmış, güvenli bir `detail` mesajı dönülür — bu bir sızıntı değil, bilinçli bir UX tercihi.
- **Vault item erişiminde IDOR koruması:** `VaultItemService`, bir kaydı her zaman hem `id` hem de o an istek yapan kullanıcının `userId`'siyle birlikte sorgular (`GetByIdAsync(id, userId)`). Kayıt hiç yoksa da, kayıt var ama başka bir kullanıcıya aitse de aynı, ayrım yapmayan `NotFoundException` (404) fırlatılır — email enumeration korumasındaki ilkenin aynısı, bu kez kaynak ID'si için uygulanmış hali.

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
- [x] `VaultItemService` implementasyonu — 5 metot da tamamlandı (CRUD + kullanıcı bazlı yetkilendirme) ve **tam test kapsamında (8/8 unit test)**

### Infrastructure Katmanı
- [x] `AppDbContext` (EF Core) + entity konfigürasyonları (Fluent API)
- [x] Repository implementasyonları (`UserRepository`, `VaultItemRepository`, `RefreshTokenRepository`)
- [x] `Argon2PasswordHasher`, `JwtTokenGenerator`
- [x] PostgreSQL kurulumu + bağlantı (`dotnet user-secrets` ile connection string)
- [x] İlk migration (`dotnet ef migrations add` + `dotnet ef database update`)
- [x] Dependency Injection kayıtları (`Program.cs` — `AddDbContext`, repository/servis kayıtları, `AuthOptions` binding'i)

### API Katmanı
- [x] `AuthController` (`POST /auth/register`, `POST /auth/login`, `GET /auth/salt`, `POST /auth/refresh`)
- [x] `VaultItemController` (`GET /api/VaultItem`, `GET /api/VaultItem/{id}`, `POST /api/VaultItem`, `PUT /api/VaultItem/{id}`, `DELETE /api/VaultItem/{id}`)
- [x] JWT Bearer authentication middleware
- [x] Global exception handling middleware (`InvalidCredentialsException`→401, `EmailAlreadyExistsException`→409)
- [x] `dotnet user-secrets` ile JWT signing key yönetimi (connection string zaten user-secrets'ta)

### Test
- [x] `tests/PasswordManager.Application.Tests` projesi (xUnit + Moq + FluentAssertions, Central Package Management ile)
- [x] `AuthService` unit testleri — 12/12 yeşil (salt/login/register/refresh, enumeration koruması ve token rotasyonu dahil)
- [x] Auth API katmanının uçtan uca (E2E) manuel doğrulaması — `dotnet run` + PowerShell `Invoke-RestMethod` ile register → login → refresh rotasyonu → eski token'ın reddi (401) zinciri gerçek bir PostgreSQL veritabanına karşı test edildi
- [x] `VaultItemService` unit testleri — 8/8 yeşil (CRUD akışları + var-olmayan/başkasına-ait kaynak senaryoları dahil) — **toplam 20/20 test yeşil**
- [x] `VaultItemController` API katmanının uçtan uca (E2E) manuel doğrulaması — `dotnet run` + PowerShell `Invoke-WebRequest` ile tokensız istek (401) → login → Create (201 + `Location`) → GetById → GetAll → Update (`ModifiedAt` kontrolü) → Delete (204) → silinen kaydın GetById'si (404) zinciri, ayrıca ikinci bir kullanıcıyla IDOR/enumeration testi (başkasının kaydına erişim denemesi de aynı 404'ü döndü) gerçek bir PostgreSQL veritabanına karşı test edildi
- [ ] `Argon2PasswordHasher` / `JwtTokenGenerator` için Infrastructure katmanı testleri (kapsam dışı bırakıldı, ileride ayrı bir konu)

### Client — .NET MAUI (geliştiriliyor)
- [x] Proje iskeleti (Windows + Android hedefli, `PasswordManager.Contracts` ile paylaşılan DTO'lar)
- [x] MVVM altyapısı (`CommunityToolkit.Mvvm`, `Views/`/`ViewModels/`/`Services/` klasör yapısı)
- [x] Auth API client katmanı (`IAuthApiService`/`AuthApiService`, exception tabanlı hata yönetimi)
- [x] Fiziksel Android cihaz networking'i (Kestrel `0.0.0.0` binding, Windows Firewall, Android cleartext config) — telefon üzerinden doğrulandı
- [x] `SecureStorage` ile token saklama (`ITokenStorageService`, Singleton DI)
- [x] `LoginViewModel` + `LoginPage` — uçtan uca test edildi (gerçek backend + PostgreSQL'e karşı, Windows'ta)
- [x] `HomePage` + login-sonrası Shell navigasyonu (`Shell.Current.GoToAsync("//HomePage")`, absolute route — `HomePage` `AppShell.xaml`'de `ShellContent` olarak tanımlı)
- [x] Client-side Argon2id key derivation (`IKeyDerivationService`) — `LoginViewModel`'e kablolandı, artık gerçek `AuthKey`/`EncryptionKey` türetiliyor, ham şifre backend'e gitmiyor
- [x] AES-256-GCM key wrapping çekirdeği + genel şifreleme (`IVaultCryptoService.WrapKey`/`UnwrapKey` ve `Encrypt`/`Decrypt`) — `WrapKey`/`UnwrapKey` artık `Encrypt`/`Decrypt`'i çağıran ince sarmalayıcılar, aynı AES-256-GCM mantığı hem key wrapping hem genel veri şifreleme için paylaşılıyor
- [x] Register ekranı (`RegisterViewModel` + `RegisterPage`) — client salt'ları kendisi üretir, Argon2id ile `AuthKey`/`EncryptionKey` türetir, rastgele bir `VaultKey` üretip `WrapKey` ile sarmalar, başarılı kayıt sonrası otomatik login + `HomePage` navigasyonu — **uçtan uca test edildi (gerçek backend + PostgreSQL'e karşı)**
- [x] Login sonrası `VaultKey`, `WrappedVaultKey`/`WrappedVaultKeyNonce`'tan `UnwrapKey` ile elde ediliyor — AES-256-GCM'in unwrap tarafı da runtime'da doğrulandı (register'da wrap edilen key, login'de aynı şekilde açılabiliyor)
- [x] `IVaultSessionService`/`VaultSessionService` (Singleton) — `VaultKey`'i sayfalar arası taşımak için, **sadece RAM'de** tutuluyor, hiçbir zaman `SecureStorage`'a/diske yazılmıyor (zero-knowledge garantisini zayıflatmamak için bilinçli bir tercih — access/refresh token'ların aksine)
- [x] `IVaultItemApiService`/`VaultItemApiService` — backend'in `VaultItemController`'ını saran tip-güvenli client katmanı (`AuthApiService` ile aynı desende)
- [x] `AuthHeaderHandler` (`DelegatingHandler`) — `VaultItemApiService`'in her isteğine `ITokenStorageService`'ten okunan access token'ı otomatik `Authorization: Bearer` header'ı olarak ekliyor, servisin kendisi token'dan habersiz kalıyor
- [x] `IVaultItemMapper`/`VaultItemMapper` — plaintext `VaultItemPayload` (Title/Username/Password/Notes) ile backend'in gördüğü opak `EncryptedData`/`Nonce` arasındaki dönüşüm; JSON serileştirme + `IVaultCryptoService.Encrypt`/`Decrypt` burada birleşiyor, backend gerçek alanları asla görmüyor
- [ ] `HomePage`, gerçek bir vault listesine dönüştürüldü (`HomeViewModel` artık `CollectionView` ile `VaultItemListEntry` listesi gösteriyor, sayfa her göründüğünde `OnAppearing` ile `LoadVaultItemsCommand` otomatik tetikleniyor) — kod tarafı tamamlandı, henüz runtime'da (gerçek backend'e karşı) doğrulanmadı
- [ ] Vault item ekleme ekranı
- [ ] Vault item düzenleme/silme ekranları

## Notlar

- Bu proje bir C# öğrenme sürecinin parçası olarak, adım adım ve her katmanın "neden" o şekilde tasarlandığı açıklanarak geliştiriliyor.
- `Microsoft.OpenApi` paketinde bilinen bir güvenlik açığı (NU1903) uyarısı mevcut, henüz güncellenmedi.
- Test projesi `FluentAssertions` v8+ kullanıyor — bireysel/ticari-olmayan kullanım için ücretsiz (Xceed Community License), ticari kullanım ayrı bir lisans gerektiriyor.
- **Zero-Knowledge kısayolu çözüldü:** `LoginViewModel` artık gerçek bir `AuthKey`/`EncryptionKey` çifti türetiyor (client-side Argon2id, `GetSaltAsync`'ten alınan kullanıcıya özel salt'larla) ve backend'e ham şifre yerine bu `AuthKey`'i gönderiyor — önceki geçici stub tamamen kaldırıldı, hem Register hem Login akışı gerçek backend'e karşı uçtan uca doğrulandı.
- **Düşük öncelikli güvenlik notu — `GET /auth/salt` timing side-channel:** Var olan/olmayan kullanıcılar için dönen JSON gövdesi yapısal olarak aynı (email enumeration'a karşı tasarlanmış), ama gerçek kullanıcıda DB sorgusu çalıştığı için yanıt süresi belirgin şekilde daha uzun (manuel test: ~224ms'e karşı ~6ms) — bu fark, yanıt gövdesine hiç bakmadan sadece süreyi ölçerek email enumeration'a imkan tanıyabilir. Olası düzeltme: sahte yanıt yoluna, gerçek DB sorgusu süresine yakın yapay bir gecikme eklemek. Henüz ele alınmadı.
