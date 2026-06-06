using Godot;
using System;
using System.Linq;
using RandomBattles.Fighters;


public partial class Weapon : Node2D
{
  [Export] public bool Enabled = true;
  [Export] public string WeaponName { set;get;} = "[Weapon Name Here]";
  [Export] public AnimationPlayer AtkAnimPlayer;
  [Export] public Area2D AtkTrigger;
  [Export] public Area2D HitBox;
  [Export] public WeaponStats Stats;
  [Export] public AudioStream HitSound;
  [Export] public AudioStream SwingSound;
  [Export] public AudioStream BlockSound;
  [Export] public float HitBoxLifeSpan = 0.2f;
  [Export] public Sprite2D MainSprite;
  [Export] private Sprite2D[] Hands;
  private AudioManager _audioManager;

  private double Damage
  {
    get => Stats.BaseDmg + (Stats.BaseDmg * _parentFighter.Strength * 0.15);
  }

  private bool _canAttack = true;

  private Fighter _parentFighter;

  private Random _random;

  private bool _isCrit;
  private float AtkSpeed
  {
    get => 0.25f + (_parentFighter.Agility * 0.15f / Stats.Weight);
  }
  public double AtkCooldownPercent
  {
    get
    {
      return AtkAnimPlayer.IsPlaying() ? AtkAnimPlayer.CurrentAnimationPosition / AtkAnimPlayer.CurrentAnimationLength : 1.0;
    }
  }


  private bool _hasHitFighter = false;

  private AudioManager.Team _teamAudioPlayer;

  public override void _Ready()
  {
    _parentFighter = GetParent<Fighter>();
    InitSounds();

    foreach (Sprite2D hand in Hands)
    {
      hand.Texture = _parentFighter.HandSprite;
    }


    AtkAnimPlayer.SpeedScale = AtkSpeed;
    AtkAnimPlayer.AnimationFinished += OnAttkAnimTimeout;

    if (_parentFighter.team == Team.A)
    {
      SetCollisions(ColLayer.B);
    }
    else
    {
      SetCollisions(ColLayer.A);
    }


    _random = new();
    _isCrit = IsCrit();

    HitBox.BodyEntered += OnAttackHit;
  }
  public override void _ExitTree()
  {
    HitBox.BodyEntered -= OnAttackHit;
    AtkAnimPlayer.AnimationFinished -= OnAttkAnimTimeout;
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
    _teamAudioPlayer = _parentFighter.team == Team.A ? _audioManager.TeamA : _audioManager.TeamB;
    _teamAudioPlayer.HitSoundPlayer.Stream = HitSound;
    _teamAudioPlayer.BlockSoundPlayer.Stream = BlockSound;
    _teamAudioPlayer.SwingSoundPlayer.Stream = SwingSound;
  }


  private void PlaySwingSound()
  {
    _teamAudioPlayer.SwingSoundPlayer.Play();
  }


  private void SetCollisions(ColLayer layer)
  {
    AtkTrigger.SetCollisionMaskValue((int)layer, true);
    HitBox.SetCollisionMaskValue((int)layer, true);
  }


  private async void ToggleHitBox(bool value)
  {

    HitBox.Monitoring = value;
    HitBox.Visible = value;
    if (value)
    {
      await ToSignal(GetTree().CreateTimer(HitBoxLifeSpan), Timer.SignalName.Timeout);
      HitBox.Monitoring = false;
      HitBox.Visible = false;
    }
    else
    {
      _hasHitFighter = false;
    }
  }


  private bool IsCrit()
  {
    return _random.NextDouble() <= _parentFighter.CritChance;
  }

  public void OnAttackHit(Node2D _)
  {
    if (_hasHitFighter) return;
    foreach (Fighter fighter in HitBox.GetOverlappingBodies().OfType<Fighter>())
    {
      HitStatus hitStatus = fighter.HitRequest(_isCrit ? Damage * Stats.CritMult : Damage, _isCrit, _parentFighter.TrueStrike);

      PlaySound(hitStatus);
      _hasHitFighter = true;
    }
  }

  private void PlaySound(HitStatus hitStatus)
  {
    if (hitStatus == HitStatus.Hit)
    {
      _teamAudioPlayer.HitSoundPlayer.Play();
    }
    else if (hitStatus == HitStatus.Block)
    {
      _teamAudioPlayer.BlockSoundPlayer.Play();
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
    _hasHitFighter = false;
    AtkAnimPlayer.SpeedScale = AtkSpeed;
  }

  private void Attack()
  {
    AtkAnimPlayer.Play("attack");
    _canAttack = false;
  }
}
