using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public sealed class MicAudioManager : BaseAudioManager
{
    private readonly WaveInEvent _waveIn;

    public MicAudioManager(WaveFormat waveFormat, int fftLength) : base(waveFormat, fftLength)
    {
        _waveIn = new WaveInEvent();
        _waveIn.DeviceNumber = 0;
        _waveIn.WaveFormat = waveFormat;
    }

    public override void StartCapturingAudio(Func<float[], int, Task> processor)
    {
        try
        {
            _waveIn.DataAvailable += (s, a) => FetchAudioSignal(a, processor);
            _waveIn.StartRecording();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting audio recording: {ex.Message}");
        }
    }

    public override void StopCapturingAudio()
    {
        _waveIn.StopRecording();
        _waveIn.Dispose();
    }
}