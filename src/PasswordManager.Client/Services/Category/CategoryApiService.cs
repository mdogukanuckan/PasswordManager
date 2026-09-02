using System.Net.Http.Json;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Http;
using PasswordManager.Contracts.DTOs.Category;

namespace PasswordManager.Client.Services.Category;

public class CategoryApiService : ICategoryApiService
{
    private readonly HttpClient _httpClient;

    public CategoryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/Category");
        return await HandleResponseAsync<IReadOnlyList<CategoryResponse>>(response);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Category", request, ClientJsonOptions.Options);
        return await HandleResponseAsync<CategoryResponse>(response);
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/Category/{id}");
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

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Category/{id}", request, ClientJsonOptions.Options);
        return await HandleResponseAsync<CategoryResponse>(response);

    }
}