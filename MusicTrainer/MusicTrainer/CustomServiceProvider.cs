using System;
using Microsoft.Extensions.DependencyInjection;
using MusicTrainer.ViewModels;

namespace MusicTrainer;

public static class CustomServiceProvider
{
    private static ServiceProvider ServiceProvider = null;
    
    public static void InitializeServiceProvider(Action<IServiceCollection> platformServiceRegistration)
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        platformServiceRegistration(collection);
        ServiceProvider = collection.BuildServiceProvider();
    }
    
    private static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainViewModel>();
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}