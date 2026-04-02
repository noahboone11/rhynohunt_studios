# Rhynohunt Studios

A multi-track audio workstation built in C# on .NET 8. Handles audio import, arrangement, mixing, effects, and WAV/MP3 export. The UI is built with Avalonia and runs cross-platform.

---

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (pinned to `8.0.418` via `global.json`)
- [DocFX](https://dotnet.github.io/docfx/) — only needed if you want to generate API docs
  ```
  dotnet tool install -g docfx
  ```

---

## Clone & Build

```bash
git clone <repo-url>
cd rhynohunt_studios

dotnet restore
dotnet build
```

---

## Running the App

```bash
dotnet run --project src/Rhynohunt.UI
```

---

## Console Test Runner

Exercises the engine directly without the UI — useful for validating playback, mixing, effects, and export:

```bash
dotnet run --project src/Rhynohunt.TestRunner
```

---

## Features

- Multi-track timeline with draggable clips
- Per-track gain and pan controls
- Mute and solo per track
- Time scrubbing via the playhead
- Play, pause, and stop transport controls
- Per-track effects chain (Gain, Delay)
- Import WAV and MP3 files
- Export session to WAV
- Session save and load (JSON)

---

## Project Structure

```
rhynohunt_studios/
├── src/
│   ├── Rhynohunt.Core/          # Domain layer — AudioClip, Track, Mixer, Session,
│   │                            #   AudioExporter, IEffect, GainEffect, DelayEffect
│   ├── Rhynohunt.AudioEngine/   # Playback layer — AudioEngine, TransportController
│   │                            #   (PortAudioSharp2)
│   ├── Rhynohunt.UI/            # Avalonia desktop UI
│   └── Rhynohunt.TestRunner/    # Console test harness
├── docs/                        # Hand-written DocFX articles
├── audio_files/                 # Sample audio assets
├── class-diagram.puml           # PlantUML class diagram
├── docfx.json                   # DocFX config
├── global.json                  # SDK version pin
└── Rhynohunt.sln
```

### Key Dependencies

| Project | Package | Purpose |
|---|---|---|
| `Rhynohunt.Core` | NAudio, NLayer | Audio decoding and file I/O |
| `Rhynohunt.AudioEngine` | PortAudioSharp2 | Cross-platform audio output |
| `Rhynohunt.UI` | Avalonia 11 | Desktop GUI framework |

---

## API Docs (DocFX)

```bash
docfx docfx.json --serve
```

Builds to `_site/` and serves at `http://localhost:8080`.
