using Godot;
using System;

public partial class PowerupSpawner : Node2D
{
  [Export] public bool Enabled = true;
  [Export] public bool Random = true;
  [Export] public float SpawnInterval = 5.0f;
  [Export] public PackedScene PowerupScene;
  [Export] public Timer Timer;

  private Powerup _powerup;

  public override void _Ready()
  {
    if(!Enabled) return;
    Timer.WaitTime = SpawnInterval;
    Timer.Timeout += OnTimerTimeout;
    Timer.Start();
  }

  public override void _ExitTree()
  {
    Timer.Timeout -= OnTimerTimeout;
  }

  private void OnTimerTimeout()
  {
    if(IsInstanceValid(_powerup)) return;
    _powerup = PowerupScene.Instantiate<Powerup>();
    _powerup.PickedUp += OnPowerupPickedUp;
    if (Random) _powerup.Random = true;
    AddChild(_powerup);
  }

  private void OnPowerupPickedUp()
  {
    GD.Print("Powerup picked up");
    Timer.Start();
  }

}
