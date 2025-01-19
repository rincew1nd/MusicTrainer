using System;
using NAudio.Dsp;

namespace MusicTrainer.Logic.Tools;

/// <summary>
/// Helper methods to ease the life.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Get the magnitude of a <see cref="Complex"/>.
    /// </summary>
    /// <param name="complex"><see cref="Complex"/></param>
    /// <returns>Frequency magnitude</returns>
    public static double Magnitude(this Complex complex)
    {
        return Math.Sqrt(complex.X * complex.X + complex.Y * complex.Y);
    }
}