using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponTrail : Line2D
{
	public Marker2D Marker;
	public float TrailLifetime = 1f;
	private bool _enabled = true;

	private Queue<Vector2> _points = [];
	private Tween _tween;

	public void Remove()
	{
		_enabled = false;
		if(IsInstanceValid(_tween)) _tween.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate", Colors.Transparent, TrailLifetime);
		_tween.TweenCallback(Callable.From(QueueFree));
	}


	public override void _Process(double delta)
	{
		if (_enabled && Marker != null)
		{
			_points.Enqueue(ToLocal(Marker.GlobalPosition));
		}

		ClearPoints();

		foreach (Vector2 point in _points)
		{
			AddPoint(point);
		}
	}
}