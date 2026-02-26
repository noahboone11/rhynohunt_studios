using Rhynohunt.Core;

var clip = AudioClip.Load("/Users/noahboone/repos/rhynohunt_studios/audio_files/ff-16b-2c-44100hz.mp3");

var track = new Track("Track 1");
track.LoadClip(clip);
track.Gain = 0.8f;
track.Pan = -0.5f;

Console.WriteLine($"Track: {track.Name}");
Console.WriteLine($"Clip: {track.Clip?.FilePath}");
Console.WriteLine($"Duration: {track.Clip?.Duration}");
Console.WriteLine($"Gain: {track.Gain}");
Console.WriteLine($"Pan: {track.Pan}");
Console.WriteLine($"Muted: {track.IsMuted}");