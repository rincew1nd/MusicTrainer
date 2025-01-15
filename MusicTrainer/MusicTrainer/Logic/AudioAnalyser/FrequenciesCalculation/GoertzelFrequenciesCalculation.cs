using MusicTrainer.Logic.Tools;

namespace MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;

/// <summary>
/// Goertzel Transformer with NAudio library methods.
/// </summary>
public class GoertzelFrequenciesCalculation : IFrequenciesCalculation
{
    private static readonly double[] _noteFrequencies =
    [
        27.50, 30.87,
        32.70, 36.71, 41.20, 43.65, 49.00, 55.00, 61.74,
        65.41, 73.42, 82.41, 87.31, 98.00, 110.00, 123.47,
        130.81, 146.83, 164.81, 174.61, 196.00, 220.00, 246.94,
        261.63, 293.66, 329.63, 349.23, 392.00, 440.00, 493.88,
        523.25, 587.33, 659.25, 698.46, 783.99, 880.00, 987.77,
        1046.50, 1174.66, 1318.51, 1396.91, 1567.98, 1760.00, 1975.53,
        2093.00, 2349.32, 2637.02, 2793.83, 3135.96, 3520.00, 3951.07,
        4186.01, 4699, 5274, 5588, 6272, 7040, 7902
    ];
    
    /// <inheritdoc/>
    public double[] Transform(float[] buffer, int sampleRate)
    {
        var magnitudes = new double[_noteFrequencies.Length];
        for (var i = 0; i < _noteFrequencies.Length; i++)
        {
            var filter = new GoertzelFilter(_noteFrequencies[i], sampleRate);
            magnitudes[i] = filter.Process(buffer);
        }
        return magnitudes;
    }
}