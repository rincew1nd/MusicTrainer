using System;
using MathNet.Numerics.LinearAlgebra;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.NoiseReducer;

/// <summary>
/// Noise reduction algorithms from external libraries.
/// </summary>
public class ExternalNoiseReducer : INoiseReducer
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
            case NoiseReductionAlgorithm.WienerFiltering:
                ApplyWienerFilteringMathNet(ref magnitudes);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(algorithm),
                    algorithm,
                    "Unsupported noise reduction algorithm");
        }
    }

    public void ApplyNoiseReduction(ref Complex[] complexes, NoiseReductionAlgorithm algorithm = NoiseReductionAlgorithm.None)
    {
        throw new NotImplementedException();
    }

    private void ApplyWienerFilteringMathNet(ref double[] magnitudes)
    {
        // Convert arrays to Math.NET vectors
        var signalVector = Vector<double>.Build.DenseOfArray(magnitudes);
        var noiseVector = Vector<double>.Build.DenseOfArray(_noiseProfile);

        // Calculate power spectra
        var signalPower = signalVector.PointwiseMultiply(signalVector);
        var noisePower = noiseVector.PointwiseMultiply(noiseVector);

        // Calculate Wiener filter gain
        var gain = signalPower.PointwiseDivide(signalPower + noisePower);

        // Apply Wiener filter
        var filteredMagnitudes = signalVector.PointwiseMultiply(gain);

        // Update the magnitudes array with the filtered values
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] = filteredMagnitudes[i];
        }
    }
}