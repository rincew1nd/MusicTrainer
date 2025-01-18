using System;

namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

public class HPSPitchDetection : BasePitchDetection
{
    public override string[] DetectNotes(double[] externalMagnitudes, int sampleRate, double harmonicThreshold)
    {
        // Copy magnitudes to not change the values
        var magnitudes = new double[externalMagnitudes.Length];
        Array.Copy(externalMagnitudes, magnitudes, externalMagnitudes.Length);
        
        // Convert harmonic threshold to integer value
        var maxHarmonics = (int)harmonicThreshold;
        
        // Maximum magnitude index to search for harmonics
        var maxSearchIndex = magnitudes.Length / maxHarmonics;
        
        // Search for the bin with the highest magnitude of harmonics
        var maxBin = 1;
        for (var i = 1; i < maxSearchIndex; i++)
        {
            for (var j = 1; j <= maxHarmonics; j++)
            {
                magnitudes[i] *= magnitudes[i*j];
            }
            maxBin = magnitudes[i] > magnitudes[maxBin] ? i : maxBin;
        }
        
        // Try to find a bin with maximum value before the current one
        var correctMaxBin = 1;
        maxSearchIndex = maxBin * 3 / 4;
        for (var i = 2; i < maxSearchIndex; i++)
        {
            correctMaxBin = magnitudes[i] > magnitudes[correctMaxBin] ? i : correctMaxBin;
        }
        
        // If "correctMaxBin" is a harmonic of "maxBin", it's a correct one.
        if (Math.Abs(correctMaxBin * 2 - maxBin) < 4 &&
            magnitudes[correctMaxBin] / magnitudes[maxBin] > 0.2)
        {
            maxBin = correctMaxBin;
        }
        
        // Convert bin to frequency
        var frequency = maxBin * sampleRate / (double)magnitudes.Length / 2;
        
        // Convert frequency to note
        return [MapFrequencyToNote(frequency), frequency.ToString()];
    }
}