using System;
using NAudio.Dsp;

namespace MusicTrainer.Logic.AudioAnalyser.FrequenciesCalculation;

/// <summary>
/// FFT Transformer with NAudio library methods.
/// </summary>
public class FFTFrequenciesCalculation : IFrequenciesCalculation
{
    /// <inheritdoc/>
    public double[] Transform(float[] buffer, int sampleRate)
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
        
        // Extract magnitude from FFT results
        var magnitudes = new double[fftLength / 2]; //+1];
        for (var i = 0; i < fftLength / 2; i++)
        {
            magnitudes[i] =
                Math.Sqrt(complexBuffer[i].X * complexBuffer[i].X + complexBuffer[i].Y * complexBuffer[i].Y);
        }
        
        return magnitudes;
    }
}