using System;
using Microsoft.Extensions.DependencyInjection;
using MusicTrainer.ViewModels;

namespace MusicTrainer;

/// <summary>
/// Custom service provider for application.
/// <remarks>
/// It's made that way because of the limitation of Avalonia UI.
/// 
/// You can't anyhow interact with the common project from platform project, so you have to inject
/// platform-specific logic into service provider to use it in common project afterward. 
/// </remarks>
/// </summary>
public static class CustomServiceProvider
{
    /// <summary>
    /// Application service provider.
    /// </summary>
    private static ServiceProvider? _serviceProvider;
    
    /// <summary>
    /// Public property to access service provider.
    /// </summary>
    public static ServiceProvider ServiceProvider
    {
        get { return _serviceProvider!; }
    }
    
    /// <summary>
    /// Initialize service provider.
    /// </summary>
    /// <param name="platformServiceRegistration">Method to inject platform-specific logic</param>
    public static void InitializeServiceProvider(Action<IServiceCollection> platformServiceRegistration)
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        platformServiceRegistration(collection);
        _serviceProvider = collection.BuildServiceProvider();
    }
    
    /// <summary>
    /// Add services from common project.
    /// </summary>
    /// <param name="collection"></param>
    private static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainViewModel>();
    }
}