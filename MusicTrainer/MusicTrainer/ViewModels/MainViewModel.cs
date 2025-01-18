using System;
using System.Diagnostics;
using System.Text;
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

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _playedNotes = string.Empty;

    private readonly IAudioManager _audioManager;
    private readonly IAudioAnalyser _audioAnalyser;
    
    private AvaPlot? _audioPlot;

    private const float Hertz = 22000f;
    private readonly int FftLength = (int)Math.Pow(2, 14);
    
    public MainViewModel(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _audioManager.SetUp(new WaveFormat(44100, 16, 1), FftLength);
        
        _audioAnalyser = new BasicAudioAnalyser(
            new FFTFrequenciesCalculation(),
            new CustomNoiseReducer(),
            new HPSPitchDetection()
        );
    }

    [RelayCommand]
    private void StartCapturingAudio()
    {
        var sw = new Stopwatch();
        sw.Start();
        var sb = new StringBuilder();
        
        _audioManager.StartCapturingAudio(
            async (audioData, sampleRate) =>
            {
                sb.Append($"Audio: {sw.ElapsedTicks}");
                sw.Restart();
                
                var _fftData = _audioAnalyser.AnalyzeSignal(
                    audioData, sampleRate,
                    WindowingAlgorithm.Hamming,
                    NoiseReductionAlgorithm.WienerFiltering);
                
                sb.Append($"   FFT: {sw.ElapsedTicks}");
                sw.Restart();
                
                var notes = _audioAnalyser.FindNotes(_fftData, sampleRate, 3);
                PlayedNotes = $"{string.Join(", ", notes)}";

                // Update the plot on the UI thread
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _audioPlot!.Plot.Clear();
                    _audioPlot.Plot.Add.Signal(_fftData, Hertz / _fftData.Length);
                    _audioPlot.Refresh();
                });
                sb.Append($"   UI: {sw.ElapsedTicks}");
                sw.Restart();

                //PlayedNotes = sb.ToString();;
                sb.Clear();
            });
    }

    [RelayCommand]
    private void StopCapturingAudio()
    {
        _audioManager.StopCapturingAudio();
    }

    public void SetUpPlot(AvaPlot? plot)
    {
        _audioPlot = plot ?? throw new ArgumentException("Plot object was not found");
        _audioPlot.Plot.YLabel("Magnitude");
        _audioPlot.Plot.XLabel("Frequency (Hz)");
        _audioPlot.Plot.Axes.SetLimits(new AxisLimits(0, 22000, -0.1, 1.1));
    }
}