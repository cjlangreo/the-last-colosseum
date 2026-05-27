using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export] public float Health { private set
    {
      field = value;
      if (field <= 0)
      {
        Die();
      }
    } get; } = 100;

  [Export] public HealthBar healthBar;
  [Export] public float InvincibilityTime {get;set;} = 0.05f;
  [Export] public Timer InvincibilityTimer;
  [Export] public Sprite2D Sprite;
  public bool Dead = false;

  private bool _isInvincible = false;
  

  private Tween _hitFeedbackTween; 

  public override void _Ready()
  {
    InvincibilityTimer.WaitTime = InvincibilityTime;
    InvincibilityTimer.Timeout += OnInvincibilityTimerTimeout;

    healthBar.MaxValue = Health;
    GD.Print($"Current health: {Health}");
  }

  public virtual void Die()
  {
    Dead = true;
  }

  public override void _ExitTree()
  {
    InvincibilityTimer.Timeout -= OnInvincibilityTimerTimeout;
  }

  private void OnInvincibilityTimerTimeout()
  {
    _isInvincible = false;
  }
  public virtual bool TakeDamage(double damage, bool isCrit)
  {
    if(_isInvincible) return false;
    _isInvincible = true;
    InvincibilityTimer.Start();

    _hitFeedbackTween?.Kill();
    _hitFeedbackTween = CreateTween();
    Sprite.SelfModulate = Colors.Red;
    _hitFeedbackTween.TweenProperty(Sprite, "self_modulate", Colors.White, 0.3);

    Tween damageNumberTween = CreateTween().SetParallel();
    Label damageNumberLabel = new()
    {
      Text = damage > 0 ? $"-{damage}" : damage.ToString(),
      Modulate = damage > 0 ? isCrit ? Colors.Yellow : Colors.Red : Colors.White,
      Scale = new(2,2)
    };
    AddChild(damageNumberLabel);
    damageNumberTween.TweenProperty(
      damageNumberLabel,
      "position:y",
      damageNumberLabel.Position.Y - 30, 0.7
    );
    damageNumberTween.TweenProperty(
      damageNumberLabel,
      "scale",
      new Vector2(0,0),
      0.7
    );
    damageNumberTween.Chain().TweenCallback(
      Callable.From(() => damageNumberLabel.QueueFree())
    );
    
    Health -= (float)damage;



    return true;
  }


  public void Heal(float value)
  {
    Health += value;
  }

  public override void _Process(double delta)
  {
    healthBar.Value = Mathf.Lerp(healthBar.Value, Health, delta * 5);
  }
}
