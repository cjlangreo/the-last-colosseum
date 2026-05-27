using Godot;
using System;
using System.Linq;

public partial class Weapon : Node2D
{
  [Export] public Node2D WeaponPivot;
  [Export] public AnimationPlayer AnimPlayer;
  [Export] public Area2D AttackTrigger;
  [Export] public Area2D HitBox;
  [Export] public Timer AttackTimer;
  [Export] public float AttackSpeed = 1f;
  private bool _canAttack = true;
  private bool _hasHitEnemy = false;
  private Fighter _wielder;

  private Fighter _parentFighter;

  private Random _random;
  public override void _Ready()
  {
    _wielder = GetParent<Fighter>();
    AttackTimer.Timeout += OnAttackTimerTimeout;
    _parentFighter = GetParent<Fighter>();

    _random = new();

    HitBox.BodyEntered += OnAttackHit;
  }

  public override void _ExitTree()
  {
    AttackTimer.Timeout -= OnAttackTimerTimeout;
    HitBox.BodyEntered -= OnAttackHit;
  }


  public void OnAttackHit(Node2D _)
  {
    foreach (Fighter fighter in HitBox.GetOverlappingBodies().OfType<Fighter>())
    {
      if (fighter is Fighter enemy && fighter != _wielder)
      {
        bool isCrit = _random.NextDouble() <= _parentFighter.CritChance;
        GD.Print(isCrit ? "Crit" : "Not crit");
        _hasHitEnemy = true;
        enemy.TakeDamage(isCrit ? _parentFighter.Damage * _parentFighter.CritMult : _parentFighter.Damage, isCrit);
      }
    }
  }


  public override void _PhysicsProcess(double delta)
  {
    if (_canAttack && AttackTrigger.Monitoring && AttackTrigger.HasOverlappingBodies())
    {
      foreach(Fighter fighter in AttackTrigger.GetOverlappingBodies().OfType<Fighter>())
      {
        if (fighter != _parentFighter && !fighter.Dead)
        {
          OnDangerAreaDetect();
        }
      }
    }
    // if (HitBox.Monitoring && HitBox.HasOverlappingBodies() && !_hasHitEnemy) OnAttackHit();
  }

  private void OnAttackTimerTimeout()
  {
    _canAttack = true;
    _hasHitEnemy = false;
  }

  private void OnDangerAreaDetect()
  {
    if (_canAttack) Attack();
  }

  private void Attack()
  {
    AnimPlayer.Play("Attack");
    _canAttack = false;
    AttackTimer.Start(AttackSpeed);
  }
}
