using Godot;
using System;
using TheLastColosseum.UI;
using TheLastColosseumFighters;

namespace TheLastColosseum;

public partial class Main : Node2D
{
	[Export] public ColorRect DissolveColorRect;
	[Export] public bool Random;
	[Export] public FighterStatsUI FighterStatsUIA;
	[Export] public FighterStatsUI FighterStatsUIB;
	[Export] public Control FighterStatsUIContainer;
	[Export] public Vector2 FighterStatsUIContainerFinalPos;
	[Export] public CanvasLayer UI;

	private Fighter _fighterA;
	private Fighter _fighterB;

	private ShaderMaterial BurnShader => (ShaderMaterial)DissolveColorRect.Material;
	private Tween _dissolveTween;
	private Tween _statContainerTween;

	private bool _teamASpinDone = false;
	private bool _teamBSpinDone = false;
	private const float TransitionDuration = 2.0f;
	private float _startDelay = TransitionDuration - .5f;


	[Signal] private delegate void FinishedEventHandler();


	public override async void _EnterTree()
	{
		GetTree().Paused = true;
		FighterStatsUIContainer.Modulate = Colors.Transparent;
		_statContainerTween = FighterStatsUIContainer.CreateTween();
		_statContainerTween.TweenProperty(FighterStatsUIContainer, "modulate", Colors.White, 1);

		UI.Show();
		

		if (Random)
		{
			FighterStatsUIA.RandomFighter = true;
			FighterStatsUIB.RandomFighter = true;
			FighterStatsUIA.SpinDone += () => OnSpinDone(Team.A);
			FighterStatsUIB.SpinDone += () => OnSpinDone(Team.B);
		}
		else
		{
			await ToSignal(GetTree().CreateTimer(_startDelay), SceneTreeTimer.SignalName.Timeout);
			StartFight();
		}
	}

	public override void _Ready()
	{
		_fighterA = (Fighter)GetTree().GetFirstNodeInGroup("Team A");
		_fighterB = (Fighter)GetTree().GetFirstNodeInGroup("Team B");
		_fighterA.Died += OnRoundEnd;
		_fighterB.Died += OnRoundEnd;
	}

	private async void OnRoundEnd()
	{
		Title title = DissolveColorRect.GetChild<Title>(0);
		title.Hide();
		await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
		FighterStatsUIContainer.ZIndex = -1;
		StartDissolveTransition(0, 1);
		await ToSignal(this, SignalName.Finished);
		GetTree().Quit();
	}

	private async void StartFight()
	{
		StartDissolveTransition(1, 0);
		await ToSignal(GetTree().CreateTimer(_startDelay), SceneTreeTimer.SignalName.Timeout);
		SetFighterStatsUITextVisiblity(false);
		MoveFighterUITop();
		FighterStatsUIA.Active = true;
		FighterStatsUIB.Active = true;
		GetTree().Paused = false;
	}

	private void SetFighterStatsUITextVisiblity(bool value)
	{
		FighterStatsUIA.SetTextVisibility(value);
		FighterStatsUIB.SetTextVisibility(value);
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

	private void MoveFighterUITop()
	{
		Tween.TransitionType transType = Tween.TransitionType.Circ;
		Tween.EaseType easeType = Tween.EaseType.InOut;
		Tween tween = CreateTween().SetEase(easeType).SetTrans(transType);
		tween.TweenProperty(FighterStatsUIContainer, "global_position:y", FighterStatsUIContainerFinalPos.Y, 1);

	}

	private async void StartDissolveTransition(float from, float to)
	{
		if (IsInstanceValid(_dissolveTween))
		{
			_dissolveTween.Kill();
		}
		_dissolveTween = DissolveColorRect.CreateTween();
		_dissolveTween.TweenMethod(Callable.From<float>(SetDissolvePercentage), from, to, TransitionDuration);
		await ToSignal(_dissolveTween, Tween.SignalName.Finished);
		EmitSignal(SignalName.Finished);
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
