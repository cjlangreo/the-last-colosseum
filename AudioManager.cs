using Godot;
using System;

public partial class AudioManager : Node
{
    public Team TeamA = new();
    public Team TeamB = new();
    
    public class Team
    {
        public AudioStreamPlayer HitSoundPlayer;
        public AudioStreamPlayer BlockSoundPlayer;
        public AudioStreamPlayer SwingSoundPlayer;
    }


  public override void _EnterTree()
    {
        ProcessMode = ProcessModeEnum.Always;
        
        TeamA.HitSoundPlayer = new(){MaxPolyphony = 10};
        TeamA.BlockSoundPlayer = new(){MaxPolyphony = 10};
        TeamA.SwingSoundPlayer = new(){MaxPolyphony = 10};

        TeamB.HitSoundPlayer = new(){MaxPolyphony = 10};
        TeamB.BlockSoundPlayer = new(){MaxPolyphony = 10};
        TeamB.SwingSoundPlayer = new(){MaxPolyphony = 10};

        AddChild(TeamA.HitSoundPlayer);
        AddChild(TeamA.BlockSoundPlayer);
        AddChild(TeamA.SwingSoundPlayer);

        AddChild(TeamB.HitSoundPlayer);
        AddChild(TeamB.BlockSoundPlayer);
        AddChild(TeamB.SwingSoundPlayer);
    }
}
