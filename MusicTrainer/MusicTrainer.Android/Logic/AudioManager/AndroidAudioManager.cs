using System;
using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Android.Logic.AudioManager;

/// <summary>
/// Android microphone audio manager.
/// </summary>
public class AndroidAudioManager : BaseAudioManager
{
    /// <inheritdoc/>
    public override void SetUp(WaveFormat? waveFormat, int bufferSize)
    {
        _waveIn = new AndroidWaveIn()
        {
            BufferMilliseconds = bufferSize
        };
        
        if (waveFormat != null)
        {
            _waveIn.WaveFormat = waveFormat;
        }
        
        base.SetUp(waveFormat, bufferSize);
    }
}