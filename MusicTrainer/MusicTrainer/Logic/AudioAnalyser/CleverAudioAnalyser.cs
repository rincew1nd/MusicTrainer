using System.Linq;
using MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;
using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

namespace MusicTrainer.Logic.AudioAnalyser;

public class CleverAudioAnalyser : IAudioAnalyser
{
    private readonly IFrequenciesCalculation _frequenciesCalculation;
    private readonly INoiseReducer _noiseReducer;

    /// <summary>
    /// .ctor
    /// </summary>
    public CleverAudioAnalyser(
        IFrequenciesCalculation frequenciesCalculation,
        INoiseReducer noiseReducer)
    {
        _frequenciesCalculation = frequenciesCalculation;
        _noiseReducer = noiseReducer;
    }

    /// <summary>
    /// Analyze signal:
    /// - Perform frequencies transformation with windowing;
    /// - Apply noise reduction to frequencies magnitudes;
    /// - Normalize frequencies magnitudes;
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <param name="sampleRate">Audio sample rate</param>
    /// <param name="normalize">Normalize magnitudes</param>
    /// <param name="windowingAlgorithm">Windowing algorithm</param>
    /// <param name="noiseReductionAlgorithm">Noise reduction algorithm</param>
    /// <returns>Frequencies Magnitudes</returns>
    public double[] AnalyzeSignal(
        float[] buffer, int sampleRate,
        bool normalize = false,
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

        if (normalize)
        {
            NormalizeMagnitudes(magnitudes);
        }
        
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

    public string[] FindNotes(double[] magnitudes)
    {
        return ["Not implemented"];
    }
}