using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

public class PolyphonicNoteDetector : INoteDetector
{
    private static readonly double[] NoteFrequencies = GenerateNoteFrequencies();
    private static readonly string[] NoteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    private static readonly double FrequencyThreshold = 5.0; // Hz tolerance for note detection

    public string[] DetectNotes(
        double[] magnitudes,
        int sampleRate,
        double magnitudeThreshold = 0.25
    )
    {
        var detectedNotes = new List<string>();

        var peakIndices = FindPeaks(magnitudes, magnitudeThreshold);

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
        Console.WriteLine(string.Join(", ", freq));

        return detectedNotes.ToArray();
    }

    private static List<int> FindPeaks(double[] magnitudes, double threshold)
    {
        var peaks = new List<int>();
        for (var i = 1; i < magnitudes.Length - 1; i++)
        {
            if (magnitudes[i] > magnitudes[i - 1] &&
                magnitudes[i] > magnitudes[i + 1] &&
                magnitudes[i] > threshold)
            {
                peaks.Add(i);
            }
        }
        return peaks;
    }

    private static string MapFrequencyToNote(double frequency)
    {
        var closestNote = NoteFrequencies
            .Select((noteFreq, index) => new { Index = index, Difference = Math.Abs(noteFreq - frequency) })
            .MinBy(x => x.Difference);

        if (closestNote == null || !(closestNote.Difference < FrequencyThreshold))
        {
            return string.Empty;
        }
        
        var midiNote = closestNote.Index;
        return MidiNoteToName(midiNote);
    }

    private static string MidiNoteToName(int midiNote)
    {
        var octave = (midiNote / 12) - 1;
        var noteName = NoteNames[midiNote % 12];
        return $"{noteName}{octave}";
    }

    private static double[] GenerateNoteFrequencies()
    {
        var frequencies = new double[128];
        for (int i = 0; i < 128; i++)
        {
            frequencies[i] = 440.0 * Math.Pow(2, (i - 69) / 12.0);
        }
        return frequencies;
    }
}