using System;
using System.Linq;
using MusicTrainer.Logic.AudioAnalyser.NoiseReducer;
using MusicTrainer.Logic.AudioAnalyser.NoteDetection;
using MusicTrainer.Logic.AudioAnalyser.SignalWindowing;
using MusicTrainer.Logic.AudioManager;
using MusicTrainer.Logic.Tools;
using NAudio.Dsp;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioAnalyser.Experiments;

public class FFTAudioAnalyzer
{
    /// <summary>
    /// <see cref="IAudioManager"/> to capture audio data.
    /// </summary>
    private readonly IAudioManager _audioManager;
    
    /// <summary>
    /// Event to send new data to;
    /// </summary>
    public event Action<double[], string> NewDataAvailable;
    
    /// <summary>
    /// Length of FFT transformation.
    /// </summary>
    private readonly int FftLength = (int)Math.Pow(2, 14);
    
    /// <summary>
    /// Length of FFT window hop.
    /// </summary>
    private readonly int FftWindowHop = (int)Math.Pow(2, 12);

    /// <summary>
    /// Buffer for audio data.
    /// </summary>
    private readonly GenericCircularBuffer<float> _circularBuffer;

    /// <summary>
    /// FFT Transformer.
    /// </summary>
    private readonly FFTTransformer _fftTransformer;

    /// <summary>
    /// Noise reducer.
    /// </summary>
    private readonly CustomNoiseReducer _noiseReducer;

    /// <summary>
    /// Harmonics product spectrum pitch detection.
    /// </summary>
    private readonly HPSPitchDetection _noteDetector;
    
    /// <summary>
    /// Previous FFT.
    /// </summary>
    private Complex[] _previousFft;

    /// <summary>
    /// Previous audio data.
    /// </summary>
    private float[] _previousAudioData;

    /// <summary>
    /// Setup view.
    /// </summary>
    /// <param name="audioManager">Platform-specific <see cref="IAudioManager"/></param>
    public FFTAudioAnalyzer(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _audioManager.SetUp(new WaveFormat(44100, 16, 1), FftLength);

        _fftTransformer = new FFTTransformer();
        _noiseReducer = new CustomNoiseReducer();
        _noteDetector = new HPSPitchDetection(5);
        
        _circularBuffer = new GenericCircularBuffer<float>(FftLength * 2);
        _previousFft = new Complex[FftLength];
    }

    /// <summary>
    /// Start capture.
    /// </summary>
    public void StartCapturingAudio()
    {
        _audioManager.OnDataAvailable += (buffer, sampleRate) =>
        {
            _previousAudioData ??= buffer;
            
            var currentHop = FftWindowHop;
            while (currentHop <= FftLength)
            {
                var newBuffer = new float[FftLength];
                Array.Copy(_previousAudioData, currentHop, newBuffer, 0, FftLength - currentHop);
                Array.Copy(buffer, 0, newBuffer, FftLength - currentHop, currentHop);
                
                ApplySignalWindowing.ApplyWindowing(ref buffer, WindowingAlgorithm.Hamming);

                var currentFft = _fftTransformer.Transform(buffer);
            
                //if (noiseReductionAlgorithm != NoiseReductionAlgorithm.None)
                //{
                //    _noiseReducer.UpdateNoiseProfile(magnitudes);
                //    _noiseReducer.ApplyNoiseReduction(ref magnitudes, noiseReductionAlgorithm);
                //}
                _noiseReducer.UpdateNoiseProfile(currentFft);
                _noiseReducer.ApplyNoiseReduction(ref currentFft, NoiseReductionAlgorithm.WienerFiltering);

                var magnitudes = currentFft.Take(currentFft.Length/2).Select(c => c.Magnitude()).ToArray();
                NormalizeMagnitudes(ref magnitudes);
            
                var notesPlayed = _noteDetector.DetectNotes(magnitudes, sampleRate);
                NewDataAvailable(magnitudes, string.Join(", ", notesPlayed));

                currentHop += FftWindowHop;
                _previousFft = currentFft;
            }
            
            _previousAudioData = buffer;
        };

        _audioManager.StartCapturingAudio();
    }

    /// <summary>
    /// Stop capture
    /// </summary>
    public void StopCapturingAudio()
    {
        _audioManager.StopCapturingAudio();
    }
    
    /// <summary>
    /// Normalize FFT magnitudes so the values be in range from 0 to 1.
    /// </summary>
    private void NormalizeMagnitudes(ref double[] magnitudes)
    {
        var maxMagnitude = magnitudes.Max();
        if (maxMagnitude == 0) return;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            magnitudes[i] /= maxMagnitude;
        }
    }
}