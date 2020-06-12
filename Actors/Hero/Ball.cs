using Godot;
using System;

public class Ball : RigidBody2D
{
	public AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	private void Destroy()
	{
		animationPlayer.Play("destroy");
	}
	
	public void __QueueFree() {
		this.QueueFree();
	}
	
	private void OnBallBodyEntered(object body)
	{
//		if (body is Enemy) {
//			body.destroy();
//		}
		GD.Print(body);
	}

}





