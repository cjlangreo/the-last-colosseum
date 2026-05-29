using Godot;
using System;
using static RandomBattles.Utils.Stats;

public partial class Ninja : Fighter, IHasAbility
{

  [Export] public float AbilityCooldownBase { set; get; } = 10;
  [Export] public float AbilityDuration { set; get; } = 10;

  private const int AbAgiBonus = 3;
  private Timer _abilityCooldownTimer;
  private Timer _abilityTimer;
  private double _abilityCooldown;


  public override void _Ready()
  {
    base._Ready();
    InitAbility();

  }

  public override void _ExitTree()
  {
    base._ExitTree();
    _abilityCooldownTimer.Timeout -= OnAbilityCooldownTimerTimeout;
    _abilityTimer.Timeout -= OnAbilityTimeout;
  }

  public void InitAbility()
  {
    _abilityCooldownTimer = new() { OneShot = true };
    _abilityTimer = new() { OneShot = true };
    AddChild(_abilityCooldownTimer);
    AddChild(_abilityTimer);

    _abilityCooldownTimer.Timeout += OnAbilityCooldownTimerTimeout;
    _abilityTimer.Timeout += OnAbilityTimeout;

    _abilityCooldown = GetAbilityCooldown(AbilityCooldownBase, Intelligence);
    _abilityCooldownTimer.Start(_abilityCooldown);
  }

  public void UseAbility()
  {
    ((ShaderMaterial)Material).SetShaderParameter("mode", 1);
    Agility += AbAgiBonus;
    _abilityTimer.Start(AbilityDuration);
  }
  public void OnAbilityCooldownTimerTimeout()
  {
    UseAbility();
  }
  public void OnAbilityTimeout()
  {
    ((ShaderMaterial)Material).SetShaderParameter("mode", 0);
    Agility -= AbAgiBonus;
    _abilityCooldownTimer.Start(_abilityCooldown);
  }
}
