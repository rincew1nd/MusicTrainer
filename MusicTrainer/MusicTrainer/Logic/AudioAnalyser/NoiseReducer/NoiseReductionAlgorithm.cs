namespace MusicTrainer.Logic.AudioAnalyser.NoiseReducer;

/// <summary>
/// Possible types of noise reduction algorithms.
/// </summary>
public enum NoiseReductionAlgorithm
{
    None = 0,
    AdaptiveSpectralSubtraction = 1,
    WienerFiltering = 2
}