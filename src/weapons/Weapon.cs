using Godot;
using System;
using System.Linq;
using TheLastColosseum.Fighters;
using TheLastColosseum.Utils;


[GlobalClass]
public partial class Weapon : Node2D
{
  public bool CanAttack {private set; get;} = true;
  [Export] public string WeaponName { set; get; } = "[Weapon Name Here]";
  [Export] public AnimationPlayer AtkAnimPlayer;
  [Export] public Area2D AtkTrigger;
  [Export] public Area2D HitBox;
  [Export] public WeaponStats Stats;
  [Export] public AudioStream HitSound;
  [Export] public AudioStream SwingSound;
  [Export] public AudioStream BlockSound;
  [Export] public float HitBoxLifeSpan = 0.2f;
  [Export] public Sprite2D[] WeaponSprites;
  [Export] public Sprite2D[] Hands;

  public float AtkSpeedBase {set;get;} = 0.15f;
  
  private AudioManager _audioManager;

  private double Damage
  {
    get => Stats.BaseDmg + (Stats.BaseDmg * Fighter.Strength * 0.15);
  }

  private bool _canAttack = true;

  private Fighter Fighter;

  private Random _random;

  private float AtkSpeed
  {
    get => 0.25f + (Fighter.Agility * AtkSpeedBase / Stats.Weight);
  }
  public double AtkCooldownPercent
  {
    get
    {
      return AtkAnimPlayer.IsPlaying() ? AtkAnimPlayer.CurrentAnimationPosition / AtkAnimPlayer.CurrentAnimationLength : 1.0;
    }
  }

  public Action WeaponSwingStart;

  [Signal]
  public delegate void WeaponSwingEndEventHandler();
  private bool _hasHitFighter = false;

  private AudioManager.Team _teamAudioPlayer;
  private bool _isCrit;

  public override void _Ready()
  {
    ToggleHitBox(false);

    Fighter = GetParent<Fighter>();
    InitSounds();

    foreach (Sprite2D hand in Hands)
    {
      hand.Texture = Fighter.HandSprite;
    }


    AtkAnimPlayer.SpeedScale = AtkSpeed;
    AtkAnimPlayer.AnimationFinished += OnAttkAnimTimeout;

    if (Fighter.team == Team.A)
    {
      SetCollisions(ColLayer.B);
    }
    else
    {
      SetCollisions(ColLayer.A);
    }


    _random = new();

    HitBox.BodyEntered += OnAttackHit;
  }
  public override void _ExitTree()
  {
    HitBox.BodyEntered -= OnAttackHit;
    AtkAnimPlayer.AnimationFinished -= OnAttkAnimTimeout;
  }


  public void Enable()
  {
    AtkTrigger.Monitoring = true;
    AtkAnimPlayer.Active = true;
    Modulate = Colors.White;
    CanAttack = true;
  }

  public void Disable(bool modulate)
  {
    AtkTrigger.Monitoring = false;
    ToggleHitBox(false);
    AtkAnimPlayer.Active = false;
    if(modulate) Modulate = Fighter.DeadColor;
    CanAttack = false;
  }

  private void InitSounds()
  {
    _audioManager = GetNode<AudioManager>("/root/AudioManager");
    _teamAudioPlayer = Fighter.team == Team.A ? _audioManager.TeamA : _audioManager.TeamB;
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


  private void ToggleHitBox(bool value)
  {

    HitBox.Monitoring = value;
    HitBox.Visible = value;
    if (value == false){
      _hasHitFighter = false;
    }
  }


  private bool IsCrit()
  {
    return _random.NextDouble() <= Fighter.CritChance;
  }

  public void OnAttackHit(Node2D _)
  {
    if (_hasHitFighter) return;
    HitStatus hitStatus = HitStatus.Miss;
    foreach (Fighter fighter in HitBox.GetOverlappingBodies().OfType<Fighter>())
    {
      hitStatus = fighter.HitRequest(_isCrit ? Damage * Stats.CritMult : Damage, _isCrit, _parentFighter.TrueStrike);

      HitStatus hitStatus = fighter.HitRequest(_isCrit ? Damage * Stats.CritMult : Damage, _isCrit, Fighter.TrueStrike);
      PrintHitInfo(Damage);
      PlaySound(hitStatus);
      _hasHitFighter = true;
    }
  }

  public void SetWeaponSpriteShaders(ShaderMaterial shaderMaterial)
  {
    foreach(Sprite2D sprite2D in WeaponSprites)
    {
      sprite2D.Material = shaderMaterial;
    }
    }

  private void PrintHitInfo(double damage)
  {
    Debug.PrintDebug($"Hit enemy with damage: {damage} crit: {Fighter.CritChance} truestrike: {Fighter.TrueStrike}", $"{Fighter.FighterName}:{WeaponName}");
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
    if (!CanAttack) return;

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
    EmitSignal(SignalName.WeaponSwingEnd);
    _canAttack = true;
    _hasHitFighter = false;
    AtkAnimPlayer.SpeedScale = AtkSpeed;
    Debug.PrintDebug("Weapon swing end", $"{Fighter.FighterName}:{WeaponName}");
  }

  private void Attack()
  {
    _isCrit = IsCrit();
    AtkAnimPlayer.Play("attack");
    _canAttack = false;
    WeaponSwingStart?.Invoke();
  }
}
