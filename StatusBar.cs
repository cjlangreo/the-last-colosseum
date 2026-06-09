using Godot;
using TheLastColosseumFighters;

public partial class StatusBar : Sprite2D
{
  [Export] private ProgressBar HealthBar;
  [Export] private ProgressBar DamageBar;
  [Export] private ProgressBar AbilityBar;


  [Export] private Color TeamAColor;
  [Export] private Color TeamBColor;

  [ExportGroup("Sprites")]
  [Export] private CompressedTexture2D HealthOnlyBar;
  [Export] private CompressedTexture2D HealthAbilityBar;

  public bool AbilityActive
  {
    get; set
    {
      field = value;
      ((ShaderMaterial)AbilityBar.Material).SetShaderParameter("ability_active", field);
    }
  }

  public double AbilityBarValue
  {
    get; set
    {
      field = value;
      AbilityBar.Value = field;
    }
  }



  public double Health;
  private const float DamageBarDrain = 3f;

  public override async void _Ready()
  {
    // Because without this, HealthBar's size won't be updated in time
    // when DamageBar references it.
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); 
    DamageBar.Size = new(DamageBar.Size.X, HealthBar.Size.Y);
  }

  public void SetAbilityStatus(bool hasAbility)
  {
    Texture = hasAbility ? HealthAbilityBar :HealthOnlyBar;
    AbilityBar.Visible = hasAbility;
  }

  public void SetMaxHealth(double value)
  {
    HealthBar.MaxValue = value;
    DamageBar.MaxValue = value;
  }
  public override void _Process(double delta)
  {
    UpdateHealthBar(delta);
  }

  private void UpdateHealthBar(double delta)
  {
    HealthBar.Value = Health;
    DamageBar.Value = Mathf.Lerp(DamageBar.Value, Health, delta * DamageBarDrain);
  }

  public void SetHealthColor(Team team)
  {
    bool isTeamA = team == Team.A;
    HealthBar.SelfModulate = isTeamA ? TeamAColor : TeamBColor;
  }
}