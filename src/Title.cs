using Godot;
using System;

namespace TheLastColosseum.UI;

public partial class Title : TextureRect
{
	[Export] private Texture2D NormalSprite;
	[Export] private Texture2D RandomizerSprite;

	[Export] private bool AnimateRotate = false;

	public enum Sprite
	{
		Normal,
		Randomizer
	}

	public const float RotationAmount = 5;
	public const float RotationTime = 2;

	public override void _Ready()
	{
		if(AnimateRotate) RotateTweenInit();
		Main main = GetNode<Main>("/root/Main");
		if(main.Random) SetSprite(Sprite.Randomizer);
	}

	private void RotateTweenInit()
	{
		RotationDegrees = -RotationAmount;
		Tween tween = CreateTween().SetLoops().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(this, "rotation_degrees", RotationAmount, RotationTime);
		tween.TweenProperty(this, "rotation_degrees", -RotationAmount, RotationTime);
	}

	public void SetSprite(Sprite sprite)
	{
		Texture = sprite == Sprite.Normal ? NormalSprite : RandomizerSprite;
	}
}
