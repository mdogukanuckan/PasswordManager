namespace PasswordManager.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string AuthKey);