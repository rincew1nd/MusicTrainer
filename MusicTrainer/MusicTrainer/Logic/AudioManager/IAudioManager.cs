using System;
using System.Threading.Tasks;

namespace MusicTrainer.Logic.AudioManager;

public interface IAudioManager
{
    void StartCapturingAudio(Func<float[], int, Task> processor);
    void StopCapturingAudio();
}