using System.Text.Json;

namespace PasswordManager.Client.Services.Http;

public static class ClientJsonOptions
{
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions(JsonSerializerDefaults.Web);
}