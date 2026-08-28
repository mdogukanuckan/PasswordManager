namespace PasswordManager.Client.Models;

public record VaultItemPayload(
    string Title,
    string Username,
    string Password,
    string Notes
);