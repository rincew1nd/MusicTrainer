using Avalonia.Controls;
using MusicTrainer.ViewModels;

namespace MusicTrainer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this.FindControl<ScottPlot.Avalonia.AvaPlot>("AudioPlot"));
    }
}