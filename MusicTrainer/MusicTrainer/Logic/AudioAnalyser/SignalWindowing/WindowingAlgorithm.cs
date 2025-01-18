namespace MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

/// <summary>
/// Types of windowing algorithms.
/// </summary>
public enum WindowingAlgorithm
{
    None = 0,
    Hamming = 1,
    Hanning = 2,
    BlackmannHarris = 3,
}