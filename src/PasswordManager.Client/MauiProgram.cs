using Microsoft.Extensions.Logging;
using PasswordManager.Client.Services.Auth;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Client.Services.Http;
using PasswordManager.Client.ViewModels;
using PasswordManager.Client.Views;

namespace PasswordManager.Client;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>

	{
		client.BaseAddress = GetApiBaseAddress();
	});
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<AddVaultItemViewModel>();
		builder.Services.AddTransient<AddVaultItemPage>();
		builder.Services.AddTransient<VaultItemDetailViewModel>();
		builder.Services.AddTransient<VaultItemDetailPage>();

		builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
		builder.Services.AddSingleton<IKeyDerivationService, KeyDerivationService>();
		builder.Services.AddSingleton<IVaultCryptoService, VaultCryptoService>();
		builder.Services.AddSingleton<IVaultSessionService, VaultSessionService>();
		builder.Services.AddSingleton<IVaultItemMapper, VaultItemMapper>();

		builder.Services.AddTransient<AuthHeaderHandler>();

		builder.Services.AddHttpClient<IVaultItemApiService, VaultItemApiService>(client =>
		{
			client.BaseAddress = GetApiBaseAddress();
		})
		.AddHttpMessageHandler<AuthHeaderHandler>();


		return builder.Build();
	}

	private static Uri GetApiBaseAddress()
	{
		return DeviceInfo.Current.Platform == DevicePlatform.Android
			? new Uri("http://172.17.10.63:5273/")
			: new Uri("http://localhost:5273/");
	}
}
