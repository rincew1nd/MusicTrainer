using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Desktop.Logic.AudioManager;

/// <summary>
/// Speaker audio manager.
/// </summary>
public class SpeakerAudioManager : BaseAudioManager
{
    /// <inheritdoc/>
    public override void SetUp(WaveFormat? waveFormat, int bufferSize)
    {
        _waveIn = new WasapiLoopbackCapture();
        
        if (waveFormat != null)
        {
            _waveIn.WaveFormat = waveFormat;
        }
        
        base.SetUp(waveFormat, bufferSize);
    }
}