// https://gist.github.com/SineVector241/58152564e615066132b081e8e2d00645
// Kudos to SineVector241 (https://gist.github.com/SineVector241)

using System;
using System.Threading;
using Android.Media;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MusicTrainer.Android.Logic.AudioManager;

public sealed class AndroidWaveIn : IWaveIn
{
    private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;
    private CaptureState _captureState = CaptureState.Stopped;
    private AudioSource audioSource { get; set; } = AudioSource.Mic;
    public int BufferMilliseconds { get; set; } = 100;
    
    private AudioRecord? _audioRecord;

    public WaveFormat? WaveFormat { get; set; }

    public event EventHandler<WaveInEventArgs>? DataAvailable;
    public event EventHandler<StoppedEventArgs>? RecordingStopped;

    public void StartRecording()
    {
        //Starting capture procedure
        OpenRecorder();

        //Check if we are already recording.
        if (_captureState == CaptureState.Capturing)
        {
            return;
        }

        //Make sure that we have some format to use.
        if (WaveFormat == null)
        {
            throw new ArgumentNullException(nameof(WaveFormat));
        }

        _captureState = CaptureState.Starting;
        _audioRecord.StartRecording();
        ThreadPool.QueueUserWorkItem((_) => RecordThread(), null);
    }

    public void StopRecording()
    {
        if (_audioRecord == null)
        {
            return;
        }

        //Check if it has already been stopped
        if (_captureState != CaptureState.Stopped)
        {
            _captureState = CaptureState.Stopped;
            CloseRecorder();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        //GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_captureState != CaptureState.Stopped)
            {
                StopRecording();
            }

            _audioRecord?.Release();
            _audioRecord?.Dispose();
            _audioRecord = null;
        }
    }

    private void OpenRecorder()
    {
        //We want to make sure the recorder is definitely closed.
        CloseRecorder();
        Encoding encoding;

        //Set the encoding
        if (WaveFormat!.Encoding == WaveFormatEncoding.Pcm || WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            encoding = WaveFormat.BitsPerSample switch
            {
                8 => Encoding.Pcm8bit,
                16 => Encoding.Pcm16bit,
                32 => Encoding.PcmFloat,
                _ => throw new ArgumentException("Input wave provider must be 8-bit, 16-bit or 32bit",
                    nameof(WaveFormat))
            };
        }
        else
        {
            throw new ArgumentException("Input wave provider must be PCM or IEEE Float", nameof(WaveFormat));
        }

        //Set the channel type. Only accepts Mono or Stereo
        var channelMask = WaveFormat.Channels switch
        {
            1 => ChannelIn.Mono,
            2 => ChannelIn.Stereo,
            _ => throw new ArgumentException("Input wave provider must be mono or stereo", nameof(WaveFormat))
        };

        //Determine the buffer size
        var bufferSize = BufferMilliseconds;
        if (bufferSize % WaveFormat.BlockAlign != 0)
        {
            bufferSize -= bufferSize % WaveFormat.BlockAlign;
        }

        //Determine min buffer size.
        var minBufferSize = AudioRecord.GetMinBufferSize(WaveFormat.SampleRate, channelMask, encoding);
        if (bufferSize < minBufferSize)
        {
            bufferSize = minBufferSize;
        }

        //Create the AudioRecord Object.
        _audioRecord = new AudioRecord(audioSource, WaveFormat.SampleRate, channelMask, encoding, bufferSize);
    }

    private void CloseRecorder()
    {
        //Make sure that the recorder was opened
        if (_audioRecord?.RecordingState != RecordState.Stopped)
        {
            _audioRecord?.Stop();
            _audioRecord?.Release();
            _audioRecord?.Dispose();
            _audioRecord = null;
        }
    }

    private void RecordThread()
    {
        Exception? exception = null;
        try
        {
            RecordingLogic();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            _captureState = CaptureState.Stopped;
            RaiseRecordingStoppedEvent(exception);
        }
    }

    private void RaiseRecordingStoppedEvent(Exception? e)
    {
        var handler = RecordingStopped;
        if (handler != null)
        {
            if (_synchronizationContext == null)
            {
                handler(this, new StoppedEventArgs(e));
            }
            else
            {
                _synchronizationContext.Post(_ => handler(this, new StoppedEventArgs(e)), null);
            }
        }
    }

    private void RecordingLogic()
    {
        if (_audioRecord == null)
        {
            throw new InvalidOperationException("Audio recorder is not initialized");
        }
        
        //Initialize the wave buffer
        int bufferSize = BufferMilliseconds;
        if (bufferSize % WaveFormat!.BlockAlign != 0)
        {
            bufferSize -= bufferSize % WaveFormat.BlockAlign;
        }

        _captureState = CaptureState.Capturing;

        //Run the record loop
        while (_captureState != CaptureState.Stopped)
        {
            if (_captureState != CaptureState.Capturing)
            {
                Thread.Sleep(10);
                continue;
            }

            if (WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                var byteBuffer = new byte[bufferSize];
                var bytesRead = _audioRecord.Read(byteBuffer, 0, bufferSize);
                if (bytesRead > 0)
                {
                    DataAvailable?.Invoke(this, new WaveInEventArgs(byteBuffer, bytesRead));
                }
            }
            else if (WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                var floatBuffer = new float[bufferSize / 4];
                var byteBuffer = new byte[bufferSize];
                var floatsRead = _audioRecord.Read(floatBuffer, 0, floatBuffer.Length, 0);
                Buffer.BlockCopy(floatBuffer, 0, byteBuffer, 0, byteBuffer.Length);
                if (floatsRead > 0)
                {
                    DataAvailable?.Invoke(this, new WaveInEventArgs(byteBuffer, floatsRead * 4));
                }
            }
        }
    }
}