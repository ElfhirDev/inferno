using Godot;
using System;

public class Mob : RigidBody2D
{
	public int speed = 100;
	private AnimatedSprite _animatedSprite;
	private CollisionShape2D _collisionShape;
	private Vector2 _screenSize;
	private Vector2 _velocity = new Vector2(0,0);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Ready Mob");
		_screenSize = GetViewport().Size;
		
		Position = new Vector2(
			x: Mathf.Clamp(0, 0, _screenSize.x),
			y: Mathf.Clamp(0, 0, _screenSize.y)
		);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		// Creation d'une variable de "vitesse" sur le plan 2D
		_velocity = new Vector2(0,0);
		_animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_animatedSprite.Animation =  "move";
		
		GD.Print(Position);
		
		if (_velocity.Length() > 0)
		{
			_velocity = _velocity.Normalized() * speed;
			_animatedSprite.Play();
		}
		else
		{
			_animatedSprite.Stop();
		}
			
	}
	

	public void OnVisibilityScreenExited()
	{
		QueueFree();
	}
}
