using System.Net.Http.Headers;
using PasswordManager.Client.Services;

namespace PasswordManager.Client.Services.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorageService;

    public AuthHeaderHandler(ITokenStorageService tokenStorageService)
    {
        _tokenStorageService = tokenStorageService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await _tokenStorageService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}