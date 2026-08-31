using System.Text;
using Konscious.Security.Cryptography;

namespace PasswordManager.Client.Services.Auth;

public class KeyDerivationService : IKeyDerivationService
{
    public async Task<byte[]> DeriveKeyAsync(
        string password,
        string saltBase64,
        int iterations,
        int memorySizeKb,
        int parallelism)
    {
        byte[] saltBytes = Convert.FromBase64String(saltBase64);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            Iterations = iterations,
            MemorySize = memorySizeKb,
            DegreeOfParallelism = parallelism
        };

        return await argon2.GetBytesAsync(32);

    }
}