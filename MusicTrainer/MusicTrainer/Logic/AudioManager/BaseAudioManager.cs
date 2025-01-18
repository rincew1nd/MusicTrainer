#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public abstract class BaseAudioManager : IAudioManager, IDisposable
{
    private BufferedWaveProvider _bufferedWaveProvider;
    private int _fftLength;
    private float[] _sampleWindows;
    
    protected IWaveIn _waveIn;
    
    public virtual void SetUp(WaveFormat waveFormat, int fftLength)
    {
        _fftLength = fftLength;
        _sampleWindows = new float[fftLength * 2];
        _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
        {
            BufferLength = _fftLength * 4,
            DiscardOnBufferOverflow = true
        };
    }

    public void StartCapturingAudio(Func<float[], int, Task> processor)
    {
        if (_waveIn == null)
        {
            throw new InvalidOperationException($"Audio Manager was not set up correctly!");
        }
        
        try
        {
            _waveIn.StartRecording();
            _waveIn.DataAvailable += (s, a) => FetchAudioSignal(a, processor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting audio recording: {ex.Message}");
        }
    }

    public void StopCapturingAudio()
    {
        _waveIn!.StopRecording();
    }

    /// <summary>
    /// Fetch audio data and feed it to processor.
    /// </summary>
    /// <param name="event"><see cref="WaveInEventArgs"/></param>
    /// <param name="processor">Audio signal processor</param>
    private void FetchAudioSignal(WaveInEventArgs @event, Func<float[], int, Task> processor)
    {
        _bufferedWaveProvider.AddSamples(@event.Buffer, 0, @event.BytesRecorded);

        var buffer = new byte[_fftLength * _bufferedWaveProvider.WaveFormat.BlockAlign];
        var bytesRead = _bufferedWaveProvider.Read(buffer, 0, buffer.Length);

        if (bytesRead <= 0) return;
        
        // Replace first window with second window (window is a float FFT length size array)
        Array.Copy(_sampleWindows, _fftLength, _sampleWindows, 0, _fftLength);
            
        var floatBuffer = ConvertToFloat(buffer, bytesRead);
        
        // Add new audio signal to the second window 
        Array.Copy(floatBuffer, 0, _sampleWindows, _fftLength, _fftLength);

        // Process second half of first window and first half of a second windows (50% signal overlap)
        var processBuffer = new float[_fftLength];
        Array.Copy(_sampleWindows, _fftLength / 2, processBuffer, 0, _fftLength);
        processor(processBuffer, _bufferedWaveProvider.WaveFormat.SampleRate);
        
        // Process second window
        processor(floatBuffer, _bufferedWaveProvider.WaveFormat.SampleRate);
    }
    
    /// <summary>
    /// Convert audio signal to float values in range of -1 to 1 based on the specified format.
    /// </summary>
    /// <param name="buffer">Input audio buffer (raw PCM bytes).</param>
    /// <param name="bytesRecorded">Number of bytes recorded in the buffer.</param>
    /// <returns>Array of normalized float values.</returns>
    private float[] ConvertToFloat(byte[] buffer, int bytesRecorded)
    {
        var bitsPerSample = _bufferedWaveProvider.WaveFormat.BitsPerSample;
        var channels = _bufferedWaveProvider.WaveFormat.Channels;

        if (bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
        {
            throw new ArgumentException("Only 16, 24, or 32 bits per sample are supported.");
        }
        
        var bytesPerSample = bitsPerSample / 8;
        var totalSamples = bytesRecorded / bytesPerSample;
        var samplesPerChannel = totalSamples / channels;

        var floatBuffer = new float[samplesPerChannel]; // Mono output

        for (var i = 0; i < samplesPerChannel; i++)
        {
            float sampleSum = 0;

            for (var channel = 0; channel < channels; channel++)
            {
                var sampleIndex = (i * channels + channel) * bytesPerSample;

                switch (bitsPerSample)
                {
                    case 16:
                        var sample16 = BitConverter.ToInt16(buffer, sampleIndex);
                        sampleSum += sample16 / (float)short.MaxValue;
                        break;
                    case 24:
                        var sample24 = buffer[sampleIndex + 2] << 16 | 
                                       buffer[sampleIndex + 1] << 8 | 
                                       buffer[sampleIndex];
                        if ((sample24 & 0x800000) != 0) sample24 |= unchecked((int)0xFF000000); // Sign extension
                        sampleSum += sample24 / 8388608f;
                        break;
                    case 32:
                        var sample32 = BitConverter.ToSingle(buffer, sampleIndex);
                        sampleSum += sample32;
                        break;
                }
            }

            // Average across channels to create mono output
            floatBuffer[i] = sampleSum / channels;
        }

        return floatBuffer;
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
    }
}