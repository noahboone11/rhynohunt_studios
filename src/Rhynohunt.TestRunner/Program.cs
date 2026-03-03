using Rhynohunt.Core;
using Rhynohunt.AudioEngine;

const string AudioFile = "/Users/noahboone/repos/rhynohunt_studios/audio_files/ff-16b-2c-44100hz.mp3";

// ── Task 1: Multiple clips with timeline positions ────────────────────────────
Console.WriteLine("=== Task 1: Multiple clips with timeline positions ===");

var clip1 = AudioClip.Load(AudioFile);
var clip2 = AudioClip.Load(AudioFile);

var multiTrack = new Track("Multi");
multiTrack.AddClip(clip1, TimeSpan.Zero);
multiTrack.AddClip(clip2, TimeSpan.FromSeconds(5));

Console.WriteLine($"  Clips on track : {multiTrack.Clips.Count}");
Console.WriteLine($"  Clip 1 start   : {multiTrack.Clips[0].StartTime}");
Console.WriteLine($"  Clip 2 start   : {multiTrack.Clips[1].StartTime}");

var mixer1 = new Mixer();
mixer1.AddTrack(multiTrack);
float[] buf = new float[512];
mixer1.Render(0, buf, 256);   // should produce non-zero samples
Console.WriteLine($"  Mixer rendered without error. First sample: {buf[0]:F6}");

// ── Task 2: TransportController ───────────────────────────────────────────────
Console.WriteLine("\n=== Task 2: TransportController ===");

var session2 = new Session();
var t2 = session2.AddTrack("Transport test");
session2.LoadClipOnTrack(t2, AudioFile, TimeSpan.Zero);

using var controller = new TransportController(AudioEngine.DefaultOutputDevice());
controller.Mixer.AddTrack(t2);

int timeEvents  = 0;
int stateEvents = 0;
controller.TimeChanged          += () => timeEvents++;
controller.PlaybackStateChanged += () => stateEvents++;

Console.WriteLine($"  TotalTime      : {controller.TotalTime:g}");
Console.WriteLine($"  IsPlaying      : {controller.IsPlaying}");

controller.Play();
Thread.Sleep(1500);
controller.Seek(TimeSpan.FromSeconds(2));
Thread.Sleep(500);
controller.Pause();
Thread.Sleep(300);
controller.Play();
Thread.Sleep(500);
controller.Stop();

Console.WriteLine($"  PlaybackStateChanged fired : {stateEvents}x");
Console.WriteLine($"  TimeChanged fired          : {timeEvents}x");
Console.WriteLine($"  CurrentTime after stop     : {controller.CurrentTime}");

// ── Task 3: Effects ───────────────────────────────────────────────────────────
Console.WriteLine("\n=== Task 3: Effects ===");

var effectTrack = new Track("FX");
effectTrack.AddClip(AudioClip.Load(AudioFile), TimeSpan.Zero);
effectTrack.AddEffect(new GainEffect { GainFactor = 0.5f });
effectTrack.AddEffect(new DelayEffect { DelayTime = TimeSpan.FromSeconds(0.25), Feedback = 0.4f });

Console.WriteLine($"  Effects on track : {effectTrack.Effects.Count}");

var fxMixer = new Mixer();
fxMixer.AddTrack(effectTrack);
float[] fxBuf = new float[512];
fxMixer.Render(0, fxBuf, 256);
Console.WriteLine($"  FX render completed. First sample: {fxBuf[0]:F6}");

effectTrack.RemoveEffect(effectTrack.Effects[0]);
Console.WriteLine($"  After RemoveEffect: {effectTrack.Effects.Count} effect(s) remaining");

// ── Task 4: Session save / load / auto-save ───────────────────────────────────
Console.WriteLine("\n=== Task 4: Session ===");

using var session = new Session();
var trackA = session.AddTrack("Drums");
var trackB = session.AddTrack("Bass");
session.LoadClipOnTrack(trackA, AudioFile, TimeSpan.Zero);
session.LoadClipOnTrack(trackA, AudioFile, TimeSpan.FromSeconds(10));
session.LoadClipOnTrack(trackB, AudioFile, TimeSpan.FromSeconds(5));
trackA.Gain = 0.8f;
trackB.Pan  = -0.3f;

string sessionPath = Path.Combine(Path.GetTempPath(), "rhynohunt_test_session.json");
session.Save(sessionPath);
Console.WriteLine($"  Saved to  : {sessionPath}");

var loaded = Session.Load(sessionPath);
Console.WriteLine($"  Loaded    : {loaded.Tracks.Count} tracks");
Console.WriteLine($"  Track 0   : '{loaded.Tracks[0].Name}', {loaded.Tracks[0].Clips.Count} clip(s), Gain={loaded.Tracks[0].Gain}");
Console.WriteLine($"  Track 1   : '{loaded.Tracks[1].Name}', {loaded.Tracks[1].Clips.Count} clip(s), Pan={loaded.Tracks[1].Pan}");

session.EnableAutoSave(sessionPath, intervalMinutes: 1);
Console.WriteLine("  Auto-save enabled (1 min interval).");
session.DisableAutoSave();
Console.WriteLine("  Auto-save disabled.");

// ── Task 5: Audio export ──────────────────────────────────────────────────────
Console.WriteLine("\n=== Task 5: Audio export ===");

string wavPath = Path.Combine(Path.GetTempPath(), "rhynohunt_export.wav");
AudioExporter.ExportWav(session, wavPath);
Console.WriteLine($"  WAV exported : {wavPath} ({new FileInfo(wavPath).Length:N0} bytes)");

string mp3Path = Path.Combine(Path.GetTempPath(), "rhynohunt_export.mp3");
AudioExporter.ExportMp3(session, mp3Path);
Console.WriteLine($"  MP3 exported : {mp3Path} ({new FileInfo(mp3Path).Length:N0} bytes)");

Console.WriteLine("\nAll smoke tests passed.");
