using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using TheLastColosseum.Fighters;
using TheLastColosseum.Utils;


namespace TheLastColosseum.Weapons;

public enum WeaponEnum
{
  IronSword,
  Greataxe,
  DoubleSai,
  Katana,
  Khopesh
}

[GlobalClass]
public partial class Weapon : Node2D
{
  public bool CanAttack { private set; get; } = true;
  public string WeaponName => WeaponNames[WeaponEnum];
  [Export] public WeaponEnum WeaponEnum;
  [Export] public AnimationPlayer AtkAnimPlayer;
  public AttackTrigger AtkTrigger;
  public Hitbox Hitbox;
  [Export] public WeaponStats Stats;
  
  [ExportGroup("Sprites")]
  [Export] public Sprite2D[] WeaponSprites;
  [Export] public Sprite2D[] Hands;
  
  [ExportGroup("Sounds")]
  [Export] public AudioStream HitSound;
  [Export] public AudioStream SwingSound;
  [Export] public AudioStream BlockSound;

  [ExportGroup("Weapon Trail")]
  [Export] public bool HasWeaponTrails = true;
  [Export] public Array<Marker2D> WeaponTrailMarkers;
  [Export] public float WeaponTrailWidth = 32f;
  [Export] public Color WeaponTrailColor = Colors.White;
  public static System.Collections.Generic.Dictionary<WeaponEnum, string> WeaponNames = new()
  {
    {WeaponEnum.DoubleSai, "Double Sai"},
    {WeaponEnum.IronSword, "Iron Sword"},
    {WeaponEnum.Greataxe, "Greataxe"},
    {WeaponEnum.Katana, "Katana"},
    {WeaponEnum.Khopesh, "Khopesh"},
  };

  private const string WeaponTrailUID = "uid://7bsxdq00fhpx";
  private WeaponTrail _currentTrail;


  public float AtkSpeedBase { set; get; } = 0.15f;

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

  private AudioManager.Team _teamAudioPlayer;
  private bool _isCrit;


  public override void _Ready()
  {
    GetNodeRefs();
    Hitbox.Team = Fighter.team;
    AtkTrigger.team = Fighter.team;
    Hitbox.InitCollissions();
    AtkTrigger.InitCollissions();


    ToggleHitBox(false);

    InitSounds();


    SetWeaponHandSprites(Fighter.FighterStats.FighterHands);
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

    Hitbox.Hit += OnAttackHit;
  }

  
  private void GetNodeRefs()
  {
    Fighter = GetParent<Fighter>();
    Hitbox = GetNode<Hitbox>("%Hitbox");
    AtkTrigger = GetNode<AttackTrigger>("%AttackTrigger");
  }

  public void SetWeaponHandSprites(Texture2D texture)
  {
    GD.Print("Setting Weapon Hand Sprites");
    foreach (Sprite2D hand in Hands)
    {
      hand.Texture = texture;
    }
  }

  public override void _ExitTree()
  {
    Hitbox.Hit -= OnAttackHit;
    AtkAnimPlayer.AnimationFinished -= OnSwingEnd;
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
    if (modulate) Modulate = Fighter.DeadColor;
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


  private void ToggleHitBox(bool value, int index = 0)
  {
    Hitbox.ToggleListen(value);
    if (value == false)
    {
      if(IsInstanceValid(_currentTrail)) _currentTrail.Remove();
    }
    else
    {
      if(HasWeaponTrails) _currentTrail = AddWeaponTrail(index);
    }
  }


  private bool IsCrit()
  {
    return _random.NextDouble() <= Fighter.CritChance;
  }

  public void OnAttackHit(Fighter fighter)
  {
      HitStatus hitStatus = fighter.HitRequest(_isCrit ? Damage * Stats.CritMult : Damage, _isCrit, Fighter.TrueStrike);
      PrintHitInfo(Damage);
      PlayHitSound(hitStatus);
  }

  public void SetWeaponSpriteShaders(ShaderMaterial shaderMaterial)
  {
    foreach (Sprite2D sprite2D in WeaponSprites)
    {
      sprite2D.Material = shaderMaterial;
    }
  }

  private void PrintHitInfo(double damage)
  {
    Debug.PrintDebug($"Hit enemy with damage: {damage} crit: {Fighter.CritChance} truestrike: {Fighter.TrueStrike}", $"{Fighter.FighterName}:{WeaponName}");
  }

  private  void PlayHitSound(HitStatus hitStatus)
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

    if (_canAttack && AtkTrigger.EnemyDetected)
    {
        Swing();
    }
  }


  private void OnSwingEnd(StringName _)
  {
    EmitSignal(SignalName.WeaponSwingEnd);
    _canAttack = true;
    Debug.PrintDebug("Weapon swing end", $"{Fighter.FighterName}:{WeaponName}");
  }

  private void Swing()
  {
    AtkAnimPlayer.SpeedScale = AtkSpeed;
    _isCrit = IsCrit();
    AtkAnimPlayer.Play("attack");
    _canAttack = false;
    WeaponSwingStart?.Invoke();
  }

  private WeaponTrail AddWeaponTrail(int markerIndex)
  {
    WeaponTrail weaponTrail = GD.Load<PackedScene>(WeaponTrailUID).Instantiate<WeaponTrail>();
    weaponTrail.Marker = WeaponTrailMarkers[markerIndex];
    weaponTrail.Width = WeaponTrailWidth;
    weaponTrail.TrailLifetime = AtkSpeed;

    Color transparentTrailColor = new(WeaponTrailColor, 0.0f);
    Gradient gradient = weaponTrail.Gradient;
    gradient.SetColor(0, transparentTrailColor);
    gradient.SetColor(1, WeaponTrailColor);
    gradient.SetColor(2, transparentTrailColor);
    AddChild(weaponTrail);
    return weaponTrail;
  }
}
