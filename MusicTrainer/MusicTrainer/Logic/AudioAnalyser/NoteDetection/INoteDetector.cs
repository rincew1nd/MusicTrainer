namespace MusicTrainer.Logic.AudioAnalyser.NoteDetection;

public interface INoteDetector
{
    string[] DetectNotes(
        double[] magnitudes,
        int sampleRate,
        double threshold
    );
}