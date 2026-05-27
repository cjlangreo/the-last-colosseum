using Godot;
using System;

public partial class Crusader : Fighter, IHasAbility, ICanDie
{
  [Export] public float AbilityCooldown { set; get; } = 10;
  [Export] public Timer AbilityCooldownTimer;

  [Export] public Timer AbilityTimer;
  [Export] public float AbilityDuration { set; get; } = 10;
  [Export] public AudioStream InvincibleHitSound;
  [Export] public Sprite2D InvincibleShield;

  private bool _isInvincible;
  private Tween _invincibleShieldTween;

  public override void _Ready()
  {
    base._Ready();
    AbilityCooldownTimer.Timeout += OnAbilityCooldownTimerTimeout;
    AbilityTimer.Timeout += OnAbilityTimeout;

    AbilityCooldownTimer.Start(AbilityCooldown);
  }

  public override void Die()
  {
    base.Die();
    AbilityCooldownTimer.Stop();
    AbilityTimer.Stop();
    DisconnectSignals();
  }

  private void DisconnectSignals()
  {
    AbilityCooldownTimer.Timeout -= OnAbilityCooldownTimerTimeout;
    AbilityTimer.Timeout -= OnAbilityTimeout;
  }

  public override void _ExitTree()
  {
    base._ExitTree();
    DisconnectSignals();
  }

  public void OnAbilityCooldownTimerTimeout()
  {
    if (Dead) return;
    GD.Print(Dead);
    UseAbility();
    AbilityTimer.Start(AbilityDuration);
  }

  public void OnAbilityTimeout()
  {
    _audioStreamPlayer.Stream = HitSound;
    ((ShaderMaterial)Material).SetShaderParameter("mode", 0);
    ((ParticleProcessMaterial)BloodParticles.ProcessMaterial).Color = Colors.Red;
    _isInvincible = false;
    AbilityCooldownTimer.Start(AbilityCooldown);
  }

  public void UseAbility()
  {
    if (Dead) return;
    GD.Print("Using ability");
    _audioStreamPlayer.Stream = InvincibleHitSound;
    ((ParticleProcessMaterial)BloodParticles.ProcessMaterial).Color = Colors.Yellow;
    ((ShaderMaterial)Material).SetShaderParameter("mode", 1);
    _isInvincible = true;
  }



  public override bool TakeDamage(double damage, bool isCrit)
  {
    bool result = base.TakeDamage(_isInvincible ? 0 : damage, isCrit);

    if (_isInvincible)
    {
      if (IsInstanceValid(_invincibleShieldTween)) _invincibleShieldTween.Kill();
      _invincibleShieldTween = CreateTween();
      InvincibleShield.Modulate = Colors.White;
      _invincibleShieldTween.TweenProperty(InvincibleShield, "modulate", Colors.Transparent, 0.5);
    }

    return result;
  }
}
