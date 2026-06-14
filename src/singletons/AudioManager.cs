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

    public AudioStreamPlayer StoneImpactPlayer1;
    public AudioStreamPlayer StoneImpactPlayer2;
    public AudioStreamPlayer StoneImpactPlayer3;

    private AudioStream[] _stoneImpactStreams =
    {
        GD.Load<AudioStream>("uid://h6qxpmxm45nb"),
        GD.Load<AudioStream>("uid://bhy0rc2yf5v5d"),
        GD.Load<AudioStream>("uid://pu607t0ot41v")
    };

    private Random _random = new();


    public override void _EnterTree()
    {
        ProcessMode = ProcessModeEnum.Always;

        TeamA.HitSoundPlayer = new() { MaxPolyphony = 10 };
        TeamA.BlockSoundPlayer = new() { MaxPolyphony = 10 };
        TeamA.SwingSoundPlayer = new() { MaxPolyphony = 10 };

        TeamB.HitSoundPlayer = new() { MaxPolyphony = 10 };
        TeamB.BlockSoundPlayer = new() { MaxPolyphony = 10 };
        TeamB.SwingSoundPlayer = new() { MaxPolyphony = 10 };

        AddChild(TeamA.HitSoundPlayer);
        AddChild(TeamA.BlockSoundPlayer);
        AddChild(TeamA.SwingSoundPlayer);

        AddChild(TeamB.HitSoundPlayer);
        AddChild(TeamB.BlockSoundPlayer);
        AddChild(TeamB.SwingSoundPlayer);

        InitStoneImpactPlayers();
    }


    private void InitStoneImpactPlayers()
    {
        float volume = 0.2f;
        StoneImpactPlayer1 = new()
        {
            Stream = _stoneImpactStreams[0],
            MaxPolyphony = 10,
            VolumeLinear = volume
        };
        StoneImpactPlayer2 = new()
        {
            Stream = _stoneImpactStreams[1],
            MaxPolyphony = 10,
            VolumeLinear = volume
        };
        StoneImpactPlayer3 = new()
        {
            Stream = _stoneImpactStreams[2],
            MaxPolyphony = 10,
            VolumeLinear = volume
        };
        AddChild(StoneImpactPlayer1);
        AddChild(StoneImpactPlayer2);
        AddChild(StoneImpactPlayer3);
    }

    public void PlayRandomStoneImpact()
    {
        int randomIndex = _random.Next(3);
        if (randomIndex == 0)
        {
            StoneImpactPlayer1.Play();
        }
        else if (randomIndex == 1)
        {
            StoneImpactPlayer2.Play();
        }
        else if (randomIndex == 2)
        {
            StoneImpactPlayer3.Play();
        }
    }
}
