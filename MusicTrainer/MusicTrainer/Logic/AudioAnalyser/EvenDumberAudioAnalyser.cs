/**using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Dsp;

namespace MusicTrainer.Logic;

public class EvenDumberAudioAnalyser : IAudioAnalyser
{
    public double[] PerformFFT(float[] timeDomainData, int fftLength, int sampleRate, WindowType windowType)
    {
        // Apply the chosen window function
        Complex[] complexBuffer = new Complex[fftLength];
        for (int i = 0; i < timeDomainData.Length; i++)
        {
            complexBuffer[i] = new Complex { X = timeDomainData[i], Y = 0 }; // Assign real and imaginary parts
        }

        switch (windowType)
        {
            case WindowType.Hamming:
                ApplyWindow(complexBuffer, FastFourierTransform.HammingWindow);
                break;
            case WindowType.Hanning:
                ApplyWindow(complexBuffer, FastFourierTransform.HannWindow);
                break;
            case WindowType.BlackmannHarris:
                ApplyWindow(complexBuffer, FastFourierTransform.BlackmannHarrisWindow);
                break;
            default:
                break;
        }
        
        FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), complexBuffer);

        double[] magnitudes = new double[fftLength / 2 + 1];
        for (int i = 0; i <= fftLength / 2; i++)
        {
            magnitudes[i] =
                Math.Sqrt(complexBuffer[i].X * complexBuffer[i].X + complexBuffer[i].Y * complexBuffer[i].Y);
        }
        
        // Filter low frequencies
        FilterLowFrequencies(magnitudes, sampleRate, 100);
        
        // Apply Harmonic Product Spectrum
        var hps = ApplyHPS(magnitudes, 6);

        return NormalizeMagnitudes(hps);
    }
    
    private void ApplyWindow(Complex[] data, Func<int, int, double> windowFunction)
    {
        int n = data.Length;
        for (int i = 0; i < n; i++)
        {
            double windowValue = windowFunction(i, n);
            data[i] = new Complex
            { 
                X = data[i].X * (float)windowValue,
                Y = data[i].Y
            };
        }
    }
    
    private double[] NormalizeMagnitudes(double[] magnitudes)
    {
        // Find the maximum magnitude for scaling purposes
        double maxMagnitude = magnitudes.Max();

        if (maxMagnitude == 0) return magnitudes; // Avoid division by zero

        // Scale all magnitudes to fit between 0 and 1
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] /= maxMagnitude;
        }

        return magnitudes;
    }
    
    private void FilterLowFrequencies(double[] magnitudes, double sampleRate, double cutoffFrequency)
    {
        int cutoffIndex = (int)(cutoffFrequency / (sampleRate / magnitudes.Length));
        for (int i = 0; i < cutoffIndex; i++)
        {
            magnitudes[i] = 0;
        }
    }

    private double[] ApplyHPS(double[] magnitudes, int maxHarmonics = 4)
    {
        int hpsSize = magnitudes.Length / maxHarmonics;
        double[] hps = new double[hpsSize];

        for (int i = 0; i < hpsSize; i++)
        {
            hps[i] = magnitudes[i];
            for (int harmonic = 2; harmonic <= maxHarmonics; harmonic++)
            {
                int harmonicIndex = i * harmonic;
                if (harmonicIndex < magnitudes.Length)
                {
                    hps[i] *= magnitudes[harmonicIndex];
                }
            }
        }
        return hps;
    }

    public string[] FindNotes(double[] magnitudes)
    {
        int sampleRate = 44100; // Assuming a default sample rate
        double minFreq = sampleRate / (2.0 * magnitudes.Length);
        double maxFreq = sampleRate / 2;

        var peaks = FindPeaks(magnitudes, minPeakDistance: 10); // Example min distance

        int[] peakIndices = peaks.Select(p => (int)(p * magnitudes.Length)).ToArray();
        double[] peakFrequencies = peakIndices.Select(i => i * (maxFreq - minFreq) / magnitudes.Length + minFreq).ToArray();

        return peakFrequencies
            .Select(GetNoteNameFromFrequency)
            .ToArray();
    }

    private static double[] FindPeaks(double[] data, int minPeakDistance)
    {
        // Implementation of peak finding
        var peaks = new List<double>();
        for (int i = 1; i < data.Length - 1; i++)
        {
            if (data[i] > data[i - 1] && data[i] > data[i + 1])
            {
                bool isPeak = true;
                // Check distance constraint
                int startIdx = Math.Max(0, i - minPeakDistance);
                for (int j = startIdx; j <= i + minPeakDistance && j < data.Length; j++)
                {
                    if (j != i && data[i] < data[j])
                    {
                        isPeak = false;
                        break;
                    }
                }

                if (isPeak)
                {
                    peaks.Add(i);
                }
            }
        }
        return peaks.ToArray();
    }

    private static string GetNoteNameFromFrequency(double frequency)
    {
        const double C0 = 16.35; // Frequency of C0
        int noteNum = (int)Math.Round(12 * Math.Log(frequency / C0, 2));
        int octave = noteNum / 12;
        int noteIndex = noteNum % 12;

        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        return noteNames[noteIndex] + octave;
    }
}
*/