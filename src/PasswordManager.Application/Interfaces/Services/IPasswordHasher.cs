namespace PasswordManager.Application.Interfaces.Services;

public interface IPasswordHasher
{
    string Hash(string input);
    bool Verify(string input, string hash);
}