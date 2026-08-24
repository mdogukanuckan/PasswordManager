namespace PasswordManager.Contracts.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string WrappedVaultKey,
    string WrappedVaultKeyNonce);