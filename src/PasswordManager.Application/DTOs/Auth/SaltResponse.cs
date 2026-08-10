namespace PasswordManager.Application.DTOs.Auth;

public record SaltResponse(
    string AuthSalt,
    string EncryptionSalt,
    int KdfIterations
);