namespace PasswordManager.Client.Services;

public interface IKeyDerivationService
{
    Task<byte[]> DeriveKeyAsync(string password, string saltBase64, int iterations, int memorySizeKb, int parallelism);
}