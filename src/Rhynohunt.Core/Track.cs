namespace Rhynohunt.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class Track: INotifyPropertyChanged
{
    private readonly ObservableCollection<AudioClip> _clips = new();
    private readonly List<IEffect> _effects = new List<IEffect>();
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; set; }
    public ObservableCollection<AudioClip> Clips => _clips;
    public IReadOnlyList<IEffect> Effects => _effects;

    private float defaultGain = 1.0f;
    public float Gain
    {
        get => defaultGain;
        set
        {
            if (defaultGain == value) return;
            defaultGain = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Gain)));
        }
    }
    public float defaultPan = 0.0f;
    public float Pan
    {
        get => defaultPan;
        set
        {
            if (defaultPan == value) return;
            defaultPan = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Gain)));
        }
    }
    public bool IsMuted { get; set; } = false;
    public bool IsSolo { get; set; } = false;

    public Track(string name)
    {
        Name = name;
    }

    public void AddClip(AudioClip clip, TimeSpan startTime)
    {
        clip.StartTime = startTime;
        _clips.Add(clip);
    }

    public void RemoveClip(AudioClip clip) => _clips.Remove(clip);

    public bool HasClips => _clips.Count > 0;

    public void AddEffect(IEffect effect) => _effects.Add(effect);
    public void RemoveEffect(IEffect effect) => _effects.Remove(effect);
}
