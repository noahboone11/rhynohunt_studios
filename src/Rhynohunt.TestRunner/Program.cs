using Rhynohunt.Core;

var clip = AudioClip.Load("/Users/noahboone/repos/rhynohunt_studios/audio_files/ff-16b-2c-44100hz.mp3");
Console.WriteLine($"Loaded: {clip.FilePath}");
Console.WriteLine($"Duration: {clip.Duration}");
Console.WriteLine($"Sample Rate: {clip.SampleRate}");
Console.WriteLine($"Channels: {clip.Channels}");
Console.WriteLine($"Total Samples: {clip.Samples.Length}");