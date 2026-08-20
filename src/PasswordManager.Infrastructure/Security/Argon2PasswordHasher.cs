using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using PasswordManager.Application.Interfaces.Services;

namespace PasswordManager.Infrastructure.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MemorySize = 9216;
    private const int Iterations = 4;
    private const int DegreeOfParallelism = 1;
    public string Hash(string input)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = ComputeHash(input, salt);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string input, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] actualHash = ComputeHash(input, salt);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);

    }

    private static byte[] ComputeHash(string input, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(input))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        return argon2.GetBytes(HashSize);
    }


}