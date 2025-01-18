using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

namespace MusicTrainer.Logic.AudioAnalyser;

/// <summary>
/// Interface for Audio Analyzers
/// </summary>
public interface IAudioAnalyser
{
    /// <summary>
    /// Analyze signal:
    /// - Perform frequencies transformation with windowing;
    /// - Apply noise reduction to frequencies magnitudes;
    /// - Normalize frequencies magnitudes;
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <param name="sampleRate">Audio sample rate</param>
    /// <param name="windowingAlgorithm">Windowing algorithm</param>
    /// <param name="noiseReductionAlgorithm">Noise reduction algorithm</param>
    /// <returns>Frequencies Magnitudes</returns>
    double[] AnalyzeSignal(
        float[] buffer, int sampleRate,
        WindowingAlgorithm windowingAlgorithm = WindowingAlgorithm.None,
        NoiseReductionAlgorithm noiseReductionAlgorithm = NoiseReductionAlgorithm.None);

    /// <summary>
    /// Analyze FFT magnitudes and try to find played notes.
    /// </summary>
    /// <param name="magnitudes">FFT magnitudes</param>
    /// <param name="sampleRate">Audio sample rate</param>
    /// <returns>Played notes</returns>
    string[] FindNotes(double[] magnitudes, int sampleRate);
}