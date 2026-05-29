using Godot;
using System;
using static RandomBattles.Utils.Stats;

public partial class Crusader : Fighter, IHasAbility, ICanDie
{
  [Export] public float AbilityCooldownBase { set; get; } = 10;
  [Export] public float AbilityDuration { set; get; } = 10;
  [Export] public AudioStream InvincibleHitSound;
  [Export] public Sprite2D InvincibleShield;

  private Timer _abilityCooldownTimer;
  private Timer _abilityTimer;
  private AudioStreamPlayer _iHitAudioPlayer;
  private bool _isInvincible;
  private Tween _invincibleShieldTween;
  private double _abilityCooldown;

  public override void _Ready()
  {
    base._Ready();
    InitAbility();

    _iHitAudioPlayer = new() { MaxPolyphony = 10, Stream = InvincibleHitSound };
    _audioManager.AddChild(_iHitAudioPlayer);

  }

  public void InitAbility()
  {
    _abilityCooldownTimer = new() { OneShot = false };
    _abilityTimer = new() { OneShot = false };
    AddChild(_abilityCooldownTimer);
    AddChild(_abilityTimer);

    _abilityCooldown = GetAbilityCooldown(AbilityCooldownBase, Intelligence);

    _abilityCooldownTimer.Timeout += OnAbilityCooldownTimerTimeout;
    _abilityTimer.Timeout += OnAbilityTimeout;
    _abilityCooldownTimer.Start(_abilityCooldown);

  }

  public override void Die()
  {
    base.Die();
    _abilityCooldownTimer.Stop();
    _abilityTimer.Stop();
  }


  private void DisconnectSignals()
  {
    _abilityCooldownTimer.Timeout -= OnAbilityCooldownTimerTimeout;
    _abilityTimer.Timeout -= OnAbilityTimeout;
  }

  public override void _ExitTree()
  {
    base._ExitTree();
    DisconnectSignals();
  }

  public void OnAbilityCooldownTimerTimeout()
  {
    // if (Dead) return;
    UseAbility();
    _abilityTimer.Start(AbilityDuration);
  }

  public void OnAbilityTimeout()
  {
    ((ShaderMaterial)Material).SetShaderParameter("mode", 0);
    ((ParticleProcessMaterial)r_BloodSplatter.ProcessMaterial).Color = Colors.Red;
    _isInvincible = false;
    _abilityCooldownTimer.Start(_abilityCooldown);
  }

  public void UseAbility()
  {
    if (Dead) return;
    ((ParticleProcessMaterial)r_BloodSplatter.ProcessMaterial).Color = Colors.Yellow;
    ((ShaderMaterial)Material).SetShaderParameter("mode", 1);
    _isInvincible = true;
  }



  public override HitStatus TakeDamage(double damage, bool isCrit)
  {
    HitStatus hitStatus = base.TakeDamage(_isInvincible ? 0 : damage, isCrit);
    if (hitStatus == HitStatus.Evade)
    {
      HitAudioPlayer.Play();
      return hitStatus;
    }
    else
    {
      if (_isInvincible)
      {
        _iHitAudioPlayer.Play();

        if (IsInstanceValid(_invincibleShieldTween)) _invincibleShieldTween.Kill();
        _invincibleShieldTween = CreateTween();
        InvincibleShield.Modulate = Colors.White;
        _invincibleShieldTween.TweenProperty(InvincibleShield, "modulate", Colors.Transparent, 0.5);
        return HitStatus.Hit;
      }
      else
      {
        HitAudioPlayer.Play();
        return HitStatus.Hit;
      }
    }
  }
}
