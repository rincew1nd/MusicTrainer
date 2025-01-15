using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MusicTrainer.Logic.AudioManager;

public interface IAudioManager
{
    void SetUp(WaveFormat waveFormat, int fftLength);
    void StartCapturingAudio(Func<float[], int, Task> processor);
    void StopCapturingAudio();
}