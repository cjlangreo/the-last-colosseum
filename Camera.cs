using Godot;
using System;
using RandomBattles.Fighters;

public partial class Camera : Camera2D
{
  private Fighter _fighter1;
  private Fighter _fighter2;

  private Fighter GetFighter(Team fighter)
  {
    return (Fighter)GetTree().GetFirstNodeInGroup(fighter == Team.A ? "Team A" : "Team B");
  }

  public override void _Process(double delta)
  {
    _fighter1 ??= GetFighter(Team.A);
    _fighter2 ??= GetFighter(Team.B);

    bool isFighter1Alive = IsInstanceValid(_fighter1);
    bool isFighter2Alive = IsInstanceValid(_fighter2);
    
    if (isFighter1Alive && isFighter2Alive)
    {
      GlobalPosition = new((_fighter1.GlobalPosition.X + _fighter2.GlobalPosition.X) * 0.5f, GlobalPosition.Y);
    }
    else if (isFighter1Alive || isFighter2Alive)
    {
      GlobalPosition = IsInstanceValid(_fighter1) ? _fighter1.GlobalPosition : _fighter2.GlobalPosition;
    }
    else
    {
      GlobalPosition = new Vector2(0,0);
    }
  }

}
