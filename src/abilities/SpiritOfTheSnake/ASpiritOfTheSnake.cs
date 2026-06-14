using Godot;
using System;

namespace TheLastColosseum.Abilities;
public partial class ASpiritOfTheSnake : Ability
{
  [Export] public int AgiBoost {set;get;} = 3;
  


    public override void InitAbility()
    {
      base.InitAbility();
      SetBlinkShaderColor(Colors.Green);
    }

    public override void UseAbility()
    {
        base.UseAbility();
        Fighter.Agility += AgiBoost;
    }

    public override void EndAbility()
    {
        base.EndAbility();
        Fighter.Agility -= AgiBoost;
    }
}
