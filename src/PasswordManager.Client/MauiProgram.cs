using Microsoft.Extensions.Logging;
using PasswordManager.Client.Services;
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
			client.BaseAddress = DeviceInfo.Current.Platform == DevicePlatform.Android
				? new Uri("http://172.17.10.63:5273/")
				: new Uri("http://localhost:5273/");
		});
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<LoginPage>();

		builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();

		return builder.Build();
	}
}
