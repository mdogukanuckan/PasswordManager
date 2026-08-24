namespace PasswordManager.Contracts.DTOs.Auth;

public record SaltResponse(
    string AuthSalt,
    string EncryptionSalt,
    int KdfIterations,
    int KdfMemorySize,
    int KdfParallelism
);