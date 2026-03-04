# Rhynohunt Studios

A .NET 8 audio workstation engine — tracks, clips, effects, mixing, session management, and WAV/MP3 export. UI integration is in progress.

---

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (pinned to `8.0.418` via `global.json`)
- [DocFX](https://dotnet.github.io/docfx/) (only needed for docs generation)
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

## Run the Test Runner

The test runner is a console app that exercises the engine directly:

```bash
dotnet run --project src/Rhynohunt.TestRunner
```

---

## API Docs (DocFX)

Generate and serve the docs locally:

```bash
docfx docfx.json --serve
```

Then open `http://localhost:8080` in your browser. The built site lands in `_site/`.

---

## Project Structure

```
rhynohunt_studios/
├── src/
│   ├── Rhynohunt.Core/          # Domain layer — AudioClip, Track, Mixer, Session,
│   │                            #   AudioExporter, IEffect, GainEffect, DelayEffect
│   ├── Rhynohunt.AudioEngine/   # Playback layer — AudioEngine, TransportController
│   │                            #   (PortAudioSharp2)
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
| `Rhynohunt.Core` | NAudio, NLayer | Audio decoding & file I/O |
| `Rhynohunt.AudioEngine` | PortAudioSharp2 | Cross-platform audio output |
