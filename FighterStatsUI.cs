using Godot;
using RandomBattles.Fighters;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class FighterStatsUI : VBoxContainer
{
  [Export] public bool Active = true;
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


  [Export] public HBoxContainer StrStarsContainer;
  [Export] public HBoxContainer AgiStarsContainer;
  [Export] public HBoxContainer IntStarsContainer;
  [Export] public Label StrOverflowLabel;
  [Export] public Label AgiOverflowLabel;
  [Export] public Label IntOverflowLabel;
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
  private Timer _spinTimer = new();
  private const int Spins = 10;

  public override void _Ready()
  {
    _fighter = team == Team.A ? EventBus.FighterA : EventBus.FighterB;
    _fighter.StatUpdated += OnStatUpdated;

    _weapon = _fighter.r_Weapon;
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
    WeaponIcon.Texture = _weapon.MainSprite.Texture;
    AbilityIcon.Texture = _ability?.AbilityIcon;
  }

  public override void _ExitTree()
  {
    _fighter.StatUpdated -= OnStatUpdated;
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


  private async void SpinFighterIcon(int spins)
  {
    FighterIconCollection.Shuffle();
    AddChild(_spinTimer);
    int currentSpins = 0;

    while(currentSpins <= spins)
    {
      foreach(CompressedTexture2D texture in FighterIconCollection)
      {
        FighterIcon.Texture = texture;
        await ToSignal(GetTree().CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);
      }
      currentSpins++;
    }
    FighterIcon.Texture = _fighter.r_Sprite.Texture;
    
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

}
