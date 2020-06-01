using Godot;
using StateMachine;
using Godot.Collections;
using System;


public class Player : KinematicBody2D {
	[Signal]
	public delegate void Hit();
	[Export]
	public int speed = 5; // pixels per seconds
	public int gravity = 4;
	
	public AnimatedSprite animatedSprite;
	public CollisionShape2D collisionShape;
	public Vector2 screenSize;
	public Vector2 velocity = new Vector2(0,0);
	public Vector2 movement = new Vector2(0,0);
	
	public String[] statesInfo;
	public FSM machine;	

	// Initialisisation : Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print("Player Ready");
		
		statesInfo = new String[]{"Idle","Run","Attack1","Fall"};
		machine = new FSM(statesInfo);

		// On obtient la taille de l'écran visible (de la fenêtre)
		screenSize = GetViewport().Size;
		//Hide();
		
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		
		Position = new Vector2(
			x: Mathf.Clamp(0 + 60, 0, screenSize.x),
			y: Mathf.Clamp(60, 0, screenSize.y)
		);
	}
	
	// IDLE
	 public void OnEnterIdle() {
	 	//GD.Print("on enter idle");
	 	animatedSprite.Animation = "idle";
	 }
	 public void OnUpdateIdle() {
		if (Mathf.Abs( velocity.x ) > .5) {
			machine.ChangeState("Run");
		}
		if (movement.y > .5) {
			machine.ChangeState("Fall");
		}
	 }
	 public void OnExitIdle() {
		//GD.Print("on exit idle");
	 }

	// RUN
	 public void OnEnterRun() {
//		GD.Print("on enter run");
	 	animatedSprite.Animation = "run";
	 }
	 public void OnUpdateRun() {
	 	if (Mathf.Abs(velocity.x) < .5) {
	 		machine.ChangeState("Idle");
	 	}
		if (Mathf.Abs(velocity.x) < .5 && movement.y > .5) {
			machine.ChangeState("Fall");
		}
	 }
	 public void OnExitRun() {
//		GD.Print("on exit run");
	 }

	// ATTACK 1
	 public void OnEnterAttack1() {
//		GD.Print("on enter Attack1");
	 	animatedSprite.Animation = "attack1";
	 }
	 public void OnUpdateAttack1() {

	 }
	 public void OnExitAttack1() {
//		GD.Print("on exit Attack1");
	 }
	
	// FALL
	 public void OnEnterFall() {
//		GD.Print("on enter fall");
	 	animatedSprite.Animation = "fall";
	 }
	 public void OnUpdateFall() {
	 	if (movement.y < 0.5) {
	 		machine.ChangeState("Idle");
	 	}
	 }
	 public void OnExitFall() {
//		GD.Print("on exit run");
	 }

//	public override void _Input(InputEvent inputEvent) {
//
//	}

	public float GetSpeed() {
		return (Mathf.Abs( velocity.x ) + Mathf.Abs( velocity.y) )/2;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
  	public override void _PhysicsProcess(float delta) {

		velocity = new Vector2(0,0);
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		
//		GD.Print(machine.GetCurrentStateName());
		
		if (machine.GetCurrentStateName() == "Idle") {
			OnEnterIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnEnterRun();
		} 
		else if (machine.GetCurrentStateName() == "Attack1") {
			OnEnterAttack1();
		}
		else if (machine.GetCurrentStateName() == "Fall") {
			OnEnterFall();
		}

		if (Input.IsActionPressed("ui_right")) {
			velocity.x += 50;
		}
		if (Input.IsActionPressed("ui_left")) {
			velocity.x -= 50;
		}
		if (Input.IsActionPressed("ui_down")) {
			//velocity.y += 50;
		}
		if (Input.IsActionPressed("ui_jump")) {

		}
		if (Input.IsActionPressed("ui_sword_out")) {

		}
		if (Input.IsActionPressed("ui_attack1")) {
			machine.ChangeState("Attack1");
		}

		if (velocity.Length() > 0)
		{
			// Normalized set Length to 1 ; avoiding fast diagonal movements
			velocity = velocity.Normalized() * speed;
		}
		velocity.y = gravity;

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
		
		
		movement = MoveAndSlide(50*velocity);

		
		if (machine.GetCurrentStateName() == "Idle") {
			OnUpdateIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnUpdateRun();
		} 
		else if (machine.GetCurrentStateName() == "Attack1") {
			OnUpdateAttack1();
		}
		else if (machine.GetCurrentStateName() == "Fall") {
			OnUpdateFall();
		}
		Play();
		//machine.Update();
		

	}
	
	public void Play() {
		animatedSprite.Play();
	}
	
	
	private void OnAnimatedSpriteAnimationFinished()
	{		
		if (animatedSprite.Animation == "attack1") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			machine.ChangeState("Idle");
		}	
	}
	
	private void OnAnimatedSpriteFrameChanged()
	{
//		GD.Print("Animation named " + animatedSprite.Animation + ", frame " + animatedSprite.Frame);
	}
	
	public void Start(Vector2 pos)
	{
		Position = pos;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

}




