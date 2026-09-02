using System.Security.Cryptography;

namespace PasswordManager.Client.Services.PasswordGeneration;

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const int PasswordLength = 16;
    private const int GuaranteedCharacterCount = 4;
    private const int RandomCharacterCount = PasswordLength - GuaranteedCharacterCount;
    public string Generate()
    {

        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*-_=+";
        const string charset = uppercase + lowercase + digits + symbols;

        char[] password = new char[PasswordLength];

        password[0] = RandomNumberGenerator.GetItems<char>(uppercase, 1)[0];
        password[1] = RandomNumberGenerator.GetItems<char>(lowercase, 1)[0];
        password[2] = RandomNumberGenerator.GetItems<char>(digits, 1)[0];
        password[3] = RandomNumberGenerator.GetItems<char>(symbols, 1)[0];

        char[] remaining = RandomNumberGenerator.GetItems<char>(charset, RandomCharacterCount);

        Array.Copy(remaining, 0, password, GuaranteedCharacterCount, RandomCharacterCount);

        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);

            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}