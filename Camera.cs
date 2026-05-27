using Godot;
using System;

public partial class Camera : Camera2D
{
  private Fighter _fighter1;
  private Fighter _fighter2;

  private enum FighterTeam
  {
    A,
    B
  }

  private Fighter GetFighter(FighterTeam fighter)
  {
    return (Fighter)GetTree().GetFirstNodeInGroup(fighter == FighterTeam.A ? "Team A" : "Team B");
  }

  public override void _Ready()
  {
    _fighter1 = GetFighter(FighterTeam.A);
    _fighter2 = GetFighter(FighterTeam.B);
  }

  public override void _Process(double delta)
  {
    bool isFighter1Alive = IsInstanceValid(_fighter1);
    bool isFighter2Alive = IsInstanceValid(_fighter2);
    
    if (isFighter1Alive && isFighter2Alive)
    {
      GlobalPosition = (_fighter1.GlobalPosition + _fighter2.GlobalPosition) * 0.5f;
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
