namespace MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;

/// <summary>
/// Interface for frequencies calculation.
/// </summary>
public interface IFrequenciesCalculation
{
    /// <summary>
    /// Transform audio signal into frequencies with magnitudes.
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <param name="sampleRate">Sample rate</param>
    /// <returns>Frequencies magnitudes</returns>
    double[] Transform(float[] buffer, int sampleRate);
}