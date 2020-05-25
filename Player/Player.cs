using Godot;
using StateMachine;
using Godot.Collections;
using System;


public class Player : Area2D {
	[Signal]
	public delegate void Hit();
	[Export]
	public int speed = 200; // pixels per seconds
	
	public AnimatedSprite animatedSprite;
	public CollisionShape2D collisionShape;
	public Vector2 screenSize;
	public Vector2 velocity = new Vector2(0,0);
	
	public String[] statesInfo;
	public FSM machine;	

	// Initialisisation : Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print("Player Ready");
		
		statesInfo = new String[]{"Idle","Run"};
		machine = new FSM(statesInfo);

		// On obtient la taille de l'écran visible (de la fenêtre)
		screenSize = GetViewport().Size;
		//Hide();
		
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		
		Position = new Vector2(
			x: Mathf.Clamp(0 + 60, 0, screenSize.x),
			y: Mathf.Clamp(screenSize.y - 100, 0, screenSize.y)
		);
	}
	
	// IDLE
	 public void OnEnterIdle() {
	 	//GD.Print("on enter idle");
	 	animatedSprite.Animation = "idle";
	 }
	 public void OnUpdateIdle() {
	 	if (GetSpeed() > .5) {
	 		machine.ChangeState("Run");
	 	}
	 }
	 public void OnExitIdle() {
		//GD.Print("on exit idle");
	 }

	// RUN
	 public void OnEnterRun() {
		//GD.Print("on enter run");
	 	animatedSprite.Animation = "run";
	 }
	 public void OnUpdateRun() {
	 	if (GetSpeed() < .5) {
	 		machine.ChangeState("Idle");
	 	}
	 }
	 public void OnExitRun() {
		//GD.Print("on exit run");
	 }

//	public override void _Input(InputEvent inputEvent) {
//
//	}

	public float GetSpeed() {
		return (Mathf.Abs( velocity.x ) + Mathf.Abs( velocity.y) )/2;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
  	public override void _Process(float delta) {

		velocity = new Vector2(0,0);
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		
		GD.Print(machine.GetCurrentStateName());
		
		if (machine.GetCurrentStateName() == "Idle") {
			OnEnterIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnEnterRun();
		} 
		

		if (Input.IsActionPressed("ui_right")) {
			velocity.x += 50;
		}
		if (Input.IsActionPressed("ui_left")) {
			velocity.x -= 50;
		}
		if (Input.IsActionPressed("ui_down")) {

		}
		if (Input.IsActionPressed("ui_jump")) {

		}
		if (Input.IsActionPressed("ui_sword_out")) {

		}


		if (velocity.Length() > 0)
		{
			// Normalized set Length to 1 ; avoiding fast diagonal movements
			velocity = velocity.Normalized() * speed;

		}
		else if (velocity.Length() < 0)
		{

		}
		
		// Update Position
		Position += velocity * delta;
		Position = new Vector2(
			x: Mathf.Clamp(Position.x, 0, screenSize.x),
			y: Mathf.Clamp(Position.y, 0, screenSize.y)
		);

		// move on left or right
		if (velocity.x != 0) {

			animatedSprite.FlipV = false;
			if (velocity.x < 0)
			{
				animatedSprite.FlipH = true;
			}
			else
			{
				animatedSprite.FlipH = false;
			}
		}
		else if (velocity.x == 0 && velocity.y == 0) {

		}
		
		if (machine.GetCurrentStateName() == "Idle") {
			OnUpdateIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnUpdateRun();
		} 
		
		Play();
		//machine.Update();
		
		if (machine.GetCurrentStateName() == "Idle") {
			OnExitIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnExitRun();
		} 
	}
	
	public void Play() {
		animatedSprite.Play();
	}
	
	
	private void OnAnimatedSpriteAnimationFinished()
	{		
		if (animatedSprite.Animation == "jump") {
			//STATUS_JUMP = false;
			animatedSprite.Stop();
		}
		else if (animatedSprite.Animation == "attack1") {
			//STATUS_ATTACK = false;
			animatedSprite.Stop();
		}		
	}

		
	private void OnPlayerBodyEntered(object body)
	{
		Hide(); // Player disappears after being hit.
		EmitSignal("Hit");
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
	}
	
	public void Start(Vector2 pos)
	{
		Position = pos;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

}






