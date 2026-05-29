using Godot;
using RandomBattles.Utils;
using System;

public partial class Main : Node2D
{

  [Export] public bool Debug = false;
  public override void _Ready()
  {
    if(Debug) EventManager.RoundEnd += OnRoundEnd;
  }

  public override void _ExitTree()
  {
    EventManager.RoundEnd -= OnRoundEnd;
  }

  private void OnRoundEnd(Fighter.Team team)
  {
    EventManager.Rounds += 1;
    if (team == Fighter.Team.B)
    {
      EventManager.TeamAPoints += 1;
    }
    else
    {
      EventManager.TeamBPoints += 1;
    }
    GD.Print($"Rounds: {EventManager.Rounds}");
    GD.Print($"Team A: {EventManager.TeamAPoints}");
    GD.Print($"Team B: {EventManager.TeamBPoints}");
    GD.Print();

    GetTree().CallDeferred("reload_current_scene");
  }

}
