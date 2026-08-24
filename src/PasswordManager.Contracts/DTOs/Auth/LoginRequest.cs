namespace PasswordManager.Contracts.DTOs.Auth;

public record LoginRequest(
    string Email,
    string AuthKey);