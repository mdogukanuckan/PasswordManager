using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PasswordManager.Application.DTOs.Auth;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Application.Options;


namespace PasswordManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthOptions _authOptions;

    public AuthService(IUserRepository userRepository, IOptions<AuthOptions> authOptions)
    {
        _userRepository = userRepository;
        _authOptions = authOptions.Value;
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
        string fakeAuthSalt = ComputeFakeSalt(pepperBytes,normalizedEmail,"auth");
        string fakeEncryptionSalt = ComputeFakeSalt(pepperBytes,normalizedEmail,"enc");
        return new SaltResponse(
            fakeAuthSalt,
            fakeEncryptionSalt,
            KdfIterations:2,
            KdfMemorySize:19456,
            KdfParallelism:1
        );
        }

    public Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        throw new NotImplementedException();
    }

    private static string ComputeFakeSalt(byte[] pepper, string normalizedEmail, string context)
    {
        using var hmac = new HMACSHA256(pepper);
        byte[] input = Encoding.UTF8.GetBytes($"{context}:{normalizedEmail}");
        byte[] hash = hmac.ComputeHash(input);
        return Convert.ToBase64String(hash[..16]);
    }
}