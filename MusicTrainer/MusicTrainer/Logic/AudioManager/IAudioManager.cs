using System;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public interface IAudioManager
{
    /// <summary>
    /// Setup audio manager.
    /// </summary>
    /// <param name="waveFormat"><see cref="WaveFormat"/> of <see cref="IWaveIn"/></param>
    void SetUp(WaveFormat? waveFormat, int bufferSize);
    
    /// <summary>
    /// Start capturing audio signal.
    /// </summary>
    void StartCapturingAudio();
    
    /// <summary>
    /// Stop capturing audio signal.
    /// </summary>
    void StopCapturingAudio();
    
    /// <summary>
    /// Subscribe to audio signal supply.
    /// </summary>
    event Action<float[], int> OnDataAvailable;
}