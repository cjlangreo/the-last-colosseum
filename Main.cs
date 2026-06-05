using Godot;
using System;

public partial class Main : Node2D
{
  [Export] public MeshInstance2D DissolveMesh;
  private ShaderMaterial BurnShader => (ShaderMaterial)DissolveMesh.Material;
  private Tween _dissolveTween;
  
  
  public override void _Ready()
  {
   _dissolveTween = CreateTween();
   _dissolveTween.TweenMethod(Callable.From<float>(SetDissolvePercentage), 1.0, 0.0, 2.0);
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
