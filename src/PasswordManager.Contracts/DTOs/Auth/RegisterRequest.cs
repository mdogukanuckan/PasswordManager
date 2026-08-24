namespace PasswordManager.Contracts.DTOs.Auth;

public record RegisterRequest
(
    string Email,
    string AuthKey,
    string AuthSalt,
    string EncryptionSalt,
    string WrappedVaultKey,
    string WrappedVaultKeyNonce);