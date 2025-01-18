namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

/// <summary>
/// Interface for algorithm to find played notes based on frequencies magnitudes.
/// </summary>
public interface INoteDetector
{
    /// <summary>
    /// Find played note/s based on frequencies magnitudes.
    /// </summary>
    /// <param name="magnitudes">Frequencies magnitudes</param>
    /// <param name="sampleRate">Signal sample rate</param>
    /// <returns>List of played note/s</returns>
    string[] DetectNotes(double[] magnitudes, int sampleRate);
}