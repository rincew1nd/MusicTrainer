using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public abstract class BaseAudioManager : IAudioManager
{
    private readonly BufferedWaveProvider _bufferedWaveProvider;
    private readonly int _fftLength;
    
    private readonly float[] _slidingWindow;
    private int _slidingWindowPosition;

    protected BaseAudioManager(WaveFormat waveFormat, int fftLength)
    {
        _fftLength = fftLength;
        _slidingWindow = new float[_fftLength];

        _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
        {
            BufferLength = _fftLength * 4,
            DiscardOnBufferOverflow = true
        };
    }

    public abstract void StartCapturingAudio(Func<float[], int, Task> processor);
    public abstract void StopCapturingAudio();

    /// <summary>
    /// Fetch audio data and feed it to processor.
    /// </summary>
    /// <param name="event"><see cref="WaveInEventArgs"/></param>
    /// <param name="processor">Audio signal processor</param>
    protected void FetchAudioSignal(WaveInEventArgs @event, Func<float[], int, Task> processor)
    {
        _bufferedWaveProvider.AddSamples(@event.Buffer, 0, @event.BytesRecorded);

        var buffer = new byte[_fftLength * 2];
        var bytesRead = _bufferedWaveProvider.Read(buffer, 0, buffer.Length);
                
        if (bytesRead > 0)
        {
            var floatBuffer = ConvertToFloat(buffer, bytesRead);

            processor(floatBuffer, _bufferedWaveProvider.WaveFormat.SampleRate)
                .ConfigureAwait(false);
            //foreach (var sample in floatBuffer)
            //{
            //    _slidingWindow[_slidingWindowPosition] = sample;
            //    _slidingWindowPosition++;
            //    if (_slidingWindowPosition >= _fftLength)
            //    {
            //        _slidingWindowPosition -= _fftLength / 4;
            //        Array.Copy(_slidingWindow, _fftLength / 4, _slidingWindow, 0, _slidingWindowPosition);
            //    
            //        processor(_slidingWindow, _bufferedWaveProvider.WaveFormat.SampleRate)
            //            .ConfigureAwait(false);
            //    }
            //}
        }
    }
    
    /// <summary>
    /// Convert audio signal to float values in range of -1 to 1.
    /// </summary>
    private float[] ConvertToFloat(byte[] buffer, int bytesRecorded)
    {
        var samples = bytesRecorded / 2;
        var floatBuffer = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            var sample = BitConverter.ToInt16(buffer, i * 2);
            floatBuffer[i] = sample / (float)short.MaxValue;
        }

        return floatBuffer;
    }
}