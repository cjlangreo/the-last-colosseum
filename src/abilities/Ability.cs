
using System;
using System.Collections.Generic;
using Godot;
using TheLastColosseum.Fighters;
using TheLastColosseum.Utils;
using static TheLastColosseum.Utils.Debug;

namespace TheLastColosseum.Abilities;

public enum AbilityEnum
{
  RaiseShield,
  MarkOfTheBear,
  MarkOfTheOwl,
  MarkofTheSnake,
  DisciplinedStrike,
  FanaxeSwing
}

[Icon("res://assets/IconGodotNode/node_2D/icon_ring.png")]
[GlobalClass]
public partial class Ability : Node2D
{
  public string AbilityName => AbilityNames[AbilityEnum];
  [Export] public AbilityEnum AbilityEnum;
  [Export] public CompressedTexture2D AbilityIcon { set; get; }
  [Export] public float AbCooldownBase { set; get; } = 10;
  [Export] public float AbDuration { set; get; } = 0;

  [Export] public Trigger trigger = Trigger.Auto;
  [Export] public bool HasBeforeDamageEffect = false;

  private const string BlinkShaderUID = "uid://dyrxsvx04xljg";


  public static Dictionary<AbilityEnum, string> AbilityNames = new(){
      {AbilityEnum.DisciplinedStrike, "Disciplined Strike"},
      {AbilityEnum.MarkOfTheBear, "Mark of the Bear"},
      {AbilityEnum.MarkofTheSnake, "Mark of the Snake"},
      {AbilityEnum.MarkOfTheOwl, "Mark of the Owl"},
      {AbilityEnum.RaiseShield, "Raise Shield"},
  };

  public enum Trigger
  {
    Auto,
    BeforeDamage,
    AfterDamage
  }


  public Fighter Fighter => GetParent<Fighter>();
  private ShaderMaterial BlinkShader => (ShaderMaterial)Fighter.Material;
  private Timer _abilityCooldownTimer;
  private Timer _abilityTimer;
  private float AbCooldown => AbCooldownBase - (AbCooldownBase * Fighter.Intelligence * 0.12f);

  public double AbTimeRemainPercent => _abilityTimer.TimeLeft / AbDuration;
  public double AbCooldownTimeRemainPercent => 1 - _abilityCooldownTimer.TimeLeft / AbCooldown;
  // public double AbCooldownTimeRemainPercent => 0.5;

  public bool Active
  {
    get;
    set
    {
      field = value;
      if (AbDuration > 0) BlinkShader.SetShaderParameter("on", value);
    }
  } = false;

  public override void _Ready()
  {
    InitAbility();
  }


  public virtual void InitAbility()
  {
    Debug.PrintDebug($"Initializing Ability", $"{Fighter.FighterName}:{AbilityName}");
    _abilityCooldownTimer = new() { Name = "AbilityCooldownTimer", OneShot = true };
    _abilityCooldownTimer.Timeout += UseAbility;
    AddChild(_abilityCooldownTimer);
    _abilityCooldownTimer.Start(AbCooldown);


    if (AbDuration > 0)
    {
      _abilityTimer = new() { Name = "AbilityTimer", OneShot = true };
      _abilityTimer.Timeout += EndAbility;
      AddChild(_abilityTimer);

      Fighter.Material = new ShaderMaterial() { Shader = GD.Load<Shader>(BlinkShaderUID) };
    }
  }

  public void SetBlinkShaderColor(Color color)
  {
    BlinkShader.SetShaderParameter("color", color);
  }

  public virtual void UseAbility()
  {
    string toPrint = $"used ability {AbilityName} with cooldown {AbCooldown}";
    if (AbDuration > 0)
    {
      toPrint += $" with duration of {AbDuration}";
      _abilityTimer.Start(AbDuration);
      Active = true;
    }
    PrintDebug(toPrint, $"{Fighter.Name}:{Fighter.team}");
  }

  public virtual void EndAbility()
  {
    _abilityCooldownTimer.Start(AbCooldown);

    if (AbDuration > 0)
    {
      Active = false;
    }
  }

  public virtual void FighterDie()
  {
    _abilityTimer?.Stop();
    _abilityCooldownTimer.Stop();
    Active = false;
  }


  public virtual Tuple<HitStatus, double> AbilityBeforeDamage(HitStatus hitStatus, double damage)
  {
    return new(hitStatus, damage);
  }
}