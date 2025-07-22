using Godot;

public partial class AudioHandler : AudioStreamPlayer3D
{

    public AudioHandler(string Path = "res://path_to_resource", float VolumeDB = 10, float MaxDB = 10, float Pitch = 1)
    {

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
}
