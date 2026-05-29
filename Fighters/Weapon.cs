using Godot;
using System;
using System.Linq;

public partial class Weapon : Node2D
{
  [Export] public AnimationPlayer AttkAnimPlayer;
  [Export] public Area2D AttkTrigger;
  [Export] public Area2D HitBox;
  [Export] public float Weight = 1f;
  [Export] public float BaseDmg = 15f;
  [Export] public float CritMult = 1.5f;

  private double Damage
  {
    get => BaseDmg + (BaseDmg * _parentFighter.Strength * 0.15);
  }

  private bool _canAttack = true;

  private Fighter _parentFighter;

  private Random _random;

  private bool _isCrit;

  public override void _Ready()
  {
    _parentFighter = GetParent<Fighter>();

    AttkAnimPlayer.SpeedScale = (float)(0.5 + (_parentFighter.Agility * 0.1f / Weight));
    AttkAnimPlayer.AnimationFinished += OnAttkAnimTimeout;

    if (_parentFighter.team == Fighter.Team.A)
    {
      SetCollisions(Fighter.ColLayer.B);
    }
    else
    {
      SetCollisions(Fighter.ColLayer.A);
    }


    _random = new();
    _isCrit = IsCrit();

    HitBox.BodyEntered += OnAttackHit;
  }

  private void SetCollisions(Fighter.ColLayer layer)
  {
    AttkTrigger.SetCollisionMaskValue((int)layer, true);
    HitBox.SetCollisionMaskValue((int)layer, true);
  }

  public override void _ExitTree()
  {
    HitBox.BodyEntered -= OnAttackHit;
    AttkAnimPlayer.AnimationFinished -= OnAttkAnimTimeout;
  }

  private bool IsCrit()
  {
    return _random.NextDouble() <= _parentFighter.CritChance;
  }

  public void OnAttackHit(Node2D _)
  {
    foreach (Fighter fighter in HitBox.GetOverlappingBodies().OfType<Fighter>())
    {
      Fighter.HitStatus hitStatus = fighter.TakeDamage(_isCrit ? Damage * CritMult : Damage, _isCrit);
    }
  }


  public override void _PhysicsProcess(double delta)
  {
    if (_canAttack && AttkTrigger.Monitoring && AttkTrigger.HasOverlappingBodies())
    {
      foreach (Fighter fighter in AttkTrigger.GetOverlappingBodies().OfType<Fighter>())
      {
        if (!fighter.Dead) Attack();
      }
    }
  }


  private void OnAttkAnimTimeout(StringName _)
  {
    _isCrit = IsCrit();
    _canAttack = true;
  }

  private void Attack()
  {
    AttkAnimPlayer.Play("Attack");
    _canAttack = false;
  }
}
