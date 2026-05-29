using Godot;
using System;
using System.Linq;

public partial class Weapon : Node2D
{
  [Export] public bool Enabled = true;
  [Export] public AnimationPlayer AtkAnimPlayer;
  [Export] public Area2D AtkTrigger;
  [Export] public Area2D HitBox;
  [Export] public float Weight = 1f;
  [Export] public float BaseDmg = 15f;
  [Export] public float CritMult = 1.5f;
  [Export] public AudioStream HitSound;
  [Export] public AudioStream BlockSound;
  private AudioManager _audioManager;

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
    InitSounds();

    AtkAnimPlayer.SpeedScale = (float)(0.5 + (_parentFighter.Agility * 0.1f / Weight));
    AtkAnimPlayer.AnimationFinished += OnAttkAnimTimeout;

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

  public void Disable()
  {
    AtkTrigger.Monitoring = false;
    HitBox.Monitoring = false;
    AtkAnimPlayer.Active = false;
    Modulate = _parentFighter.DeadColor;
  }

  private void InitSounds()
  {
    _audioManager = GetNode<AudioManager>("/root/AudioManager");
    if (_parentFighter.team == Fighter.Team.A)
    {
      _audioManager.TeamA.HitSoundPlayer.Stream = HitSound;
      _audioManager.TeamA.BlockSoundPlayer.Stream = BlockSound;
    }
    else
    {
      _audioManager.TeamB.HitSoundPlayer.Stream = HitSound;
      _audioManager.TeamB.BlockSoundPlayer.Stream = BlockSound;
    }
  }


  private void SetCollisions(Fighter.ColLayer layer)
  {
    AtkTrigger.SetCollisionMaskValue((int)layer, true);
    HitBox.SetCollisionMaskValue((int)layer, true);
  }

  public override void _ExitTree()
  {
    HitBox.BodyEntered -= OnAttackHit;
    AtkAnimPlayer.AnimationFinished -= OnAttkAnimTimeout;
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
      PlaySound(hitStatus, _parentFighter.team);
    }
  }

  private void PlaySound(Fighter.HitStatus hitStatus, Fighter.Team team)
  {
    if (team == Fighter.Team.A)
    {
      if (hitStatus == Fighter.HitStatus.Hit)
      {
        _audioManager.TeamA.HitSoundPlayer.Play();
      }
      else if (hitStatus == Fighter.HitStatus.Block)
      {
        _audioManager.TeamA.BlockSoundPlayer.Play();
      }
    }
    else
    {
      if (hitStatus == Fighter.HitStatus.Hit)
      {
        _audioManager.TeamB.HitSoundPlayer.Play();
      }
      else if (hitStatus == Fighter.HitStatus.Block)
      {
        _audioManager.TeamB.BlockSoundPlayer.Play();
      }
    }
  }


  public override void _PhysicsProcess(double delta)
  {
    if (!Enabled) return;

    if (_canAttack && AtkTrigger.Monitoring && AtkTrigger.HasOverlappingBodies())
    {
      foreach (Fighter fighter in AtkTrigger.GetOverlappingBodies().OfType<Fighter>())
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
    AtkAnimPlayer.Play("attack");
    _canAttack = false;
  }
}
