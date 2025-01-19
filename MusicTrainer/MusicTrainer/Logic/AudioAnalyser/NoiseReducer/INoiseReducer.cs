using System;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.NoiseReducer;

/// <summary>
/// Interface for noise reducing algorithms.
/// </summary>
public interface INoiseReducer
{
    /// <summary>
    /// Update noise profile with new magnitudes.
    /// </summary>
    /// <param name="magnitudes">Magnitudes</param>
    /// <param name="reset">Reset noise profile</param>
    void UpdateNoiseProfile(double[] magnitudes, bool reset = false);

    /// <summary>
    /// Apply noise reduction algorithm to magnitudes.
    /// <see cref="magnitudes"/> will be overwritten after execution!
    /// </summary>
    /// <param name="magnitudes">Magnitudes</param>
    /// <param name="algorithm">Noise reduction algorithm</param>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported noise reduction algorithm</exception>
    void ApplyNoiseReduction(ref double[] magnitudes, NoiseReductionAlgorithm algorithm = NoiseReductionAlgorithm.None);
    
    /// <summary>
    /// Apply noise reduction algorithm to magnitudes.
    /// <see cref="complexes"/> will be overwritten after execution!
    /// </summary>
    /// <param name="complexes"><see cref="Complex"/></param>
    /// <param name="algorithm">Noise reduction algorithm</param>
    /// <exception cref="ArgumentOutOfRangeException">Unsupported noise reduction algorithm</exception>
    void ApplyNoiseReduction(ref Complex[] complexes, NoiseReductionAlgorithm algorithm = NoiseReductionAlgorithm.None);
}