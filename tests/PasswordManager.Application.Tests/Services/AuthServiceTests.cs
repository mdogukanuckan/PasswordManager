using Moq;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Application.Options;
using PasswordManager.Application.Services;
using static Microsoft.Extensions.Options.Options;
using FluentAssertions;
using PasswordManager.Domain.Entities;
using PasswordManager.Application.DTOs.Auth;
using PasswordManager.Application.Exceptions;
namespace PasswordManager.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();

        var authOptions = Create(new AuthOptions { EmailHmacPepper = "dGVzdC1wZXBwZXI=" });
        _sut = new AuthService(
    _userRepositoryMock.Object,
    authOptions,
    _passwordHasherMock.Object,
    _tokenGeneratorMock.Object,
    _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSaltAsync_ExistingUser_ReturnsUsersRealSalt()
    {
        // Arrange
        var existingUser = new User
        {
            Email = "test@example.com",
            AuthSalt = "real-auth-salt",
            EncryptionSalt = "real-encryption-salt",
            KdfIterations = 3,
            KdfMemorySize = 65536,
            KdfParallelism = 2
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.GetSaltAsync("test@example.com");

        // Assert
        result.AuthSalt.Should().Be(existingUser.AuthSalt);
        result.EncryptionSalt.Should().Be(existingUser.EncryptionSalt);
        result.KdfIterations.Should().Be(existingUser.KdfIterations);
        result.KdfMemorySize.Should().Be(existingUser.KdfMemorySize);
        result.KdfParallelism.Should().Be(existingUser.KdfParallelism);
    }

    [Fact]
    public async Task GetSaltAsync_NonExistingUser_ReturnsDeterministicFakeSalt()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("ghost@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        var firstCall = await _sut.GetSaltAsync("ghost@example.com");
        var secondCall = await _sut.GetSaltAsync("ghost@example.com");

        // Assert
        firstCall.Should().BeEquivalentTo(secondCall);
    }

    [Fact]
    public async Task GetSaltAsync_TwoDifferentNonExistingEmails_ReturnDifferentFakeSalts()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("ghost1@example.com")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("ghost2@example.com")).ReturnsAsync((User?)null);

        // Act
        var result1 = await _sut.GetSaltAsync("ghost1@example.com");
        var result2 = await _sut.GetSaltAsync("ghost2@example.com");

        // Assert
        result1.AuthSalt.Should().NotBe(result2.AuthSalt);
        result1.EncryptionSalt.Should().NotBe(result2.EncryptionSalt);
    }
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var existingUser = new User
        {
            Email = "test@example.com",
            AuthHash = "stored-hash",
            WrappedVaultKey = "wrapped-vault-key",
            WrappedVaultKeyNonce = "wrapped-vault-key-nonce"
        };
        var request = new LoginRequest("test@example.com", "correct-auth-key");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(h => h.Verify("correct-auth-key", "stored-hash"))
            .Returns(true);

        _tokenGeneratorMock
            .Setup(t => t.GenerateAccessToken(existingUser))
            .Returns("fake-access-token");

        _tokenGeneratorMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _tokenGeneratorMock
            .Setup(t => t.HashRefreshToken("fake-refresh-token"))
            .Returns("fake-refresh-token-hash");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.WrappedVaultKey.Should().Be(existingUser.WrappedVaultKey);
        result.WrappedVaultKeyNonce.Should().Be(existingUser.WrappedVaultKeyNonce);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var existingUser = new User
        {
            Email = "test@example.com",
            AuthHash = "stored-hash"
        };
        var request = new LoginRequest("test@example.com", "wrong-auth-key");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(h => h.Verify("wrong-auth-key", "stored-hash"))
            .Returns(false);

        // Act
        Func<Task> act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_NonExistingEmail_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("ghost@example.com", "any-auth-key");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("ghost@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
    [Fact]
    public async Task RegisterAsync_NewUser_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest(
            "new@example.com",
            "auth-key",
            "auth-salt",
            "encryption-salt",
            "wrapped-vault-key",
            "wrapped-vault-key-nonce");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("new@example.com"))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.Hash("auth-key"))
            .Returns("hashed-auth-key");

        _tokenGeneratorMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fake-access-token");

        _tokenGeneratorMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _tokenGeneratorMock
            .Setup(t => t.HashRefreshToken("fake-refresh-token"))
            .Returns("fake-refresh-token-hash");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.WrappedVaultKey.Should().Be(request.WrappedVaultKey);
        result.WrappedVaultKeyNonce.Should().Be(request.WrappedVaultKeyNonce);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Email == "new@example.com" &&
                u.AuthHash == "hashed-auth-key" &&
                u.AuthSalt == "auth-salt" &&
                u.EncryptionSalt == "encryption-salt")),
            Times.Once);
    }
    [Fact]
    public async Task RegisterAsync_ExistingEmail_ThrowsEmailAlreadyExistsException()
    {
        // Arrange
        var existingUser = new User { Email = "taken@example.com" };
        var request = new RegisterRequest(
            "taken@example.com",
            "auth-key",
            "auth-salt",
            "encryption-salt",
            "wrapped-vault-key",
            "wrapped-vault-key-nonce");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("taken@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<EmailAlreadyExistsException>();

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }
    [Fact]
    public async Task RefreshAsync_ValidToken_RevokesOldTokenAndReturnsNewAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            WrappedVaultKey = "wrapped-vault-key",
            WrappedVaultKeyNonce = "wrapped-vault-key-nonce"
        };
        var existingToken = new RefreshToken
        {
            User = user,
            TokenHash = "old-token-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = null
        };

        _tokenGeneratorMock
            .Setup(t => t.HashRefreshToken("raw-refresh-token"))
            .Returns("old-token-hash");

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("old-token-hash"))
            .ReturnsAsync(existingToken);

        _tokenGeneratorMock
            .Setup(t => t.GenerateAccessToken(user))
            .Returns("new-access-token");

        _tokenGeneratorMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _tokenGeneratorMock
            .Setup(t => t.HashRefreshToken("new-refresh-token"))
            .Returns("new-refresh-token-hash");

        // Act
        var result = await _sut.RefreshAsync("raw-refresh-token");

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        _refreshTokenRepositoryMock.Verify(r => r.RevokeAsync(existingToken), Times.Once);
    }

    [Theory]
[InlineData(false, false, false)] // token hiç bulunamadı
[InlineData(true, true, false)]   // token revoke edilmiş
[InlineData(true, false, true)]   // token süresi dolmuş
public async Task RefreshAsync_InvalidToken_ThrowsInvalidCredentialsException(
    bool tokenExists, bool isRevoked, bool isExpired)
{
    // Arrange
    RefreshToken? existingToken = null;
    if (tokenExists)
    {
        existingToken = new RefreshToken
        {
            User = new User(),
            TokenHash = "some-token-hash",
            RevokedAt = isRevoked ? DateTime.UtcNow : null,
            ExpiresAt = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1)
        };
    }

    _tokenGeneratorMock
        .Setup(t => t.HashRefreshToken("raw-refresh-token"))
        .Returns("some-token-hash");

    _refreshTokenRepositoryMock
        .Setup(r => r.GetByTokenHashAsync("some-token-hash"))
        .ReturnsAsync(existingToken);

    // Act
    Func<Task> act = () => _sut.RefreshAsync("raw-refresh-token");

    // Assert
    await act.Should().ThrowAsync<InvalidCredentialsException>();
}
}
