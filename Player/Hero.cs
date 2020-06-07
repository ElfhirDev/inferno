using System;
using Godot;
using Godot.Collections;


public class Hero : KinematicBody2D
{
	[Export]
	public int speed = 200; // pixels per seconds
	public int gravity = 300;
	public int jumpForce = 420;
	
	public AnimationPlayer animationPlayer;
	public AnimationTree animationTree;
	public object stateMachine;	

	public Area2D attackArea;
	public Camera2D camera;
	public CollisionShape2D collisionShape;
	public RayCast2D rayJump;
	public Sprite sprite;
	public CollisionShape2D swordShape;

	public Vector2 screenSize;
	public Vector2 velocity = new Vector2(0,0);
	public Vector2 movement = new Vector2(0,0);
	
	private Vector2 _UP = new Vector2(0,-1);
	
	public override void _Ready() {
		
		screenSize = GetViewport().Size;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		attackArea = GetNode<Area2D>("AttackArea");
		camera = GetNode<Camera2D>("Camera2D");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		rayJump = GetNode<RayCast2D>("RayJump");
		sprite = GetNode<Sprite>("Sprite");
		swordShape = GetNode<CollisionShape2D>("AttackArea/SwordShape");
		
		stateMachine = animationTree.Get("Parameters/Playback");
		
		GD.Print("Hero Ready");
	}
	
}
