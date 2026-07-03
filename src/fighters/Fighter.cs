using Godot;
using System;
using System.Linq;
using TheLastColosseum.Abilities;
using TheLastColosseum.Utils;
using TheLastColosseum.Weapons;

namespace TheLastColosseum.Fighters;

public enum FighterEnum
{
  Orc,
  Ninja,
  Knight,
  Samurai,
  Medjay
}
public enum Team
{
  A,
  B
}

public enum HitStatus
{
  Hit,
  Block,
  Evade,
  Miss
}

public enum Stat
{
  Strength,
  Agility,
  Intelligence
}

public enum ColLayer
{
  A = 31,
  B = 32
}


public partial class Fighter : CharacterBody2D, ICanDie
{

  public string FighterName => FighterStats.FighterNames[FighterStats.Fighter];
  public int Strength
  {
    get => FighterStats.Strength;
    private set
    {
      FighterStats.Strength = value;
    }
  }

  public int Agility
  {
    get => FighterStats.Agility;
    set
    {
      FighterStats.Agility = value;
      StatUpdated?.Invoke(Stat.Agility, value);
    }
  }

  public int Intelligence
  {
    get => FighterStats.Intelligence;
    set
    {
      FighterStats.Intelligence = value;
      StatUpdated?.Invoke(Stat.Intelligence, value);
    }
  }

  public Action<Stat, int> StatUpdated;
  public Action Died;
  public FighterStats FighterStats;
  private const string CrossSpriteUID = "uid://bltixp8bd61yr";
  private const string BloodSplatterParticleUID = "uid://8iekng5vgng7";
  private const string StatusBarUID = "uid://cigdb1c0d5tou";
  public const int MaxTotalAbilityPoints = 9;
  public const int MaxAbilityPoints = 5;
  private const float HitStopDurationBase = 5f;
  private const float MovementSpeedMulti = 45f;



  protected float MovementSpeed
  {
    get => 20 + Agility * MovementSpeedMulti;
  }
  protected float Evasion
  {
    get => Agility * 0.10f;
  }
  public double Health { get; set; }
  public double MaxHealth
  {
    get => GetMaxHealth(Strength);
  }
  public float CritChance
  {
    get => Intelligence * CritChanceBase;
  }

  public float TrueStrike
  {
    get => Intelligence * TrueStrikeBase;
  }

  private float TimeScale
  {
    get;
    set
    {
      field = value;
      Engine.TimeScale = value;
    }
  }
  public float CritChanceBase = 0.18f;
  public float TrueStrikeBase = 0.02f;
  public float DamageMultiBase = 1.0f;
  public bool CanMove = true;

  public StatusBar StatusBar;
  public Sprite2D Sprite;
  public Weapon Weapon => GetChildren().OfType<Weapon>().FirstOrDefault();
  public GpuParticles2D BloodSplatter;
  public Ability Ability => GetChildren().OfType<Ability>().FirstOrDefault();
  public Team team;
  public bool HasAbility => Ability != null;
  private Tween _hitFeedbackTween;
  protected Sprite2D _deathSprite;
  public Fighter Enemy;
  protected Vector2 Direction { get; set; }
  private Tween _modulateTween;
  private Random _random;
  public bool Dead = false;
  public Color DeadColor = new(0.3f, 0.3f, 0.3f);


  private AudioManager _audioManager;
  private Tween _hitStopTween;


  public override void _EnterTree()
  {
    InitChildren();
    Name = FighterName;
    _audioManager = GetNode<AudioManager>("/root/AudioManager");
  }


  private void InitChildren()
  {
    Sprite = new()
    {
      Texture = FighterStats.FighterIcon,
      UseParentMaterial = true
    };

    // Because the samurai and medjay icons are small
    if(FighterStats.Fighter == FighterEnum.Samurai || FighterStats.Fighter == FighterEnum.Medjay)
    {
      float scale = 1.2f;
      Sprite.Scale = new(scale, scale);
    }

    BloodSplatter = new()
    {
      Emitting = false,
      Amount = 25,
      Lifetime = 0.5f,
      OneShot = true,
      Explosiveness = 1.0f,
      ProcessMaterial = GD.Load<ParticleProcessMaterial>(BloodSplatterParticleUID)
    };
    CollisionShape2D collisionShape = new()
    {
      Shape = new RectangleShape2D() { Size = new(){X = 16f, Y=16f} }
    };
    StatusBar = GD.Load<PackedScene>(StatusBarUID).Instantiate<StatusBar>();

    AddChild(Sprite);
    AddChild(BloodSplatter);
    AddChild(collisionShape);
    AddChild(StatusBar);
  }



  public override void _Ready()
  {
    Enemy = GetEnemy();
    Enemy.Died += OnWin;


    StatusBar = GetNode<StatusBar>("StatusBar");
    StatusBar.SetAbilityStatus(HasAbility);


    _random = new();
    ValidateStats();
    InitHealth();

    Direction = GetRandomDirection();
    StatusBar.SetHealthColor(team);

    _deathSprite = new()
    {
      Texture = (CompressedTexture2D)GD.Load(CrossSpriteUID),
      Visible = false
    };
    AddChild(_deathSprite);
  }

  public override void _ExitTree()
  {
    Enemy.Died -= OnWin;
  }


  private void InitHealth()
  {
    Health = MaxHealth;
    StatusBar.Health = MaxHealth;
    StatusBar.SetMaxHealth(MaxHealth);
  }


  private void ValidateStats()
  {
    if ((Agility + Strength + Intelligence) != MaxTotalAbilityPoints)
    {
      GD.PrintErr(FighterName, $" Stats does not add up to {MaxTotalAbilityPoints}!");
    }

    if (Agility > MaxAbilityPoints || Strength > MaxAbilityPoints || Intelligence > MaxAbilityPoints)
    {
      GD.PrintErr(FighterName, " Stat overflow!");
    }
  }
  public void SetStrength(int newStrength)
  {
    double healthPercent = Health / GetMaxHealth(Strength);
    Health = GetMaxHealth(newStrength) * healthPercent;
    Strength = newStrength;
    StatusBar.Health = Health;
    StatusBar.SetMaxHealth(GetMaxHealth(Strength));
    StatUpdated?.Invoke(Stat.Strength, Strength);
  }

  private double GetMaxHealth(int strength)
  {
    return strength * 50 + 50;
  }

  private void OnWin()
  {
    Debug.PrintDebug("Win!", FighterName);
    ZIndex = 1;
    Weapon.Disable(false);
    Ability.DisableAbility();
  }

  public void InitTeam(Team _team)
  {
    team = _team;
    AddToGroup(team == Team.A ? "Team A" : "Team B");
    SetCollisionLayerValue((int)(team == Team.A ? ColLayer.A : ColLayer.B), false);
    SetCollisionMaskValue((int)(team == Team.A ? ColLayer.B : ColLayer.A), false);
    SetCollisionLayerValue((int)(team == Team.A ? ColLayer.A : ColLayer.B), true);
    SetCollisionMaskValue((int)(team == Team.A ? ColLayer.B : ColLayer.A), true);
    SetCollisionMaskValue((int)ColLayer.Wall, true);
  }


  public virtual void Die()
  {
    Dead = true;
    Weapon.Disable(true);
    Ability?.FighterDie();
    SetCollisionLayerValue((int)(team == Team.A ? ColLayer.A : ColLayer.B), false);
    SetCollisionMaskValue((int)(team == Team.A ? ColLayer.B : ColLayer.A), false);
    Sprite.Modulate = DeadColor;
    _deathSprite.Scale = new(0, 0);
    _deathSprite.Show();
    Tween deathSpriteTween = CreateTween().SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
    deathSpriteTween.TweenProperty(_deathSprite, "scale", new Vector2(1, 1), 1);
    Died.Invoke();
  }

  public virtual HitStatus HitRequest(double damage, bool isCrit, float trueStrike)
  {
    HitStatus hitStatus = _random.NextDouble() <= Evasion - trueStrike ? HitStatus.Evade : HitStatus.Hit;

    if (Ability != null && Ability.Active && Ability.HasBeforeDamageEffect)
    {
      (hitStatus, damage) = Ability.AbilityBeforeDamage(hitStatus, damage);
    }

    switch (hitStatus)
    {
      case HitStatus.Hit:
        TakeDamage(damage, isCrit);
        break;
      case HitStatus.Block:
        DisplayDamageNumber("block", Colors.White);
        EmitHitParticles(Colors.Yellow);
        break;
      case HitStatus.Evade:
        DisplayDamageNumber("evade", Colors.Gray);
        break;
    }

    return hitStatus;
  }

  private void EmitHitParticles(Color color)
  {
    ParticleProcessMaterial processMaterial = (ParticleProcessMaterial)BloodSplatter.ProcessMaterial;
    processMaterial.Direction = new(
      (float)_random.NextDouble(),
      (float)_random.NextDouble(),
      0
    );
    processMaterial.Color = color;
    BloodSplatter.Restart();
  }

  private void TakeDamage(double damage, bool isCrit)
  {
    damage *= DamageMultiBase;
    string text = damage > 0 ? $"-{Math.Round(damage, 2)}" : Math.Round(damage, 2).ToString();
    Color color = damage > 0 ? isCrit ? Colors.Yellow : Colors.Red : Colors.White;
    DisplayDamageNumber(text, color);

    EmitHitParticles(Colors.Red);

    if (IsInstanceValid(_hitFeedbackTween))
    {
      _hitFeedbackTween.Kill();
    }
    _hitFeedbackTween = CreateTween();
    Sprite.SelfModulate = Colors.Red;
    _hitFeedbackTween.TweenProperty(Sprite, "self_modulate", Colors.White, 0.3);

    SetHealth(Health - (float)damage);

    HitStop(damage);

    if (Health <= 0) Die();
  }

  private void HitStop(double damage)
  {
    TimeScale = 0;
    if (IsInstanceValid(_hitStopTween))
    {
      _hitStopTween.Kill();
    }
    _hitStopTween = CreateTween().SetIgnoreTimeScale();
    _hitStopTween.TweenProperty(this, "TimeScale", 1.0, HitStopDurationBase * damage / GetMaxHealth(Strength));
  }

  private void SetHealth(double value)
  {
    Health = value;
    StatusBar.Health = Health;
  }


  private void DisplayDamageNumber(string text, Color color)
  {
    Label damageNumberLabel = new()
    {
      Text = text,
      Modulate = color,
      Scale = new(2, 2),
      GlobalPosition = GlobalPosition,
      PivotOffset = new(0.5f, 0.5f),
      HorizontalAlignment = HorizontalAlignment.Center
    };
    Tween damageNumberTween = damageNumberLabel.CreateTween().SetParallel();

    GetTree().CurrentScene.AddChild(damageNumberLabel);
    damageNumberTween.TweenProperty(
      damageNumberLabel,
      "position:y",
      damageNumberLabel.Position.Y - 30, 0.7
    );
    damageNumberTween.TweenProperty(
      damageNumberLabel,
      "scale",
      new Vector2(0, 0),
      0.7
    );
    damageNumberTween.Chain().TweenCallback(
      Callable.From(() => damageNumberLabel.QueueFree())
    );
  }
  public void Heal(float value)
  {
    Health += value;
  }
  public override void _Process(double delta)
  {
    if (HasAbility)
    {
      StatusBar.AbilityBarValue =
            Ability.Active ?
            Ability.AbTimeRemainPercent :
            Ability.AbCooldownTimeRemainPercent;
    }
  }


  public override void _PhysicsProcess(double delta)
  {
    if (Dead)
    {
      Velocity = Vector2.Zero;
    }
    else
    {
      if (IsInstanceValid(Enemy))
      {
        float targetAngle = GetAngleTo(Enemy.GlobalPosition);
        Weapon?.Rotation = Mathf.LerpAngle(Weapon.Rotation, targetAngle, (Agility + 2) * (float)delta);
      }

      Velocity = Direction.Normalized() * (float)(MovementSpeed * delta);
    }

    KinematicCollision2D collision = CanMove ? MoveAndCollide(Velocity) : null;
    if (collision != null)
    {
      Direction = Direction.Bounce(collision.GetNormal());
      _audioManager.PlayRandomStoneImpact();
    }
    ;
  }

  public Vector2 GetRandomDirection()
  {
    Vector2 newDirection = new()
    {
      X = (float)_random.NextDouble(),
      Y = (float)_random.NextDouble()
    };
    return newDirection;
  }

  private Fighter GetEnemy()
  {
    return (Fighter)GetTree().GetFirstNodeInGroup($"Team {(team == Team.A ? 'B' : 'A')}");
  }
}

