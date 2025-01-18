using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Browser.Logic.AudioManager;

// TODO Doesn't work. "Error starting audio recording: winmm.dll"
// Need to implement custom IWaveIn for browser like it was done for android...

/// <summary>
/// Browser microphone audio manager.
/// </summary>
public sealed class BrowserAudioManager : BaseAudioManager
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