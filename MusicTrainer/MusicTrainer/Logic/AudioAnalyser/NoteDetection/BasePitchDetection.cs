using System;
using System.Linq;

namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

public abstract class BasePitchDetection : INoteDetector
{
    /// <summary>
    /// Hz tolerance for note detection.
    /// </summary>
    private const double FrequencyThreshold = 5.0;
    
    /// <summary>
    /// Note frequencies.
    /// </summary>
    private static readonly double[] NoteFrequencies = GenerateNoteFrequencies();
    
    /// <summary>
    /// Note names.
    /// </summary>
    private static readonly string[] NoteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    
    public abstract string[] DetectNotes(double[] magnitudes, int sampleRate, double threshold);

    protected string MapFrequencyToNote(double frequency)
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

    private string MidiNoteToName(int midiNote)
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