namespace Rhynohunt.Core;

/// <summary>
/// Represents a single audio track in the mixer, holding an optional <see cref="AudioClip"/>
/// along with per-track gain, pan, mute, and solo settings.
/// </summary>
public class Track
{
    /// <summary>Gets or sets the display name of this track.</summary>
    public string Name { get; set; }

    /// <summary>Gets the audio clip currently loaded on this track, or <c>null</c> if no clip is loaded.</summary>
    public AudioClip? Clip { get; private set; }

    /// <summary>
    /// Gets or sets the gain (volume multiplier) for this track.
    /// </summary>
    /// <value>0.0 is silent; 1.0 is full volume. Values above 1.0 amplify the signal.</value>
    public float Gain { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the stereo pan position for this track.
    /// </summary>
    /// <value>-1.0 is full left; 0.0 is center; 1.0 is full right.</value>
    public float Pan { get; set; } = 0.0f;

    /// <summary>Gets or sets a value indicating whether this track is muted.</summary>
    public bool IsMuted { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this track is soloed.
    /// When any track is soloed, only soloed tracks are rendered by the <see cref="Mixer"/>.
    /// </summary>
    public bool IsSolo { get; set; } = false;

    /// <summary>Initializes a new <see cref="Track"/> with the given name and no clip loaded.</summary>
    /// <param name="name">The display name for this track.</param>
    public Track(string name)
    {
        Name = name;
    }

    /// <summary>Loads the specified <see cref="AudioClip"/> onto this track, replacing any existing clip.</summary>
    /// <param name="clip">The clip to load.</param>
    public void LoadClip(AudioClip clip)
    {
        Clip = clip;
    }

    /// <summary>Removes the currently loaded clip from this track.</summary>
    public void ClearClip()
    {
        Clip = null;
    }

    /// <summary>Gets a value indicating whether this track has a clip loaded.</summary>
    public bool HasClip => Clip != null;
}
