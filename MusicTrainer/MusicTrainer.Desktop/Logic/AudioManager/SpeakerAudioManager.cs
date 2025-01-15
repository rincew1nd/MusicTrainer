using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Desktop.Logic.AudioManager;

public class SpeakerAudioManager : BaseAudioManager
{
    public override void SetUp(WaveFormat waveFormat, int fftLength)
    {
        base.SetUp(waveFormat, fftLength);
        
        _waveIn = new WasapiLoopbackCapture();
        _waveIn.WaveFormat = waveFormat;
    }
}