using Godot;
using System;
using RandomBattles.Fighters;

namespace Abilities;
public partial class ADivineProtection : Ability
{
  [Export] private CompressedTexture2D _shieldTexture;
  private Sprite2D _invincibleShield;
  private Tween _invincibleShieldTween;
  

  public override void InitAbility()
  {
    base.InitAbility();
    _invincibleShield = new()
    {
      Name = "InvincibleShieldSprite",
      Texture = _shieldTexture,
      Modulate = Colors.Transparent
    };
    Fighter.CallDeferred("add_child", _invincibleShield);
    SetBlinkShaderColor(Colors.Yellow);
  }

  public override Tuple<HitStatus, double> AbilityBeforeDamage(HitStatus hitstatus, double damage)
  {
    base.AbilityBeforeDamage(hitstatus, damage);
    if (IsInstanceValid(_invincibleShieldTween)) _invincibleShieldTween.Kill();
    _invincibleShieldTween = CreateTween();
    _invincibleShield.Modulate = Colors.White;
    _invincibleShieldTween.TweenProperty(_invincibleShield, "modulate", Colors.Transparent, 0.5);
    return new(HitStatus.Block, 0);
  }

}
