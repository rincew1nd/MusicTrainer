using System;
using Avalonia.Controls;
using MusicTrainer.ViewModels;

namespace MusicTrainer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        (DataContext as MainViewModel).SetUpPlot(this.FindControl<ScottPlot.Avalonia.AvaPlot>("AudioPlot"));
    }
}