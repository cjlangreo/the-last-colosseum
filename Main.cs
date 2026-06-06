using Godot;
using System;
using RandomBattles.Fighters;
using System.Threading.Tasks;

public partial class Main : Node2D
{
  [Export] public MeshInstance2D DissolveMesh;
  [Export] public bool Random;
  [Export] public FighterStatsUI fighterStatsUIA;
  [Export] public FighterStatsUI fighterStatsUIB;

  private ShaderMaterial BurnShader => (ShaderMaterial)DissolveMesh.Material;
  private Tween _dissolveTween;

  private bool _teamASpinDone = false;
  private bool _teamBSpinDone = false;
  private const float TransitionDuration = 2.0f;
  private float _startDelay = TransitionDuration - .5f;
  

  public override async void _EnterTree()
  {
    GetTree().Paused = true;
    if (Random)
    {
      fighterStatsUIA.RandomFighter = true;
      fighterStatsUIB.RandomFighter = true;
      fighterStatsUIA.SpinDone += () => OnSpinDone(Team.A);
      fighterStatsUIB.SpinDone += () => OnSpinDone(Team.B);
    }
    else
    {
      StartFight();
    }
  }

  private async void StartFight()
  {
    StartDissolveTransition();
    await ToSignal(GetTree().CreateTimer(_startDelay), SceneTreeTimer.SignalName.Timeout);
    GetTree().Paused = false;
  }

  private async void OnSpinDone(Team team)
  {
    if (team == Team.A) _teamASpinDone = true;
    else _teamBSpinDone = true;

    if (_teamASpinDone && _teamBSpinDone)
    {
      GD.Print("Spin done!");
      StartFight();
    }
  }

  private void StartDissolveTransition()
  {
    _dissolveTween = DissolveMesh.CreateTween();
    _dissolveTween.TweenMethod(Callable.From<float>(SetDissolvePercentage), 1.0, 0.0, TransitionDuration);
  }

  private void SetDissolvePercentage(float percentage)
  {
    BurnShader.SetShaderParameter("percentage", percentage);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }
}
