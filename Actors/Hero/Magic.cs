using Godot;
using System;

public class Magic : Position2D
{


	public float BULLET_VELOCITY = 300.0f;
	public PackedScene ballScene;
	public RigidBody2D ball;
	
	public Timer cooldown;
	public AudioStreamPlayer2D shootSound;

	public override void _Ready()
	{
		cooldown = GetNode<Timer>("Cooldown");
		shootSound = GetNode<AudioStreamPlayer2D>("Shoot");
		ballScene = ResourceLoader.Load("res://Actors/Hero/Ball.tscn") as PackedScene;
	}

	
	// Shoot a Magic ball
	public bool Shoot(float direction = 1.0f) {
		ball = ballScene.Instance() as RigidBody2D;
		if (!cooldown.IsStopped()) {
			return false;
		}
//		ball.SetGlobalPosition(GetGlobalPosition());
		Vector2 impulse = new Vector2(direction * BULLET_VELOCITY, 0);
		Vector2 offset = Vector2.Zero;
		ball.ApplyImpulse(offset, impulse);
//		ball.LinearVelocity = new Vector2(direction * BULLET_VELOCITY, 0);
//		ball.SetAsTopLevel(true);
		AddChild(ball);
		shootSound.Play();
		return true;
	}

}
