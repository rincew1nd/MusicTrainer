using System.Linq;
using MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;
using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.NoteDetection;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

namespace MusicTrainer.Logic.AudioAnalyser;

public class BasicAudioAnalyser : IAudioAnalyser
{
    private readonly IFrequenciesCalculation _frequenciesCalculation;
    private readonly INoiseReducer _noiseReducer;
    private readonly INoteDetector _noteDetector;

    /// <summary>
    /// .ctor
    /// </summary>
    public BasicAudioAnalyser(
        IFrequenciesCalculation frequenciesCalculation,
        INoiseReducer noiseReducer,
        INoteDetector noteDetector)
    {
        _frequenciesCalculation = frequenciesCalculation;
        _noiseReducer = noiseReducer;
        _noteDetector = noteDetector;
    }

    /// <summary>
    /// Analyze signal:
    /// - Perform frequencies transformation with windowing;
    /// - Apply noise reduction to frequencies magnitudes;
    /// - Normalize frequencies magnitudes;
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <param name="sampleRate">Audio sample rate</param>
    /// <param name="windowingAlgorithm">Windowing algorithm</param>
    /// <param name="noiseReductionAlgorithm">Noise reduction algorithm</param>
    /// <returns>Frequencies Magnitudes</returns>
    public double[] AnalyzeSignal(
        float[] buffer, int sampleRate,
        WindowingAlgorithm windowingAlgorithm = WindowingAlgorithm.None,
        NoiseReductionAlgorithm noiseReductionAlgorithm = NoiseReductionAlgorithm.None)
    {
        ApplySignalWindowing.ApplyWindowing(ref buffer, windowingAlgorithm);
        
        var magnitudes = _frequenciesCalculation.Transform(buffer, sampleRate);

        if (noiseReductionAlgorithm != NoiseReductionAlgorithm.None)
        {
            _noiseReducer.UpdateNoiseProfile(magnitudes);
            _noiseReducer.ApplyNoiseReduction(ref magnitudes, noiseReductionAlgorithm);
        }

        NormalizeMagnitudes(magnitudes);
        
        return magnitudes;
    }
        
    /// <summary>
    /// Normalize FFT magnitudes so the values be in range from 0 to 1.
    /// </summary>
    private void NormalizeMagnitudes(double[] magnitudes)
    {
        var maxMagnitude = magnitudes.Max();
        if (maxMagnitude == 0) return;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] /= maxMagnitude;
        }
    }

    /// <summary>
    /// Detect played notes.
    /// </summary>
    /// <param name="magnitudes">Frequencies Magnitudes</param>
    /// <param name="sampleRate">Audio sample rate</param>
    /// <param name="threshold">Threshold (depends on algorithm)</param>
    /// <returns>Played notes</returns>
    public string[] FindNotes(double[] magnitudes, int sampleRate, double threshold)
    {
        return _noteDetector.DetectNotes(magnitudes, sampleRate, threshold);
    }
}