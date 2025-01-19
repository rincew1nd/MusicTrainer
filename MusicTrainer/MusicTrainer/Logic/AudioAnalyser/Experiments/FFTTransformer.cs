using System;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.Experiments;

/// <summary>
/// FFT Transformer with NAudio library methods.
/// </summary>
public class FFTTransformer
{
    /// <summary>
    /// Perform FFT transformation
    /// </summary>
    /// <param name="buffer">Raw audio signal</param>
    /// <returns>Frequencies magnitudes</returns>
    public Complex[] Transform(float[] buffer)
    {
        var fftLength = buffer.Length;
        
        // Fill complex buffer for FFT
        var complexBuffer = new Complex[fftLength];
        for (var i = 0; i < fftLength; i++)
        {
            complexBuffer[i].X = buffer[i];
            complexBuffer[i].Y = 0;
        }

        // Perform FFT
        FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), complexBuffer);
        
        return complexBuffer;
    }
}