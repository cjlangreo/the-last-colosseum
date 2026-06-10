using Godot;
using System;

namespace TheLastColosseum.Abilities;
public partial class ASpiritOfTheBear : Ability
{
  [Export] public int AbStrBoost = 3;


  public override void InitAbility()
  {
    base.InitAbility();
    SetBlinkShaderColor(Colors.Red);
  }

  public override void UseAbility()
  {
    base.UseAbility();
    Fighter.SetStrength(Fighter.Strength + AbStrBoost);
  }

  public override void EndAbility()
  {
    base.EndAbility();
    Fighter.SetStrength(Fighter.Strength - AbStrBoost);
  }
}
