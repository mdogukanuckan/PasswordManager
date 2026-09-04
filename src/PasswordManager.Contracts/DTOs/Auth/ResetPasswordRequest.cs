namespace PasswordManager.Contracts.DTOs.Auth;

public record ResetPasswordRequest(
    string Token,
    string AuthKey,
    string AuthSalt,
    string EncryptionSalt,
    string WrappedVaultKey,
    string WrappedVaultKeyNonce
);