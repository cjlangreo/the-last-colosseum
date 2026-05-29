using Godot;
using System;
using RandomBattles.Utils;

public partial class RapidTestUi : VBoxContainer
{
    [Export] public Label RoundsLabel;
    [Export] public Label TeamALabel;
    [Export] public Label TeamBLabel;

    public override void _Ready()
    {
        RoundsLabel.Text = $"Rounds: {EventManager.Rounds}";
        TeamALabel.Text = $"Team A: {EventManager.TeamAPoints}";
        TeamBLabel.Text = $"Team B: {EventManager.TeamBPoints}";

    }
}
