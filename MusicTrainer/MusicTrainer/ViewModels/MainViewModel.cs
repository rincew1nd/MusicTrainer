using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicTrainer.Logic.AudioAnalyser;
using MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;
using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;
using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;
using ScottPlot;
using ScottPlot.Avalonia;

namespace MusicTrainer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _playedNotes = string.Empty;

    private readonly AvaPlot _audioPlot;
    private readonly IAudioManager _audioManager;
    private readonly IAudioAnalyser _audioAnalyser;

    private const float Hertz = 22000f;
    private readonly int FftLength = (int)Math.Pow(2, 13);

    public MainViewModel(AvaPlot? plot)
    {
        _audioPlot = plot ?? throw new ArgumentException("Plot object was not found");
        _audioPlot.Plot.YLabel("Magnitude");
        _audioPlot.Plot.XLabel("Frequency (Hz)");
        _audioPlot.Plot.Axes.SetLimits(new AxisLimits(0, 22000, -0.1, 1.1));
        
        _audioManager = new SpeakerAudioManager(new WaveFormat(44100, 16, 1), FftLength);
        //_audioManager = new MicAudioManager(new WaveFormat(44100, 16, 1), FftLength);
        
        //_audioAnalyser = new DumbAudioAnalyser();
        _audioAnalyser = new CleverAudioAnalyser(new FFTFrequenciesCalculation(), new CustomNoiseReducer());
    }

    [RelayCommand]
    private void StartCapturingAudio()
    {
        _audioManager.StartCapturingAudio(
            async (audioData, sampleRate) =>
            {
                var _fftData = _audioAnalyser.AnalyzeSignal(
                    audioData, sampleRate,
                    true,
                    WindowingAlgorithm.Hanning,
                    NoiseReductionAlgorithm.WienerFiltering);
                
                var notes = _audioAnalyser.FindNotes(_fftData);
                PlayedNotes = $"{string.Join(", ", notes)}";

                // Update the plot on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _audioPlot.Plot.Clear();
                    _audioPlot.Plot.Add.Signal(_fftData, Hertz / _fftData.Length);
                    _audioPlot.Refresh();
                });
            });
    }

    [RelayCommand]
    private void StopCapturingAudio()
    {
        _audioManager.StopCapturingAudio();
    }
}