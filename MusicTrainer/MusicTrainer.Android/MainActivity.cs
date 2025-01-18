using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using MusicTrainer.Android.Logic.AudioManager;
using MusicTrainer.Logic.AudioManager;

namespace MusicTrainer.Android;

[Activity(
    Label = "MusicTrainer.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        CustomServiceProvider.InitializeServiceProvider(AddPlatformServices);
        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
    
    /// <summary>
    /// Register platform-specific services.
    /// </summary>
    static void AddPlatformServices(IServiceCollection collection)
    {
        collection.AddScoped<IAudioManager, AndroidAudioManager>();
    }
}