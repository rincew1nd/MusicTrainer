using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using MusicTrainer.Desktop.Logic.AudioManager;
using MusicTrainer.Logic.AudioManager;
using MusicTrainer.ViewModels;
using NAudio.Wave;

namespace MusicTrainer.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Register DI services
        CustomServiceProvider.InitializeServiceProvider(AddPlatformServices);
        
        // Configure and start app
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Register platform-specific services.
    /// </summary>
    static void AddPlatformServices(IServiceCollection collection)
    {
        collection.AddScoped<IAudioManager, SpeakerAudioManager>();
    }
}