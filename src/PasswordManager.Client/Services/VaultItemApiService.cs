using System.Net.Http.Json;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Http;
using PasswordManager.Contracts.DTOs.VaultItem;

namespace PasswordManager.Client.Services;

public class VaultItemApiService : IVaultItemApiService
{
    private readonly HttpClient _httpClient;

    public VaultItemApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<VaultItemResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/VaultItem");
        return await HandleResponseAsync<IReadOnlyList<VaultItemResponse>>(response);
    }

    public async Task<VaultItemResponse> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/VaultItem/{id}");
        return await HandleResponseAsync<VaultItemResponse>(response);
    }

    public async Task<VaultItemResponse> CreateAsync(CreateVaultItemRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/VaultItem", request, ClientJsonOptions.Options);
        return await HandleResponseAsync<VaultItemResponse>(response);
    }

    public async Task<VaultItemResponse> UpdateAsync(Guid id, UpdateVaultItemRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/VaultItem/{id}", request, ClientJsonOptions.Options);
        return await HandleResponseAsync<VaultItemResponse>(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/VaultItem/{id}");

        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync<object>(response);
        }
    }

    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            if (typeof(T) == typeof(object))
            {
                return default!;
            }

            var result = await response.Content.ReadFromJsonAsync<T>(ClientJsonOptions.Options);
            return result!;
        }

        string message = "Bilinmeyen hata";

        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ClientJsonOptions.Options);

            if (!string.IsNullOrWhiteSpace(problemDetails?.Detail))
            {
                message = problemDetails.Detail;
            }
        }
        catch (System.Text.Json.JsonException)
        {
        }

        throw new ApiException((int)response.StatusCode, message);
    }
}