using Godot;
using System;

public partial class AudioHandler : Node
{

    public AudioHandler(string Path = "res://path_to_resource", float VolumeDB = 10,float UnitSize = 10 ,float MaxDB = 10,float Pitch = 1)
    {
        AudioStreamPlayer3D Audio = new AudioStreamPlayer3D();
        AudioStream AudioStream = ResourceLoader.Load<AudioStream>("res://Audio/myfile.mp3");
        if (AudioStream != null)
        {
            Audio.Stream = AudioStream;
        }
        else
        {
            GD.PrintErr("Failed to load Audio");
        }
    }
}
