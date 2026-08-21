using PasswordManager.Application.Exceptions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PasswordManager.Application.DTOs.Auth;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Application.Options;
using PasswordManager.Domain.Entities;


namespace PasswordManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthOptions _authOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(IUserRepository userRepository, IOptions<AuthOptions> authOptions,
     IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator, IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _authOptions = authOptions.Value;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }
    public async Task<SaltResponse> GetSaltAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user is not null)
        {
            return new SaltResponse(
                user.AuthSalt,
                user.EncryptionSalt,
                user.KdfIterations,
                user.KdfMemorySize,
                user.KdfParallelism
            );
        }
        byte[] pepperBytes = Convert.FromBase64String(_authOptions.EmailHmacPepper);
        string fakeAuthSalt = ComputeFakeSalt(pepperBytes, normalizedEmail, "auth");
        string fakeEncryptionSalt = ComputeFakeSalt(pepperBytes, normalizedEmail, "enc");
        return new SaltResponse(
            fakeAuthSalt,
            fakeEncryptionSalt,
            KdfIterations: 2,
            KdfMemorySize: 19456,
            KdfParallelism: 1
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null || !_passwordHasher.Verify(request.AuthKey, user.AuthHash))
        {
            throw new InvalidCredentialsException();
        }
        return await IssueAuthResponseAsync(user);

    }

    public Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (existingUser is not null)
        {
            throw new EmailAlreadyExistsException();
        }

        var user = new User
        {
            Email = normalizedEmail,
            AuthHash = _passwordHasher.Hash(request.AuthKey),
            AuthSalt = request.AuthSalt,
            EncryptionSalt = request.EncryptionSalt,
            WrappedVaultKey = request.WrappedVaultKey,
            WrappedVaultKeyNonce = request.WrappedVaultKeyNonce
        };

        await _userRepository.AddAsync(user);

        return await IssueAuthResponseAsync(user);
    }

    private static string ComputeFakeSalt(byte[] pepper, string normalizedEmail, string context)
    {
        using var hmac = new HMACSHA256(pepper);
        byte[] input = Encoding.UTF8.GetBytes($"{context}:{normalizedEmail}");
        byte[] hash = hmac.ComputeHash(input);
        return Convert.ToBase64String(hash[..16]);
    }

    private async Task<AuthResponse> IssueAuthResponseAsync(User user)
    {
        string accessToken = _tokenGenerator.GenerateAccessToken(user);
        string refreshToken = _tokenGenerator.GenerateRefreshToken();
        string refreshTokenHash = _tokenGenerator.HashRefreshToken(refreshToken);
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        return new AuthResponse(
            accessToken,
            refreshToken,
            user.WrappedVaultKey,
            user.WrappedVaultKeyNonce
        );
    }
}