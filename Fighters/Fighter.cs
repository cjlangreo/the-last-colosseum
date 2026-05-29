using Godot;
using System;

public partial class Fighter : CharacterBody2D, ICanDie
{
  [Export] public int Strength { get; set; } = 3;
  [Export] public int Agility { get; set; } = 3;
  [Export] public int Intelligence { get; set; } = 2;
  protected double MovementSpeed
  {
    get => 40 + Agility * 40;
  }
  protected float Evasion
  {
    get => Agility * 0.1f;
  }
  public double Health { get; set; }
  public double MaxHealth
  {
    get => Strength * 30 + 50;
  }
  public float CritChance
  {
    get => Intelligence * 0.18f;
  }

  [ExportGroup("Refs", "r_")]
  [Export] public HealthBar r_HealthBar;
  [Export] public Sprite2D r_Sprite;
  [Export] public Weapon r_Weapon;
  [Export] public GpuParticles2D r_BloodSplatter;
  protected Sprite2D _deathSprite;
  [Export] public AnimationPlayer r_WeaponAnimPlayer;
  [Export] public Area2D AttackTrigger;
  [Export] public Team team = Team.A;
  private Tween _hitFeedbackTween;
  public Fighter Enemy;
  protected Vector2 Direction { get; set; }
  private Tween _modulateTween;
  private Random _random;
  public bool Dead = false;
  public Color DeadColor = new(0.3f,0.3f,0.3f);
  public enum Team
  {
    A,
    B
  }

  public enum ColLayer
  {
    A = 31,
    B = 32
  }

  public enum HitStatus
  {
    Hit,
    Block,
    Evade
  }


  public override void _Ready()
  {
    if ((Agility + Strength + Intelligence) != 8)
    {
      GD.PrintErr("Stats does not add up to 8!");
    }

    _random = new();

    Direction = GetRandomDirection();

    SetTeamGroup();

    SetCollisionLayerValue((int)(team == Team.A ? ColLayer.A : ColLayer.B), true);
    SetCollisionMaskValue((int)(team == Team.A ? ColLayer.B : ColLayer.A), true);

    Health = MaxHealth;
    r_HealthBar.Health = MaxHealth;
    r_HealthBar.SetMaxValue(MaxHealth);

    _deathSprite = new()
    {
      Texture = (CompressedTexture2D)GD.Load("uid://bltixp8bd61yr"),
      Visible = false
    };
    AddChild(_deathSprite);

  }

  private void SetTeamGroup()
  {
    AddToGroup(team == Team.A ? "Team A" : "Team B");
  }

  public virtual void Die()
  {
    Dead = true;
    r_Weapon.Disable();
    r_Sprite.Modulate = DeadColor;

    SetCollisionLayerValue((int)(team == Team.A ? ColLayer.A : ColLayer.B), false);


    _deathSprite.Scale = new(0, 0);
    _deathSprite.Show();
    Tween deathSpriteTween = CreateTween().SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
    deathSpriteTween.TweenProperty(_deathSprite, "scale", new Vector2(1, 1), 1);

    // EventManager.RoundEnd.Invoke(team);
  }

  public virtual HitStatus TakeDamage(double damage, bool isCrit)
  {
    bool evade = _random.NextDouble() <= Evasion;
    if (evade)
    {
      DisplayDamageNumber("evade", Colors.Gray);
      return HitStatus.Evade;
    }
    else
    {
      string text = damage > 0 ? $"-{Math.Round(damage, 2)}" : Math.Round(damage, 2).ToString();
      Color color = damage > 0 ? isCrit ? Colors.Yellow : Colors.Red : Colors.White;
      DisplayDamageNumber(text, color);

      ((ParticleProcessMaterial)r_BloodSplatter.ProcessMaterial).Direction = new(
        (float)_random.NextDouble(),
        (float)_random.NextDouble(),
        0
      );
      r_BloodSplatter.Restart();

      _hitFeedbackTween?.Kill();
      _hitFeedbackTween = CreateTween();
      r_Sprite.SelfModulate = Colors.Red;
      _hitFeedbackTween.TweenProperty(r_Sprite, "self_modulate", Colors.White, 0.3);
      Health -= (float)damage;
      r_HealthBar.Health = Health;
      if (Health <= 0) Die();
      return HitStatus.Hit;
    };
  }

  private void DisplayDamageNumber(string text, Color color)
  {
    Tween damageNumberTween = CreateTween().SetParallel();
    Label damageNumberLabel = new()
    {
      Text = text,
      Modulate = color,
      Scale = new(2, 2),
      GlobalPosition = GlobalPosition,
      PivotOffset = new(0.5f, 0.5f),
      HorizontalAlignment = HorizontalAlignment.Center
    };

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
    Enemy ??= GetEnemy();
  }

  public override void _PhysicsProcess(double delta)
  {
    if (Dead) return;

    if (IsInstanceValid(Enemy))
    {
      float targetAngle = GetAngleTo(Enemy.GlobalPosition);
      r_Weapon.Rotation = Mathf.LerpAngle(r_Weapon.Rotation, targetAngle, (Agility + 2) * (float)delta);
    }

    KinematicCollision2D collision = MoveAndCollide(Direction.Normalized() * (float)(MovementSpeed * delta));
    if (collision != null)
    {
      Direction = Direction.Bounce(collision.GetNormal());
    }
    ;
  }

  public Vector2 GetRandomDirection()
  {
    Random random = new();
    Vector2 newDirection = new()
    {
      X = (float)random.NextDouble(),
      Y = (float)random.NextDouble()
    };
    return newDirection;
  }

  private Fighter GetEnemy()
  {
    return (Fighter)GetTree().GetFirstNodeInGroup($"Team {(team == Team.A ? 'B' : 'A')}");
  }
}

