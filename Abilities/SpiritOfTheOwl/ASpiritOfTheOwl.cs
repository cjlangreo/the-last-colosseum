using Godot;
using System;

public partial class ASpiritOfTheOwl : Ability
{
  [Export] public int AbIntBonus = 3;


  public override void UseAbility()
  {
    base.UseAbility();
    Fighter.Intelligence += AbIntBonus;
  }

  public override void InitAbility()
  {
    base.InitAbility();
    SetBlinkShaderColor(Colors.Blue);
  }
  public override void EndAbility()
  {
    base.EndAbility();
    Fighter.Intelligence -= AbIntBonus;
  }
}
