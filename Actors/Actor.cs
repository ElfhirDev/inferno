using System;
using Godot;
using Godot.Collections;

public class Actor : KinematicBody2D
{
	[Export]
	protected Vector2 speed = new Vector2(150.0f, 350.0f);
	
	protected object gravity = ProjectSettings.GetSetting("physics/2d/default_gravity");
	protected Vector2 FLOOR_NORMAL = Vector2.Up;
	
	protected Vector2 velocity = Vector2.Zero;
	
	public override void _Ready() {
		
	}
	
	// _PhysicsProcess is called after the inherited _PhysicsProcess function.
	// _PhysicsProcess est appelé après la fonction _PhysicsProcess hérité
	// This allows the Hero and Mob scenes to be affected by gravity.
	public override void _PhysicsProcess(float delta) {
//		velocity.y += (float)gravity * delta;
	}

}
