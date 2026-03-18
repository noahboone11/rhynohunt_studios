namespace Rhynohunt.Core;
using System.Collections.ObjectModel;

/// <summary>
/// Represents a single audio track in the mixer, holding a collection of <see cref="AudioClip"/>
/// objects placed at specific timeline positions, along with per-track gain, pan, mute, and solo settings.
/// </summary>
public class Track
{
    private readonly ObservableCollection<AudioClip> _clips = new();
    private readonly List<IEffect> _effects = new List<IEffect>();

    /// <summary>Gets or sets the display name of this track.</summary>
    public string Name { get; set; }

    /// <summary>Gets the read-only ordered list of clips on this track.</summary>
    public ObservableCollection<AudioClip> Clips => _clips;

    /// <summary>Gets the read-only list of effects applied to this track during rendering.</summary>
    public IReadOnlyList<IEffect> Effects => _effects;

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

    /// <summary>Initializes a new <see cref="Track"/> with the given name and no clips loaded.</summary>
    /// <param name="name">The display name for this track.</param>
    public Track(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Adds the specified <see cref="AudioClip"/> to this track at the given timeline position.
    /// Sets <see cref="AudioClip.StartTime"/> on the clip before adding it.
    /// </summary>
    /// <param name="clip">The clip to add.</param>
    /// <param name="startTime">The position on the timeline where this clip begins.</param>
    public void AddClip(AudioClip clip, TimeSpan startTime)
    {
        clip.StartTime = startTime;
        _clips.Add(clip);
    }

    /// <summary>Removes the specified <see cref="AudioClip"/> from this track.</summary>
    /// <param name="clip">The clip to remove.</param>
    public void RemoveClip(AudioClip clip) => _clips.Remove(clip);

    /// <summary>Gets a value indicating whether this track has at least one clip loaded.</summary>
    public bool HasClips => _clips.Count > 0;

    /// <summary>Appends an effect to the end of this track's effect chain.</summary>
    /// <param name="effect">The effect to add.</param>
    public void AddEffect(IEffect effect) => _effects.Add(effect);

    /// <summary>Removes an effect from this track's effect chain.</summary>
    /// <param name="effect">The effect to remove.</param>
    public void RemoveEffect(IEffect effect) => _effects.Remove(effect);
}
