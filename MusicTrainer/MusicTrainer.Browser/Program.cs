using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Extensions.DependencyInjection;
using MusicTrainer;
using MusicTrainer.Browser.Logic.AudioManager;
using MusicTrainer.Logic.AudioManager;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        // Register DI services
        CustomServiceProvider.InitializeServiceProvider(AddPlatformServices);
        
        return AppBuilder.Configure<App>()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    /// <summary>
    /// Register platform-specific services.
    /// </summary>
    static void AddPlatformServices(IServiceCollection collection)
    {
        collection.AddScoped<IAudioManager, BrowserAudioManager>();
    }
}