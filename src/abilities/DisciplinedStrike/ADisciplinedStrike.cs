using Godot;
using TheLastColosseum.Utils;
using TheLastColosseum.Weapons;

namespace TheLastColosseum.Abilities;

public partial class ADisciplinedStrike : Ability
{
  [Export] private Timer AbilityWaitTimer;
  [Export] private Shader ShakeShader;
  private const float BonusDamage = 1.5f;
  private const float BonusAtkSpeed = 2f;
  private const float ScaleAmount = 1.7f;
  private const float ScaleDuration = 1f;
  private const float AbilityWaitTime = 3f;
  private Tween _scaleTween;
  private ShaderMaterial _shakeShaderMaterial;
  private bool _abilityActive = false;
  private float OriginBaseDamage { set; get; }
  private float OriginTrueStrikeBase { set; get; }
  private float OriginCritChanceBase { set; get; }
  private float OriginAtkSpeedBase { set; get; }



  public override void InitAbility()
  {
    base.InitAbility();

    // We don't unsubscribe because they don't outlive each other.
    Fighter.Weapon.WeaponSwingStart += OnWeaponSwingStart;
    Fighter.Weapon.WeaponSwingEnd += OnWeaponSwingEnd;
    AbilityWaitTimer.Timeout += OnWaitTimerTimeout;
    _shakeShaderMaterial = new ShaderMaterial()
    {
      Shader = ShakeShader
    };
    Fighter.Weapon.SetWeaponSpriteShaders(_shakeShaderMaterial);

    AbilityWaitTimer.WaitTime = AbilityWaitTime;
  }

  private void OnWaitTimerTimeout()
  {
    OnWeaponSwingEnd();
  }

  private void ToggleShakeShader(bool value)
  {
    _shakeShaderMaterial.SetShaderParameter("enabled", value);
  }

  private void OnWeaponSwingStart()
  {
    if (_abilityActive) return;
    if (IsInstanceValid(_scaleTween)) _scaleTween.Kill();
    Fighter.Weapon.Scale = Vector2.One;
    ToggleShakeShader(false);
  }

  public override async void UseAbility()
  {
    base.UseAbility();
    if (Fighter.Weapon.AtkAnimPlayer.IsPlaying())
    {
      await ToSignal(Fighter.Weapon, Weapon.SignalName.WeaponSwingEnd);
    }
    AbilityWaitTimer.Start();
    _abilityActive = true;
    ToggleShakeShader(true);

    StoreOriginStats();

    Fighter.CanMove = false;
    Fighter.Weapon.Disable(false);


    SetNewStats();
    if (IsInstanceValid(_scaleTween)) _scaleTween.Kill();
    _scaleTween = CreateTween();
    _scaleTween.TweenProperty(Fighter.Weapon, "scale", new Vector2(ScaleAmount, ScaleAmount), ScaleDuration);

    await ToSignal(_scaleTween, Tween.SignalName.Finished);

    if (!Fighter.Dead) Fighter.Weapon.Enable();

  }

  private void SetNewStats()
  {
    Fighter.TrueStrikeBase = 1f;
    Fighter.CritChanceBase = 1f;
    Fighter.Weapon.Stats.BaseDmg *= BonusDamage;
    Fighter.Weapon.AtkSpeedBase *= BonusAtkSpeed;
  }

  private void StoreOriginStats()
  {
    Debug.PrintDebug("Storing Original Stats", $"{Fighter.Name}:{AbilityName}");
    OriginCritChanceBase = Fighter.CritChanceBase;
    OriginTrueStrikeBase = Fighter.TrueStrikeBase;
    OriginBaseDamage = Fighter.Weapon.Stats.BaseDmg;
    OriginAtkSpeedBase = Fighter.Weapon.AtkSpeedBase;
  }

  private void RestoreOriginStats()
  {
    Fighter.CritChanceBase = OriginCritChanceBase;
    Fighter.TrueStrikeBase = OriginTrueStrikeBase;
    Fighter.Weapon.Stats.BaseDmg = OriginBaseDamage;
    Fighter.Weapon.AtkSpeedBase = OriginAtkSpeedBase;
  }

  private void OnWeaponSwingEnd()
  {
    if (!_abilityActive) return;
    _abilityActive = false;
    Fighter.CanMove = true;

    if (IsInstanceValid(_scaleTween)) _scaleTween.Kill();
    _scaleTween = CreateTween();
    _scaleTween.TweenProperty(Fighter.Weapon, "scale", Vector2.One, ScaleDuration);
    RestoreOriginStats();
    EndAbility();
  }
}
