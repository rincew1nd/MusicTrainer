using System;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public abstract class BaseAudioManager : IAudioManager, IDisposable
{
    /// <summary>
    /// Audio signal buffer <see cref="BufferedWaveProvider"/>.
    /// </summary>
    private BufferedWaveProvider _bufferedWaveProvider = null!;
    
    /// <summary>
    /// Required buffer size.
    /// </summary>
    private int _bufferSize;
    
    /// <summary>
    /// Platform-specific implementation of <see cref="IWaveIn"/>.
    /// </summary>
    protected IWaveIn _waveIn = null!;
    
    /// <summary>
    /// Subscribe to audio signal supply.
    /// </summary>
    public event Action<float[], int> OnDataAvailable;
    
    /// <summary>
    /// Setup audio manager.
    /// </summary>
    /// <param name="waveFormat"><see cref="WaveFormat"/> of <see cref="IWaveIn"/></param>
    /// <param name="bufferSize">Buffer size</param>
    public virtual void SetUp(WaveFormat? waveFormat, int bufferSize)
    {
        _bufferSize = bufferSize;
        _bufferedWaveProvider = new BufferedWaveProvider(_waveIn.WaveFormat)
        {
            BufferLength = _bufferSize * _waveIn.WaveFormat.BlockAlign * 3,
            DiscardOnBufferOverflow = true
        };
    }

    /// <summary>
    /// Start capturing audio signal.
    /// </summary>
    public void StartCapturingAudio()
    {
        if (_waveIn == null)
        {
            throw new InvalidOperationException($"Audio Manager was not set up correctly!");
        }
        
        try
        {
            _waveIn.StartRecording();
            _waveIn.DataAvailable += (s, e) =>
            {
                // Add samples to buffer provider
                _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);

                // Check if buffer provider have necessary buffered
                //var bufferSize = _bufferSize * _bufferedWaveProvider.WaveFormat.BlockAlign;
                //if (_bufferedWaveProvider.BufferedBytes <= bufferSize) return;
                
                // Read buffer from buffer provider
                //var buffer = new byte[bufferSize];
                var buffer = new byte[_bufferSize * _bufferedWaveProvider.WaveFormat.BlockAlign];
                var bufferRead = _bufferedWaveProvider.Read(buffer, 0, buffer.Length);
        
                if (bufferRead == 0) return;
                
                // Calculate buffer and process buffer
                var floatBuffer = ConvertToFloat(buffer, buffer.Length);
                
                // Pass audio signal to listeners
                OnDataAvailable(floatBuffer, _waveIn.WaveFormat.SampleRate);
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting audio recording: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop capturing audio signal.
    /// </summary>
    public void StopCapturingAudio()
    {
        _waveIn!.StopRecording();
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

    /// <summary>
    /// Dispose <see cref="IAudioManager"/>.
    /// </summary>
    public void Dispose()
    {
        _waveIn?.Dispose();
    }
}