using System;

namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

/// <summary>
/// Harmonics product spectrum pitch detection.
/// </summary>
public class HPSPitchDetection : BasePitchDetection
{
    /// <summary>
    /// Count of harmonics to analyze.
    /// </summary>
    private readonly int MaxHarmonics;
    
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="maxHarmonics">Count of harmonics to analyze</param>
    public HPSPitchDetection(int maxHarmonics)
    {
        MaxHarmonics = maxHarmonics;
    }
    
    /// <inheritdoc/>
    public override string[] DetectNotes(double[] externalMagnitudes, int sampleRate)
    {
        // Copy magnitudes to not change the values
        var magnitudes = new double[externalMagnitudes.Length];
        Array.Copy(externalMagnitudes, magnitudes, externalMagnitudes.Length);
        
        // Maximum magnitude index to search for harmonics
        var maxSearchIndex = magnitudes.Length / MaxHarmonics;
        
        // Search for the bin with the highest magnitude of harmonics
        var maxBin = 1;
        for (var i = 1; i < maxSearchIndex; i++)
        {
            for (var j = 1; j <= MaxHarmonics; j++)
            {
                magnitudes[i] *= magnitudes[i*j];
            }
            maxBin = magnitudes[i] > magnitudes[maxBin] ? i : maxBin;
        }
        
        // Try to find a bin with maximum value before the current one
        var correctMaxBin = 1;
        maxSearchIndex = (int) (maxBin * 0.75f);
        for (var i = 2; i < maxSearchIndex; i++)
        {
            correctMaxBin = magnitudes[i] > magnitudes[correctMaxBin] ? i : correctMaxBin;
        }
        
        // If "correctMaxBin" is a harmonic of "maxBin", it's a correct one.
        if (Math.Abs(correctMaxBin * 2 - maxBin) < 4 &&
            magnitudes[correctMaxBin] / magnitudes[maxBin] > 0.1)
        {
            maxBin = correctMaxBin;
        }
        
        // Convert bin to frequency
        var frequency = maxBin * sampleRate / (double)magnitudes.Length / 2;

#if DEBUG
        // Convert bin of currentMaxBin to frequency
        var correctFrequency = correctMaxBin * sampleRate / (double)magnitudes.Length / 2;
        
        return [
            $"OMax: {MapFrequencyToNote(frequency)} [{maxBin.ToString()}]",
            $"CMax: {MapFrequencyToNote(correctFrequency)} [{correctMaxBin.ToString()}]",
            $"Diff: {Math.Abs(correctMaxBin * 2 - maxBin):00}",
            $"Magn: {magnitudes[correctMaxBin] / magnitudes[maxBin]:N3}"
        ];
#endif
        
        // Convert frequency to note
        return [MapFrequencyToNote(frequency)];
    }
}