using Godot;
using System;

public partial class HealthBar : ProgressBar
{
    public double Health;
    [Export] public ProgressBar DamageBar;


    public void SetMaxValue(double value)
    {
        MaxValue = value;
        DamageBar.MaxValue = value;
    }

    public override void _Process(double delta)
    {
        Value = Health;
        DamageBar.Value = Mathf.Lerp(DamageBar.Value, Health, delta * 2);
    }
}
