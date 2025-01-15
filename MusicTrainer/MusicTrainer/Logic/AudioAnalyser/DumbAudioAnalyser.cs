using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;

namespace MusicTrainer.Logic.AudioAnalyser;

public class DumbAudioAnalyser : IAudioAnalyser
{
    public double[] AnalyzeSignal(
        float[] buffer, int sampleRate,
        bool normalize = false,
        WindowingAlgorithm windowingAlgorithm = WindowingAlgorithm.None,
        NoiseReductionAlgorithm noiseReductionAlgorithm = NoiseReductionAlgorithm.None)
    {
        var fftLength = buffer.Length;
        
        // Fill complex buffer for FFT
        var complexBuffer = new Complex[fftLength];
        for (var i = 0; i < fftLength; i++)
        {
            complexBuffer[i] = new Complex(buffer[i], 0);
        }
     
        // Perform FFT   
        Fourier.Forward(complexBuffer, FourierOptions.NoScaling);
        
        // Calculate magnitudes
        var magnitudes = new double[complexBuffer.Length / 2];
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] = complexBuffer[i].Magnitude;
        }
        
        // Filter low frequencies
        //FilterLowFrequencies(magnitudes, sampleRate, 100);
        
        // Apply Harmonic Product Spectrum
        //var magnitudes = ApplyHPS(magnitudes, 6);

        // Normalize magnitudes
        if (normalize)
        {
            NormalizeMagnitudes(magnitudes);
        }

        return magnitudes;
    }
    
    private int NextPowerOfTwo(int value) => (int)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));

    private double[] PadWithZeros(float[] data, int length)
    {
        var result = new double[length];
        Array.Copy(data, result, data.Length);
        return result;
    }
    
    private double[] ApplyWindow(double[] data, WindowingAlgorithm windowingAlgorithm)
    {
        double[] windowedData = new double[data.Length];
        for (int n = 0; n < data.Length; n++)
        {
            switch (windowingAlgorithm)
            {
                case WindowingAlgorithm.Hanning:
                    windowedData[n] = data[n] * (0.5 - 0.5 * Math.Cos(2 * Math.PI * n / (data.Length - 1)));
                    break;
                case WindowingAlgorithm.Hamming:
                    windowedData[n] = data[n] * (0.54 - 0.46 * Math.Cos(2 * Math.PI * n / (data.Length - 1)));
                    break;
                default:
                    windowedData[n] = data[n]; // No window
                    break;
            }
        }
        return windowedData;
    }
    
    private void FilterLowFrequencies(double[] magnitudes, double sampleRate, double cutoffFrequency)
    {
        int cutoffIndex = (int)(cutoffFrequency / (sampleRate / magnitudes.Length));
        for (int i = 0; i < cutoffIndex; i++)
        {
            magnitudes[i] = 0;
        }
    }
    
    private void NormalizeMagnitudes(double[] magnitudes, double scaleFactor = 1.0)
    {
        double maxMagnitude = magnitudes.Max();
        if (maxMagnitude > 0)
        {
            for (int i = 0; i < magnitudes.Length; i++)
            {
                magnitudes[i] = (magnitudes[i] / maxMagnitude) * scaleFactor;
            }
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
        var peaks = FindPeaks(magnitudes, 0.75, 5);
        
        List<string> notes = new List<string>();
        foreach (int peakIndex in peaks)
        {
            double frequency = IndexToFrequency(peakIndex, magnitudes.Length * 2, 44100);
            string note = FrequencyToNote(frequency);
            notes.Add(note);
        }

        return notes.Distinct().ToArray();
    }
    
    private List<int> FindPeaks(double[] magnitudes, double threshold, int minPeakDistance)
    {
        List<int> peaks = new List<int>();
        for (int i = 1; i < magnitudes.Length - 1; i++)
        {
            if (magnitudes[i] > magnitudes[i - 1] && magnitudes[i] > magnitudes[i + 1] && magnitudes[i] > threshold)
            {
                bool tooCloseToExistingPeak = false;
                foreach (int peak in peaks)
                {
                    if (Math.Abs(i - peak) < minPeakDistance)
                    {
                        tooCloseToExistingPeak = true;
                        break;
                    }
                }
                if (!tooCloseToExistingPeak)
                {
                    peaks.Add(i);
                }
            }
        }
        Console.WriteLine(string.Join(", ", peaks));
        return peaks;
    }
    
    private double IndexToFrequency(int index, int fftSize, double sampleRate)
    {
        return (index * sampleRate) / fftSize;
    }

    private string FrequencyToNote(double frequency)
    {
        if (frequency <= 0) return "N/A";

        double A4 = 440.0; // Frequency of A4
        double semitoneRatio = Math.Pow(2, 1.0 / 12.0);

        double semitonesFromA4 = 12 * Math.Log(frequency / A4, 2);
        int noteNumber = (int)Math.Round(69 + semitonesFromA4); // MIDI note number

        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (noteNumber - 12) / 12;
        int noteIndex = (noteNumber - 12) % 12;

        return noteNames[noteIndex] + octave;
    }
}