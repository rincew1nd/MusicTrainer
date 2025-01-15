using System;
using System.Collections;
using System.Linq;
using MusicTrainer.Logic.Tools;

namespace MusicTrainer.Logic.AudioAnalyser.NoiseReducer;

/// <summary>
/// Custom noise reducing algorithms.
/// </summary>
public class CustomNoiseReducer : INoiseReducer
{
    /// <summary>
    /// The rate at which new signal magnitudes will be applied to noise profile.
    /// </summary>
    private const float NewSignalWeight = 0.1f;

    /// <summary>
    /// Array of noise profile.
    /// </summary>
    private double[] _noiseProfile = [];

    /// <summary>
    /// How much noise profile samples to pick after the start of processing.
    /// </summary>
    private const int NoiseProfileSampleSize = 10;

    /// <summary>
    /// How much noise profile samples left to process.
    /// </summary>
    private int _noiseProfileSamplesLeft = NoiseProfileSampleSize;
    
    /// <inheritdoc/>
    public void UpdateNoiseProfile(double[] magnitudes, bool reset = false)
    {
        if (_noiseProfile.Length == 0 || reset)
        {
            _noiseProfile = new double[magnitudes.Length];
        }

        if (_noiseProfileSamplesLeft <= 0)
        {
            return;
        }

        _noiseProfileSamplesLeft--;
        for (var i = 0; i < magnitudes.Length; i++)
        {
            _noiseProfile[i] = (_noiseProfile[i] * (1 - NewSignalWeight)) + (magnitudes[i] * NewSignalWeight);
        }
    }
    
    /// <inheritdoc/>
    public void ApplyNoiseReduction(ref double[] magnitudes, NoiseReductionAlgorithm algorithm = NoiseReductionAlgorithm.None)
    {
        if (magnitudes.Length == 0 || algorithm == NoiseReductionAlgorithm.None)
        {
            return;
        }
        
        switch (algorithm)
        {
            case NoiseReductionAlgorithm.AdaptiveSpectralSubtraction:
            {
                for (var i = 0; i < magnitudes.Length; i++)
                {
                    magnitudes[i] = Math.Max(0, magnitudes[i] - _noiseProfile[i]);
                }
                break;
            }
            case NoiseReductionAlgorithm.WienerFiltering:
            {
                for (var i = 0; i < magnitudes.Length; i++)
                {
                    var noisePower = _noiseProfile[i] * _noiseProfile[i];
                    var signalPower = magnitudes[i] * magnitudes[i];
                    var gain = signalPower / (signalPower + noisePower);
                    magnitudes[i] *= gain;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(algorithm),
                    algorithm,
                    "Unsupported noise reduction algorithm");
        }
    }

    #region Doesn't work properly

    /// <summary>
    /// Last 10 RMS values
    /// </summary>
    private LimitedQueue<double> RmsValues = new LimitedQueue<double>(100);
    
    /// <summary>
    /// Compute RMS amplitude of magnitudes and find out is it a silence.
    /// </summary>
    /// <param name="magnitudes">FFT magnitudes</param>
    private bool IsSilence(double[] magnitudes)
    {
        var rms = Math.Sqrt(magnitudes.Sum(t => t * t) / magnitudes.Length);
        RmsValues.Enqueue(rms);

        double rmsSum = 0;
        double rmsSqSum = 0;
        foreach (var rmsValue in RmsValues)
        {
            rmsSum += rmsValue;
            rmsSqSum += rmsValue * rmsValue;
        }
        
        var meanRMS = rmsSum / RmsValues.Count;
        var stdDevRMS = Math.Sqrt((rmsSqSum / RmsValues.Count) - (meanRMS * meanRMS));

        return rms <= meanRMS + (stdDevRMS * 1.5);
    }

    #endregion
}