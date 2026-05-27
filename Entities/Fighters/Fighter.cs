using Godot;
using System;

public partial class Fighter : Entity, ICanDie
{
  [Export] public double Speed { get; set; } = 50;
  [Export] public double Damage { get; set; } = 10;
  [Export] public Weapon Weapon;
  [Export] public Team team = Team.A;
  [Export] public GpuParticles2D BloodParticles;
  [Export] public AudioStream HitSound;
  [Export] public float CritChance { set; get; } = 0.10f;
  [Export] public float CritMult { set; get; } = 2;
  [Export] public Sprite2D DeathSprite;
  [Export] public AnimationPlayer animationPlayer;
  [Export] public Area2D AttackTrigger;

  private Tween _modulateTween;
  private AudioManager _audioManager;
  protected AudioStreamPlayer _audioStreamPlayer;

  public enum Team
  {
    A,
    B
  }

  private Fighter _enemy;
  public Vector2 Direction { get; set; }
  public override void _Ready()
  {
    base._Ready();
    _audioManager = GetNode<AudioManager>("/root/AudioManager");
    _audioStreamPlayer = team == Team.A ? _audioManager.StreamPlayer1 : _audioManager.StreamPlayer2;
    _audioStreamPlayer.Stream = HitSound;
    Direction = GetRandomDirection();
  }
  public override void Die()
  {
    base.Die();
    ZIndex = -1;
    AttackTrigger.Monitoring = false;
    Weapon.Modulate = new(.3f, .3f, .3f);
    Weapon.HitBox.Monitoring = false;
    Sprite.Modulate = new(.3f, .3f, .3f);
    animationPlayer.Active = false;
    SetCollisionLayerValue(1, false);

    DeathSprite.Scale = new(0,0);
    DeathSprite.Show();
    Tween deathSpriteTween = CreateTween().SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
    deathSpriteTween.TweenProperty(DeathSprite, "scale", new Vector2(1,1), 1);
  }

  public override bool TakeDamage(double damage, bool isCrit)
  {
    BloodParticles.Restart();
    _audioStreamPlayer.Play();
    return base.TakeDamage(damage, isCrit);
  }

  private Fighter GetEnemy()
  {
    return (Fighter)GetTree().GetFirstNodeInGroup($"Team {(team == Team.A ? 'B' : 'A')}");
  }

  public override void _Process(double delta)
  {
    base._Process(delta);
    _enemy ??= GetEnemy();
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);
    if (Dead) return;

    if (IsInstanceValid(_enemy))
    {
      float targetAngle = GetAngleTo(_enemy.GlobalPosition);
      Weapon.Rotation = Mathf.LerpAngle(Weapon.Rotation, targetAngle, 10f * (float)delta);
    }



    KinematicCollision2D collision = MoveAndCollide(Direction.Normalized() * (float)(Speed * delta));
    if (collision != null)
    {
      Direction = Direction.Bounce(collision.GetNormal());
    }

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
}
