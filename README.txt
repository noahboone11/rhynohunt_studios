Rhynohunt Studios - Final Code Submission
==========================================

WHAT THIS IS
------------
Rhynohunt Studios is our DAW (digital audio workstation) app built in .NET 8.
It has a core domain layer, an audio engine running on PortAudio, and a desktop
UI using Avalonia. You can load audio clips into tracks, apply effects (gain and
pan), play back the session, and export to WAV or MP3. There's also a
graphical timeline that shows the waveform.


WHAT YOU NEED TO RUN IT
-----------------------
- .NET 8 SDK (specifically 8.0.418, pinned in global.json)
  Get it at: https://dotnet.microsoft.com/download/dotnet/8.0


CODE LAYOUT
-----------
Everything we wrote is under src/. Here's the breakdown:

  src/
  ├── Rhynohunt.Core/
  │   ├── AudioClip.cs         - holds a loaded audio clip
  │   ├── Track.cs             - a track with a name and one clip
  │   ├── Mixer.cs             - combines tracks for playback
  │   ├── Session.cs           - the whole session (tracks + mixer together)
  │   ├── AudioExporter.cs     - handles WAV/MP3 export
  │   ├── IEffect.cs           - interface all effects implement
  │   ├── GainEffect.cs        - volume/gain effect
  │   └── DelayEffect.cs       - echo/delay effect
  │
  ├── Rhynohunt.AudioEngine/
  │   ├── AudioEngine.cs       - real-time audio output via PortAudio
  │   ├── TransportController.cs - play/pause/stop logic
  │   └── AudioTest.cs         - bare-bones audio test we used early on
  │
  ├── Rhynohunt.UI/
  │   ├── App.axaml / App.axaml.cs         - app entry point
  │   ├── MainWindow.axaml / .axaml.cs     - main window
  │   ├── EffectsWindow.axaml / .axaml.cs  - effects panel per track
  │   ├── NameDialog.axaml / .axaml.cs     - popup for naming things
  │   ├── TimelineCanvas.cs                - custom waveform timeline view
  │   ├── MovableBorder.cs                 - draggable panel we made
  │   └── Program.cs                       - boots the app
  │
  └── Rhynohunt.TestRunner/
      └── Program.cs           - console runner for testing the engine without UI


HOW TO BUILD AND RUN
--------------------
1. Restore packages first:
       dotnet restore

2. Build:
       dotnet build

3. Run the app:
       dotnet run --project src/Rhynohunt.UI

4. Or run the console test harness (no UI, just tests the engine):
       dotnet run --project src/Rhynohunt.TestRunner

If you're using Visual Studio 2022 or Rider you can just open Rhynohunt.sln
directly and run from there.


DEPENDENCIES (NuGet handles these automatically)
------------------------------------------------
  Rhynohunt.Core:        NAudio 2.2.1, NLayer 1.16.0
  Rhynohunt.AudioEngine: PortAudioSharp2 1.0.6
  Rhynohunt.UI:          Avalonia 11.3.12 (Desktop, Fluent, Fonts,
                         Svg.Skia, Diagnostics), PortAudioSharp2 1.0.6


WHAT'S NOT IN THE ZIP
---------------------
Per the submission guidelines we left out stuff that isn't our source code:

  - audio_files/          test audio samples (.mp3, .wav)
  - _site/                auto-generated DocFX site
  - api/                  auto-generated DocFX YAML
  - docs/                 documentation markdown files
  - src/Rhynohunt.UI/Assets/Images/rhynohunt_transparent.svg   (logo)
  - docfx.json, global.json, index.md, toc.yml, docs/toc.yml,
    docs/getting-started.md, docs/introduction.md


GROUP MEMBERS
-------------
Noah Boone, Rhys Fifield, Hunter Ryan
