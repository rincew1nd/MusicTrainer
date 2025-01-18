using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Desktop.Logic.AudioManager;

/// <summary>
/// Microphone audio manager.
/// </summary>
public sealed class MicAudioManager : BaseAudioManager
{
    /// <inheritdoc/>
    public override void SetUp(WaveFormat? waveFormat, int bufferSize)
    {
        _waveIn = new WaveInEvent()
        {
            DeviceNumber = 0
        };

        if (waveFormat != null)
        {
            _waveIn.WaveFormat = waveFormat;
        }
        
        base.SetUp(waveFormat, bufferSize);
    }
}