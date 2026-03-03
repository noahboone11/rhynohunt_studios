using Rhynohunt.Core;
using Rhynohunt.AudioEngine;

var clip = AudioClip.Load("/Users/noahboone/repos/rhynohunt_studios/audio_files/ff-16b-2c-44100hz.mp3");

var track = new Track("Track 1");
track.AddClip(clip, TimeSpan.Zero);

var mixer = new Mixer();
mixer.AddTrack(track);

using var engine = new AudioEngine(mixer, outputDevice: 1);
engine.Play();

Console.WriteLine("Playing for 10 seconds...");
Thread.Sleep(10000);

engine.Pause();
Console.WriteLine("Paused for 3 seconds...");
Thread.Sleep(3000);

engine.Play();
Console.WriteLine("Resumed for 5 seconds...");
Thread.Sleep(5000);

engine.Stop();
Console.WriteLine("Done.");
