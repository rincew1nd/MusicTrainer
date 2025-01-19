using System;
using System.Linq;
using MusicTrainer.Logic.Tools;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.NoiseReducer;

/// <summary>
/// Custom written noise reducing algorithms.
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
    public void UpdateNoiseProfile(Complex[] complexes, bool reset = false)
    {
        if (_noiseProfile.Length == 0 || reset)
        {
            _noiseProfile = new double[complexes.Length];
        }

        if (_noiseProfileSamplesLeft <= 0)
        {
            return;
        }

        _noiseProfileSamplesLeft--;
        for (var i = 0; i < complexes.Length; i++)
        {
            _noiseProfile[i] = (_noiseProfile[i] * (1 - NewSignalWeight)) + (complexes[i].Magnitude() * NewSignalWeight);
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
    
    /// <inheritdoc/>
    public void ApplyNoiseReduction(ref Complex[] complexes, NoiseReductionAlgorithm algorithm = NoiseReductionAlgorithm.None)
    {
        if (complexes.Length == 0 || algorithm == NoiseReductionAlgorithm.None)
        {
            return;
        }
        
        switch (algorithm)
        {
            case NoiseReductionAlgorithm.AdaptiveSpectralSubtraction:
            {
                for (var i = 0; i < complexes.Length; i++)
                {
                    var magnitude = complexes[i].Magnitude();

                    var gain = magnitude / Math.Max(0, magnitude - _noiseProfile[i]);
                    
                    complexes[i] = new Complex
                    {
                        X = complexes[i].X * (float)gain,
                        Y = complexes[i].Y * (float)gain
                    };
                }
                break;
            }
            case NoiseReductionAlgorithm.WienerFiltering:
            {
                for (var i = 0; i < complexes.Length; i++)
                {
                    var magnitude = complexes[i].Magnitude();
                    
                    var noisePower = _noiseProfile[i] * _noiseProfile[i];
                    var signalPower = magnitude * magnitude;
                    var gain = signalPower / (signalPower + noisePower);

                    complexes[i] = new Complex
                    {
                        X = complexes[i].X * (float)gain,
                        Y = complexes[i].Y * (float)gain
                    };
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
}