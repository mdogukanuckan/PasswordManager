namespace PasswordManager.Client.Services.Http;

public record ProblemDetailsDto
(
    string? Title,
    string? Detail,
    int? Status
);