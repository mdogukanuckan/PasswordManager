namespace PasswordManager.Application.DTOs.Auth;

public record RegisterRequest
(
    string Email,
    string AuthKey,
    string AuthSalt,
    string EncryptionSalt,
    string WrappedVaultKey);