using System;
using System.Collections.Generic;

namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

/// <summary>
/// Polyphonic notes detection.
/// </summary>
public class PolyphonicNoteDetector : BasePitchDetection
{
    /// <summary>
    /// Minimum magnitude value to filter frequencies.
    /// </summary>
    private double MagnitudeThreshold { get; set; }
    
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="magnitudeThreshold">Minimum magnitude value to filter frequencies</param>
    public PolyphonicNoteDetector(double magnitudeThreshold = 0.3d)
    {
        MagnitudeThreshold = magnitudeThreshold;
    }

    /// <inheritdoc/>
    public override string[] DetectNotes(double[] magnitudes, int sampleRate)
    {
        var detectedNotes = new List<string>();

        var peakIndices = FindPeaks(magnitudes);

        var freq = new List<double>();
        foreach (var index in peakIndices)
        {
            var frequency = index * sampleRate / (double)magnitudes.Length / 2;
            freq.Add(frequency);
            var note = MapFrequencyToNote(frequency);
            if (!string.IsNullOrEmpty(note) && !detectedNotes.Contains(note))
            {
                detectedNotes.Add(note);
            }
        }
        
#if DEBUG
        Console.WriteLine(string.Join(", ", freq));
#endif

        return detectedNotes.ToArray();
    }

    /// <summary>
    /// Find peaks in frequencies.
    /// </summary>
    /// <param name="magnitudes">Frequency magnitudes</param>
    /// <returns>Bins with frequencies bigger than threshold</returns>
    private List<int> FindPeaks(double[] magnitudes)
    {
        var peaks = new List<int>();
        for (var i = 1; i < magnitudes.Length - 1; i++)
        {
            if (magnitudes[i] > magnitudes[i - 1] &&
                magnitudes[i] > magnitudes[i + 1] &&
                magnitudes[i] > MagnitudeThreshold)
            {
                peaks.Add(i);
            }
        }
        return peaks;
    }
}