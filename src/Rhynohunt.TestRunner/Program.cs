using Rhynohunt.Core;

var clip = AudioClip.Load("/Users/noahboone/repos/rhynohunt_studios/audio_files/ff-16b-2c-44100hz.mp3");

var track = new Track("Track 1");
track.LoadClip(clip);

var mixer = new Mixer();
mixer.AddTrack(track);

// Render a small buffer at position 0 and check values
float[] buffer = new float[512];
mixer.Render(0, buffer, 256);

Console.WriteLine($"Tracks in mixer: {mixer.Tracks.Count}");
Console.WriteLine($"First few samples: {buffer[0]:F4}, {buffer[1]:F4}, {buffer[2]:F4}, {buffer[3]:F4}");
Console.WriteLine("Mixer render OK");
Console.WriteLine($"Raw samples at 0: {clip.Samples[0]:F4}, {clip.Samples[1]:F4}, {clip.Samples[2]:F4}, {clip.Samples[3]:F4}");
// Check samples further in
Console.WriteLine($"Samples at 1000: {clip.Samples[1000]:F4}, {clip.Samples[1001]:F4}");
Console.WriteLine($"Samples at 10000: {clip.Samples[10000]:F4}, {clip.Samples[10001]:F4}");

float[] buffer2 = new float[512];
mixer.Render(5000, buffer2, 256);
Console.WriteLine($"Mixer at position 5000: {buffer2[0]:F4}, {buffer2[1]:F4}, {buffer2[2]:F4}, {buffer2[3]:F4}");