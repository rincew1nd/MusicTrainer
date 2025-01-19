using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicTrainer.Logic.AudioAnalyser;
using MusicTrainer.Logic.AudioAnalyser.Experiments;
using ScottPlot;
using ScottPlot.Avalonia;

namespace MusicTrainer.ViewModels;

/// <summary>
/// TODO: Temporary view. Capture and process audio data. 
/// </summary>
public partial class MainViewModelExperimental : ViewModelBase
{
    /// <summary>
    /// FFT audio analyzer.
    /// </summary>
    private readonly FFTAudioAnalyzer _fftAudioAnalyzer;

    /// <summary>
    /// List of detected notes.
    /// </summary>
    [ObservableProperty] private string _playedNotes = string.Empty;
    
    /// <summary>
    /// Plotting UI control.
    /// </summary>
    private AvaPlot? _audioPlot;
    
    /// <summary>
    /// Maximum captured range of hertz.
    /// </summary>
    private const float Hertz = 22000f;

    public MainViewModelExperimental(FFTAudioAnalyzer fftAudioAnalyzer)
    {
        _fftAudioAnalyzer = fftAudioAnalyzer;
    }
    
    /// <summary>
    /// BUTTON: Start capture
    /// </summary>
    [RelayCommand]
    private void StartCapturingAudio()
    {
        _fftAudioAnalyzer.StartCapturingAudio();

        _fftAudioAnalyzer.NewDataAvailable += (magnitudes, notes) =>
        {
            PlayedNotes = notes;
            // Update the plot on the UI thread
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _audioPlot!.Plot.Clear();
                _audioPlot.Plot.Add.Signal(magnitudes, Hertz / magnitudes.Length);
                _audioPlot.Refresh();
            }).Wait();
        };
    }
    
    /// <summary>
    /// BUTTON: Stop capture
    /// </summary>
    [RelayCommand]
    private void StopCapturingAudio()
    {
        _fftAudioAnalyzer.StopCapturingAudio();
    }

    /// <summary>
    /// Method to supply view with plotting UI control.
    /// </summary>
    /// <param name="plot"><see cref="AvaPlot"/></param>
    /// <exception cref="ArgumentException">Plot object was not found</exception>
    public void SetUpPlot(AvaPlot? plot)
    {
        _audioPlot = plot ?? throw new ArgumentException("Plot object was not found");
        _audioPlot.Plot.YLabel("Magnitude");
        _audioPlot.Plot.XLabel("Frequency (Hz)");
        _audioPlot.Plot.Axes.SetLimits(new AxisLimits(0, 22000, -0.1, 1.1));
    }
}