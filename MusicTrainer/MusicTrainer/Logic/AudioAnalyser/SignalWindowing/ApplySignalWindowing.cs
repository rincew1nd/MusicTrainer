using System;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

public static class ApplySignalWindowing
{
    public static void ApplyWindowing(ref float[] buffer, WindowingAlgorithm windowingAlgorithm)
    {
        // Apply windowing
        switch (windowingAlgorithm)
        {
            case WindowingAlgorithm.Hamming:
                ApplyConcreteWindowing(ref buffer, FastFourierTransform.HammingWindow);
                break;
            case WindowingAlgorithm.Hanning:
                ApplyConcreteWindowing(ref buffer, FastFourierTransform.HannWindow);
                break;
            case WindowingAlgorithm.BlackmannHarris:
                ApplyConcreteWindowing(ref buffer, FastFourierTransform.BlackmannHarrisWindow);
                break;
            case WindowingAlgorithm.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(windowingAlgorithm),
                    windowingAlgorithm,
                    "Unsupported windowing algorithm");
        }
    }
    
    /// <summary>
    /// Method for applying chosen windowing algorithm. 
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <param name="windowFunction">Windowing function</param>
    private static void ApplyConcreteWindowing(ref float[] buffer, Func<int, int, double> windowFunction)
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            var windowValue = windowFunction(i, buffer.Length);
            buffer[i] *= (float)windowValue;
        }
    }
}