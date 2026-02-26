using PortAudioSharp;

namespace Rhynohunt.AudioEngine;

/// <summary>
/// Provides static utility methods for testing PortAudio device enumeration,
/// sine wave playback, audio recording, and record-then-playback round-trips.
/// Intended for development and diagnostic use only.
/// </summary>
public class AudioTest
{
    /// <summary>
    /// Prints all available PortAudio devices to the console, including
    /// their index, name, and maximum input/output channel counts.
    /// </summary>
    public static void ListDevices()
    {
        PortAudio.Initialize();

        int deviceCount = PortAudio.DeviceCount;
        Console.WriteLine($"Found {deviceCount} audio devices:");

        for (int i = 0; i < deviceCount; i++)
        {
            DeviceInfo info = PortAudio.GetDeviceInfo(i);
            Console.WriteLine($"  [{i}] {info.name} - Inputs: {info.maxInputChannels} Outputs: {info.maxOutputChannels}");
        }

        PortAudio.Terminate();
    }

    /// <summary>
    /// Plays a sine wave tone on the specified output device for a given duration.
    /// The tone is output as stereo Float32 at 44100 Hz with 256 frames per buffer.
    /// </summary>
    /// <param name="deviceIndex">The PortAudio output device index to use.</param>
    /// <param name="frequency">The frequency of the sine wave in Hz. Defaults to 440 Hz (concert A).</param>
    /// <param name="durationSeconds">How long to play the tone, in seconds. Defaults to 3.</param>
    public static void PlaySineWave(int deviceIndex, double frequency = 440.0, int durationSeconds = 3)
    {
        PortAudio.Initialize();

        double phase = 0;
        double phaseIncrement = 2 * Math.PI * frequency / 44100;

        PortAudioSharp.Stream stream = new PortAudioSharp.Stream(
            inParams: null,
            outParams: new StreamParameters
            {
                device = deviceIndex,
                channelCount = 2,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(deviceIndex).defaultLowOutputLatency
            },
            sampleRate: 44100,
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: (IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData) =>
            {
                unsafe
                {
                    float* out_ = (float*)output;
                    for (int i = 0; i < frameCount; i++)
                    {
                        float sample = (float)Math.Sin(phase);
                        out_[i * 2] = sample;       // left
                        out_[i * 2 + 1] = sample;   // right
                        phase += phaseIncrement;
                    }
                }
                return StreamCallbackResult.Continue;
            },
            userData: IntPtr.Zero
        );

        stream.Start();
        Console.WriteLine($"Playing {frequency}hz sine wave on device [{deviceIndex}] for {durationSeconds} seconds...");
        Thread.Sleep(durationSeconds * 1000);
        stream.Stop();
        stream.Dispose();

        PortAudio.Terminate();
    }

    /// <summary>
    /// Records mono audio from the specified input device for a given duration
    /// and reports the total number of captured samples to the console.
    /// </summary>
    /// <param name="deviceIndex">The PortAudio input device index to use.</param>
    /// <param name="durationSeconds">How long to record, in seconds. Defaults to 5.</param>
    public static void RecordAudio(int deviceIndex, int durationSeconds = 5)
    {
        PortAudio.Initialize();

        List<float> recordedSamples = new List<float>();

        PortAudioSharp.Stream stream = new PortAudioSharp.Stream(
            inParams: new StreamParameters
            {
                device = deviceIndex,
                channelCount = 1,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(deviceIndex).defaultLowInputLatency
            },
            outParams: null,
            sampleRate: 44100,
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: (IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData) =>
            {
                unsafe
                {
                    float* in_ = (float*)input;
                    for (int i = 0; i < frameCount; i++)
                    {
                        recordedSamples.Add(in_[i]);
                    }
                }
                return StreamCallbackResult.Continue;
            },
            userData: IntPtr.Zero
        );

        stream.Start();
        Console.WriteLine($"Recording from device [{deviceIndex}] for {durationSeconds} seconds... speak into your mic!");
        Thread.Sleep(durationSeconds * 1000);
        stream.Stop();
        stream.Dispose();

        PortAudio.Terminate();

        Console.WriteLine($"Captured {recordedSamples.Count} samples.");
    }

    /// <summary>
    /// Records mono audio from an input device, then immediately plays it back
    /// as stereo on an output device. Useful for verifying end-to-end audio routing.
    /// </summary>
    /// <param name="inputDevice">The PortAudio input device index to record from.</param>
    /// <param name="outputDevice">The PortAudio output device index to play back through.</param>
    /// <param name="durationSeconds">How long to record, in seconds. Defaults to 5.</param>
    public static void RecordThenPlayback(int inputDevice, int outputDevice, int durationSeconds = 5)
    {
        PortAudio.Initialize();

        List<float> recordedSamples = new List<float>();

        // Record
        PortAudioSharp.Stream inputStream = new PortAudioSharp.Stream(
            inParams: new StreamParameters
            {
                device = inputDevice,
                channelCount = 1,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(inputDevice).defaultLowInputLatency
            },
            outParams: null,
            sampleRate: 44100,
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: (IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData) =>
            {
                unsafe
                {
                    float* in_ = (float*)input;
                    for (int i = 0; i < frameCount; i++)
                        recordedSamples.Add(in_[i]);
                }
                return StreamCallbackResult.Continue;
            },
            userData: IntPtr.Zero
        );

        inputStream.Start();
        Console.WriteLine($"Recording for {durationSeconds} seconds... say something!");
        Thread.Sleep(durationSeconds * 1000);
        inputStream.Stop();
        inputStream.Dispose();

        Console.WriteLine($"Captured {recordedSamples.Count} samples. Playing back...");

        // Playback
        int playbackIndex = 0;

        PortAudioSharp.Stream outputStream = new PortAudioSharp.Stream(
            inParams: null,
            outParams: new StreamParameters
            {
                device = outputDevice,
                channelCount = 2,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = PortAudio.GetDeviceInfo(outputDevice).defaultLowOutputLatency
            },
            sampleRate: 44100,
            framesPerBuffer: 256,
            streamFlags: StreamFlags.ClipOff,
            callback: (IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData) =>
            {
                unsafe
                {
                    float* out_ = (float*)output;
                    for (int i = 0; i < frameCount; i++)
                    {
                        float sample = playbackIndex < recordedSamples.Count ? recordedSamples[playbackIndex++] : 0f;
                        out_[i * 2] = sample;       // left
                        out_[i * 2 + 1] = sample;   // right
                    }
                }
                return playbackIndex >= recordedSamples.Count
                    ? StreamCallbackResult.Complete
                    : StreamCallbackResult.Continue;
            },
            userData: IntPtr.Zero
        );

        outputStream.Start();
        Console.WriteLine("Playing back...");
        Thread.Sleep((durationSeconds + 1) * 1000);
        outputStream.Stop();
        outputStream.Dispose();

        PortAudio.Terminate();
        Console.WriteLine("Done.");
    }
}
