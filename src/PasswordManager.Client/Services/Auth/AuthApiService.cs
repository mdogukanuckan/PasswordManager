using System.Net.Http.Json;
using System.Text.Json;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Http;
using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Client.Services.Auth;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>(
                ClientJsonOptions.Options);

            return result!;
        }

        string message = "Bilinmeyen hata";

        try
        {
            var problemDetails =
                await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(
                    ClientJsonOptions.Options);

            if (!string.IsNullOrWhiteSpace(problemDetails?.Detail))
            {
                message = problemDetails.Detail;
            }
        }
        catch (JsonException)
        {
            
        }

        throw new ApiException((int)response.StatusCode, message);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/login",
            request,
            ClientJsonOptions.Options);

        return await HandleResponseAsync<AuthResponse>(response);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/register",
            request,
            ClientJsonOptions.Options);

        return await HandleResponseAsync<AuthResponse>(response);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/refresh",
            request,
            ClientJsonOptions.Options);

        return await HandleResponseAsync<AuthResponse>(response);
    }

    public async Task<SaltResponse> GetSaltAsync(string email)
    {
        var response = await _httpClient.GetAsync(
            $"api/Auth/salt?email={Uri.EscapeDataString(email)}");

        return await HandleResponseAsync<SaltResponse>(response);
    }
}