using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicTrainer.Logic.AudioAnalyser;
using MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;
using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.NoteDetection;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;
using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;
using ScottPlot;
using ScottPlot.Avalonia;

namespace MusicTrainer.ViewModels;

/// <summary>
/// TODO: Temporary view. Capture and process audio data. 
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// List of detected notes.
    /// </summary>
    [ObservableProperty] private string _playedNotes = string.Empty;
    
    /// <summary>
    /// Plotting UI control.
    /// </summary>
    private AvaPlot? _audioPlot;

    /// <summary>
    /// <see cref="IAudioManager"/> to capture audio data.
    /// </summary>
    private readonly IAudioManager _audioManager;
    
    /// <summary>
    /// <see cref="IAudioAnalyser"/> to analyze captured data.
    /// </summary>
    private readonly IAudioAnalyser _audioAnalyser;

    /// <summary>
    /// Maximum captured range of hertz.
    /// </summary>
    private const float Hertz = 22000f;
    
    /// <summary>
    /// Length of FFT transformation.
    /// </summary>
    private readonly int FftLength = (int)Math.Pow(2, 15);
    
    /// <summary>
    /// Setup view.
    /// </summary>
    /// <param name="audioManager">Platform-specific <see cref="IAudioManager"/></param>
    public MainViewModel(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _audioManager.SetUp(new WaveFormat(44100, 16, 1), FftLength);
        
        _audioAnalyser = new BasicAudioAnalyser(
            new FFTFrequenciesCalculation(),
            new CustomNoiseReducer(),
            new HPSPitchDetection(5)
        );
    }

    /// <summary>
    /// BUTTON: Start capture
    /// </summary>
    [RelayCommand]
    private void StartCapturingAudio()
    {
        var sb = new Stopwatch();
        
        _audioManager.StartCapturingAudio(
            async (audioData, sampleRate) =>
            {
                var _fftData = _audioAnalyser.AnalyzeSignal(
                    audioData, sampleRate,
                    WindowingAlgorithm.Hamming,
                    NoiseReductionAlgorithm.WienerFiltering);

                var notes = _audioAnalyser.FindNotes(_fftData, sampleRate);
                PlayedNotes = $"{string.Join(", ", notes)}";

                // Update the plot on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _audioPlot!.Plot.Clear();
                    _audioPlot.Plot.Add.Signal(_fftData, Hertz / _fftData.Length);
                    _audioPlot.Refresh();
                });
            });
    }

    /// <summary>
    /// BUTTON: Stop capture
    /// </summary>
    [RelayCommand]
    private void StopCapturingAudio()
    {
        _audioManager.StopCapturingAudio();
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