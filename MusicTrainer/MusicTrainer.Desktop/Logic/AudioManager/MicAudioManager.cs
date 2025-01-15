using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Desktop.Logic.AudioManager;

public sealed class MicAudioManager : BaseAudioManager
{
    public override void SetUp(WaveFormat waveFormat, int fftLength)
    {
        base.SetUp(waveFormat, fftLength);

        _waveIn = new WaveInEvent()
        {
            DeviceNumber = 0
        };
        _waveIn.WaveFormat = waveFormat;
    }
}