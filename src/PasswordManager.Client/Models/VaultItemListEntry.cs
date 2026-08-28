namespace PasswordManager.Client.Models;

public record VaultItemListEntry(
    Guid Id,
    VaultItemPayload Payload
);