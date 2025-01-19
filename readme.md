# MusicTrainer

## Purpose
- Tuner for music instruments, mostly string
- Sheet music reading
- Perfect pitch training

### Roadmap

1. WIP pitch detection for keyboard/strings instruments:
   - Implement DSP
   - Implement noise reduction algorithms
   - Implement note detection
2. Basic tuner for keyboard/strings instruments.
3. Tone generator, virtual piano
4. TODO (sheet music/ear training)

### DSP

Currently, project has two DSP algorithms:

1. Fast Fourier Transform:
   - Works, but the resolution is not great
   - Need to figure out how to properly code audio signal windowing, so the app calculates FFT ~10 times per second with ability to change to different value
   - Need to implement in-between bins pitch detection to increase frequency resolution:
     - Quadratic Interpolation of FFT Peaks
     - Phase-Based Interpolation
2. Goertzel:
   - Doesn't work properly, broken

### Noise reduction

1. Adaptive Spectral Subtraction:
   - Works, but need to fix silence detection to gather correct noise profile for the signal
2. Wiener Filtering
   - Works fine

### Note detection

1. Polyphonic:
   - Works, but not for harmonics
2. Harmonics product spectrum:
   - Works fine
   - Need to implement Harmonic Frequency Interpolation