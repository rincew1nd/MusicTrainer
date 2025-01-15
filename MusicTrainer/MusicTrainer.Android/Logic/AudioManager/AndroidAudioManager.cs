using System;
using MusicTrainer.Logic.AudioManager;
using NAudio.Wave;

namespace MusicTrainer.Android.Logic.AudioManager;

public class AndroidAudioManager : BaseAudioManager
{
    public override void SetUp(WaveFormat waveFormat, int fftLength)
    {
        base.SetUp(waveFormat, fftLength);

        _waveIn = new AndroidWaveIn()
        {
            BufferMilliseconds = fftLength
        };
        _waveIn.WaveFormat = waveFormat;
    }
}