using Godot;
using System;
using TheLastColosseumFighters;
using System.Threading.Tasks;

public partial class ADisciplinedStrike : Ability
{
  [Export] float originDamage;
  private bool _abilityActive = false;
  public override void InitAbility()
  {
    base.InitAbility();
    Fighter.Weapon.Attacked += OnAttack;
  }



  public override async void UseAbility()
  {
    base.UseAbility();

    Fighter.Weapon.AtkAnimPlayer.Play("RESET");
    originDamage = Fighter.Weapon.Stats.BaseDmg;
    ToggleCollisions(false);
    Fighter.CanMove = false;
    Fighter.Weapon.Disable();
    Tween tween = CreateTween().SetParallel();
    tween.TweenProperty(Fighter.Weapon, "scale", new Vector2(1.7f,1.7f), 1);
    tween.TweenProperty(Fighter.Weapon, "Stats:BaseDmg", 20, 1);
    await ToSignal(tween, Tween.SignalName.Finished);
    Fighter.Weapon.Enable();
    _abilityActive = true;

  }

  private void OnAttack(HitStatus hitStatus)
  {
    if(!_abilityActive) return;
    Fighter.CanMove = true;
    Fighter.Weapon.Stats.BaseDmg = originDamage;
    Fighter.Weapon.Scale = Vector2.One;
    ToggleCollisions(true);
  }

  private void ToggleCollisions(bool value)
  {
    Fighter.SetCollisionLayerValue((int)(Fighter.team == Team.A ? ColLayer.A : ColLayer.B), value);
    Fighter.SetCollisionMaskValue((int)(Fighter.team == Team.A ? ColLayer.B : ColLayer.A), value);
  }

  public override void EndAbility()
  {
    base.EndAbility();
  }
}
