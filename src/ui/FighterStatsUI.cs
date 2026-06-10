using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TheLastColosseum.Fighters;
using TheLastColosseum.Abilities;

public partial class FighterStatsUI : VBoxContainer
{
  [Export] public bool Active = true;
  [Export] public bool RandomFighter = false;
  [Export] public int Spins = 10;
  [Export] public float SpinDuration = 3;
  [Export] public Team team = Team.A;
  [Export] public TextureRect FighterIcon;
  [Export] public TextureRect WeaponIcon;
  [Export] public TextureRect AbilityIcon;

  [Export] public CompressedTexture2D[] FighterIconCollection;
  [Export] public CompressedTexture2D[] WeaponIconCollection;
  [Export] public CompressedTexture2D[] AbilityIconCollection;

  [Export] public PanelContainer AbilityFrameContainer;
  [Export] public PanelContainer WeaponFrameContainer;
  [Export] public StyleBoxTexture BlueFrameStylebox;
  [Export] public StyleBoxTexture RedFrameStylebox;


  [Export] public Label FighterNameLabel;
  [Export] public Label AbilityNameLabel;
  [Export] public Label WeaponNameLabel;

  [Export] public HBoxContainer StrStarsContainer;
  [Export] public HBoxContainer AgiStarsContainer;
  [Export] public HBoxContainer IntStarsContainer;
  [Export] public Label StrOverflowLabel;
  [Export] public Label AgiOverflowLabel;
  [Export] public Label IntOverflowLabel;

  public Action SpinDone;


  private const string StrStarUID = "uid://cfbx3w2613wb4";
  private const string AgiStarUID = "uid://bbobvq7wlbojg";
  private const string IntStarUID = "uid://byeqgasc7idte";
  private const string BlankStarUID = "uid://b8wmeiivaqqpy";


  private CompressedTexture2D _strStarTexture = GD.Load<CompressedTexture2D>(StrStarUID);
  private CompressedTexture2D _agiStarTexture = GD.Load<CompressedTexture2D>(AgiStarUID);
  private CompressedTexture2D _intStarTexture = GD.Load<CompressedTexture2D>(IntStarUID);
  private CompressedTexture2D _blankStarTexture = GD.Load<CompressedTexture2D>(BlankStarUID);


  private Fighter _fighter;
  private Ability _ability;
  private Weapon _weapon;
  private TextureRect[] _strStars;
  private TextureRect[] _agiStars;
  private TextureRect[] _intStars;


  private ShaderMaterial AbilityShaderMat => (ShaderMaterial)AbilityIcon.GetParent<PanelContainer>().Material;
  private ShaderMaterial WeaponShaderMat => (ShaderMaterial)WeaponIcon.GetParent<PanelContainer>().Material;
  private double AbilityShaderValue { set => AbilityShaderMat.SetShaderParameter("percent", value); }
  private bool AbilityShaderActive { set => AbilityShaderMat.SetShaderParameter("ability_active", value); }
  private double WeaponShaderValue { set => WeaponShaderMat.SetShaderParameter("percent", value); }

  private Random _random = new();


  private Tween _rotateTween;
  private readonly float RotateRads = Mathf.DegToRad(1);
  private const float RotateTweenDuration = 2f;

  private enum Slot
  {
    Fighter,
    Weapon,
    Ability
  }
  private bool _fighterSpinDone = false;
  private bool _weaponSpinDone = false;
  private bool _abilitySpinDone = false;

  public override void _Ready()
  {
    RotateTweenInit();

    _fighter = team == Team.A ? EventBus.FighterA : EventBus.FighterB;
    _fighter.StatUpdated += OnStatUpdated;

    _weapon = _fighter.Weapon;
    _ability = _fighter.Ability;

    _strStars = StrStarsContainer.GetChildren().OfType<TextureRect>().ToArray();
    _agiStars = AgiStarsContainer.GetChildren().OfType<TextureRect>().ToArray();
    _intStars = IntStarsContainer.GetChildren().OfType<TextureRect>().ToArray();
    SetStatStars(Stat.Strength, _fighter.Strength);
    SetStatStars(Stat.Agility, _fighter.Agility);
    SetStatStars(Stat.Intelligence, _fighter.Intelligence);

    AbilityFrameContainer.Visible = _ability != null;


    AbilityFrameContainer.AddThemeStyleboxOverride("panel", team == Team.A ? RedFrameStylebox : BlueFrameStylebox);
    WeaponFrameContainer.AddThemeStyleboxOverride("panel", team == Team.A ? RedFrameStylebox : BlueFrameStylebox);

    FighterIcon.Texture = _fighter.r_Sprite.Texture;
    WeaponIcon.Texture = _weapon.WeaponSprites[0].Texture;
    AbilityIcon.Texture = _ability?.AbilityIcon;

    FighterNameLabel.Text = _fighter.FighterName;
    WeaponNameLabel.Text = _weapon.WeaponName;
    AbilityNameLabel.Text = _ability?.AbilityName;

    if (RandomFighter)
    {
      SpinSlot(Spins, Slot.Fighter);
      SpinSlot(Spins, Slot.Weapon);
      SpinSlot(Spins, Slot.Ability);
    }
  }

  public override void _ExitTree()
  {
    _fighter.StatUpdated -= OnStatUpdated;
  }

  private void RotateTweenInit()
  {
    OffsetTransformRotation = (float)GD.RandRange(-RotateRads, RotateRads);
    _rotateTween = CreateTween().SetLoops().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
    _rotateTween.TweenProperty(this, "offset_transform_rotation", RotateRads, RotateTweenDuration);
    _rotateTween.TweenProperty(this, "offset_transform_rotation", -RotateRads, RotateTweenDuration);
  }

  private void OnStatUpdated(Stat stat, int value)
  {
    SetStatStars(stat, value);
  }

  private void SetStatStars(Stat stat, int value)
  {
    TextureRect[] stars = null;
    CompressedTexture2D starTexture = null;
    Label overflowLabel = null;
    switch (stat)
    {
      case Stat.Strength:
        stars = _strStars;
        starTexture = _strStarTexture;
        overflowLabel = StrOverflowLabel;
        break;
      case Stat.Agility:
        stars = _agiStars;
        starTexture = _agiStarTexture;
        overflowLabel = AgiOverflowLabel;
        break;
      case Stat.Intelligence:
        stars = _intStars;
        starTexture = _intStarTexture;
        overflowLabel = IntOverflowLabel;
        break;
    }


    foreach (TextureRect star in stars)
    {
      star.Texture = _blankStarTexture;
    }

    if (value > Fighter.MaxAbilityPoints)
    {
      overflowLabel.Text = $"+{value - Fighter.MaxAbilityPoints}";
      value = Fighter.MaxAbilityPoints;
    }
    else
    {
      overflowLabel.Text = "";
    }

    for (int i = 0; i < value; i++)
    {
      stars[i].Texture = starTexture;
    }

  }


  private async void SpinSlot(int spins, Slot spinIcon)
  {
    List<CompressedTexture2D> shuffledList = [];
    int currentIconIndex = 0;
    TextureRect textureRect = null;
    switch (spinIcon)
    {
      case Slot.Fighter:
        shuffledList = [.. FighterIconCollection];
        currentIconIndex = shuffledList.FindIndex(icon => icon == _fighter.r_Sprite.Texture);
        textureRect = FighterIcon;
        break;
      case Slot.Ability:
        if(!_fighter.HasAbility) return;
        shuffledList = [.. AbilityIconCollection];
        currentIconIndex = shuffledList.FindIndex(icon => icon == _ability.AbilityIcon);
        textureRect = AbilityIcon;
        break;
      case Slot.Weapon:
        shuffledList = [.. WeaponIconCollection];
        currentIconIndex = shuffledList.FindIndex(icon => icon == _weapon.WeaponSprites[0].Texture);
        textureRect = WeaponIcon;
        break;
    }
    CompressedTexture2D temp = shuffledList.ElementAt(currentIconIndex);
    shuffledList.RemoveAt(currentIconIndex);
    shuffledList = [.. shuffledList.Shuffle().Prepend(temp)];

    int totalSpins = spins * shuffledList.Count;
    // Tween spinTween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
    Tween spinTween = CreateTween();
    spinTween.TweenMethod(Callable.From((int index) => ChangeSlot(textureRect, shuffledList[index % shuffledList.Count], spinIcon)), 0, totalSpins, SpinDuration);
    await ToSignal(spinTween, Tween.SignalName.Finished);
    OnSpinDone(spinIcon);
  }

  private void ChangeSlot(TextureRect textureRect, CompressedTexture2D texture, Slot spinIcon)
  {
    textureRect.Texture = texture;
    switch (spinIcon)
    {
      case Slot.Fighter:
        FighterNameLabel.Text = RandomString(GD.RandRange(3, 10));
        SetStatStars(Stat.Strength, _random.Next(1, Fighter.MaxAbilityPoints));
        SetStatStars(Stat.Agility, _random.Next(1, Fighter.MaxAbilityPoints));
        SetStatStars(Stat.Intelligence, _random.Next(1, Fighter.MaxAbilityPoints));
        break;
      case Slot.Weapon:
        WeaponNameLabel.Text = RandomString(GD.RandRange(3, 10));
        break;
      case Slot.Ability:
        AbilityNameLabel.Text = RandomString(GD.RandRange(3, 10));
        break;
    }

  }

  private void OnSpinDone(Slot spinIcon)
  {
    switch (spinIcon)
    {
      case Slot.Fighter:
        _fighterSpinDone = true;
        FighterNameLabel.Text = _fighter.FighterName;
        SetStatStars(Stat.Strength, _fighter.Strength);
        SetStatStars(Stat.Agility, _fighter.Agility);
        SetStatStars(Stat.Intelligence, _fighter.Intelligence);
        break;
      case Slot.Ability:
        _abilitySpinDone = true;
        AbilityNameLabel.Text = _ability.AbilityName;
        break;
      case Slot.Weapon:
        _weaponSpinDone = true;
        WeaponNameLabel.Text = _weapon.WeaponName;
        break;
    }
    if (_fighterSpinDone && (_fighter.HasAbility ? _abilitySpinDone : true) && _weaponSpinDone)
    {
      SpinDone?.Invoke();
    }
  }

  private double EaseOutExpo(double number)
  {
    return 1 - Math.Pow(1 - number, 5);
  }

  public override void _Process(double delta)
  {
    if (!Active)
    {
      WeaponShaderValue = 1.0;
      AbilityShaderValue = 1.0;
      AbilityShaderActive = false;
      return;
    }

    if (_fighter.Dead) return;
    WeaponShaderValue = _weapon.AtkCooldownPercent;

    if (!_fighter.HasAbility) return;
    if (_ability.Active)
    {
      AbilityShaderValue = _ability.AbTimeRemainPercent;
    }
    else
    {
      AbilityShaderValue = _ability.AbCooldownTimeRemainPercent;
    }
    AbilityShaderActive = _ability.Active;
  }

  private string RandomString(int length)
  {
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstu vwxyz";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[_random.Next(s.Length)]).ToArray());
  }

  public void SetTextVisibility(bool value)
  {
    Tween tween = CreateTween().SetParallel();
    Color modulate = value ? Colors.White : Colors.Transparent;

    tween.TweenProperty(FighterNameLabel, "self_modulate", modulate, 1);
    tween.TweenProperty(AbilityNameLabel, "self_modulate", modulate, 1);
    tween.TweenProperty(WeaponNameLabel, "self_modulate", modulate, 1);
  }
}
