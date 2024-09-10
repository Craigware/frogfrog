using Godot;
using System;

public partial class SoundSystem : Node
{

    public enum SoundEffect {
        NIL
    }

    static Godot.Collections.Array<AudioStream> SoundEffectStreams = new() {
        GD.Load<AudioStream>("res://assets/rr_mainV1.wav")
    };

    Node SoundEffectsContainer;
    Node MusicContainer;
    int MaxSoundEffects;

    Godot.Collections.Array<AudioStreamPlayer2D> SoundEffects;
    Godot.Collections.Array<AudioStreamPlayer2D> Music;

    public override void _Ready() {
        SoundEffectsContainer = new(){ Name = "SoundEffects" };
        MusicContainer = new(){ Name = "Music" };
        AddChild(SoundEffectsContainer);
        AddChild(MusicContainer);
        MaxSoundEffects = (int)ProjectSettings.GetSetting("soundsystem/maxsoundeffects");
        GD.Print(MaxSoundEffects);
        SoundEffects = new();
        SoundEffects.Resize(MaxSoundEffects);
        // PlaySoundEffect(SoundEffect.NIL);
    }

    public int GrabOpenSoundEffectSlot() {
        for (int i = 0; i < MaxSoundEffects; i++) {
            if (SoundEffects[i]==null) return i;
            if (!SoundEffects[i].Playing) {
                SoundEffectsContainer.RemoveChild(SoundEffects[i]);
                return i;
            };
        }
        throw new Exception("No open sound effect slot found.");
    }

    public void PlaySoundEffect(SoundEffect se) {
        int slot = GrabOpenSoundEffectSlot();
        SoundEffects[slot] = new() {
            Stream = SoundEffectStreams[(int)se]
        };
        SoundEffects[slot].Finished += () => { 
            SoundEffects[slot].QueueFree();
            SoundEffects[slot] = null;
        };
        SoundEffectsContainer.AddChild(SoundEffects[slot]);
        SoundEffects[slot].Playing = true;
    }

    public void TransitionMusic() {
    }
}
