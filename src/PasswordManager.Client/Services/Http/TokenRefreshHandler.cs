using System.Net;
using PasswordManager.Client.Services.Auth;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Client.Services.Http;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IAuthApiService _authApiService;
    private readonly ITokenStorageService _tokenStorageService;

    public TokenRefreshHandler(
        IAuthApiService authApiService,
        ITokenStorageService tokenStorageService)
    {
        _authApiService = authApiService;
        _tokenStorageService = tokenStorageService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return response;

        try
        {
            var authResponse = await _authApiService.RefreshAsync(
                new RefreshRequest(refreshToken));

            await _tokenStorageService.UpdateTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken);
        }
        catch (ApiException)
        {
            _tokenStorageService.ClearTokens();
            return response;
        }

        using var retryRequest = new HttpRequestMessage(
            request.Method,
            request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            retryRequest.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);
        }

        return await base.SendAsync(retryRequest, cancellationToken);
    }
}