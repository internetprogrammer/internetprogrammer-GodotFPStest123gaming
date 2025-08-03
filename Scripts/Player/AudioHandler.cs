using Godot;

public partial class AudioHandler : AudioStreamPlayer3D
{
    SoundEvent SoundEvent;
    public AudioHandler(string Path = "res://path_to_resource", float VolumeDB = 10, float MaxDB = 10, float Pitch = 1, bool Suspicious = false, float range = 25f)
    {
        if (Suspicious) //this system is le not cool so le replace
        {
            SoundEvent = new SoundEvent(range);
            AddChild(SoundEvent);
        }
        AudioStream AudioStream = ResourceLoader.Load<AudioStream>(Path);
        if (AudioStream != null)
        {
            Stream = AudioStream;
        }
        else
        {
            GD.PrintErr("Failed to load Audio");
        }
    }
    public void play()
    {
        if (SoundEvent != null) { SoundEvent.Check();}
        
        this.Play();
    }
}
