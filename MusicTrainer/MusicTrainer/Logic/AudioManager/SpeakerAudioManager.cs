using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public class SpeakerAudioManager : BaseAudioManager
{
    private readonly WasapiLoopbackCapture _waveIn;

    public SpeakerAudioManager(WaveFormat waveFormat, int fftLength) : base(waveFormat, fftLength)
    {
        _waveIn = new WasapiLoopbackCapture();
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