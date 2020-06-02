using Godot;
using StateMachine;
using Godot.Collections;
using System;


public class Player : KinematicBody2D {
	[Signal]
	public delegate void Hit();
	[Export]
	public int speed = 200; // pixels per seconds
	public int gravity = 300;
	public int jumpForce = 420;
	
	public AnimatedSprite animatedSprite;
	public CollisionShape2D collisionShape;
	public Vector2 screenSize;
	public Vector2 velocity = new Vector2(0,0);
	public Vector2 movement = new Vector2(0,0);
	
	private Vector2 _UP = new Vector2(0,1);
	
	public bool isAttacking = false;
	public bool isJumping = false;
	
	public String[] statesInfo;
	public FSM machine;	

	// Initialisisation : Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		GD.Print("Player Ready");
		
		statesInfo = new String[]{"Idle","Run","Attack1","Jump","Fall"};
		machine = new FSM(statesInfo);
		machine.ChangeState("Idle");

		// On obtient la taille de l'écran visible (de la fenêtre)
		screenSize = GetViewport().Size;
		//Hide();
		
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		animatedSprite.Play();
	}
	
	public void ChangeAnimation(State state) {
		animatedSprite.Animation = state.GetName().ToLower();
	}
	
	public void OnEnter() {

	}
	public void OnUpdate() {
//		GD.Print(movement);

		// Left or Right movement
		if (Mathf.Abs(movement.x) > 0) {
			machine.ChangeState("Run");
			
			if (movement.y > 0) {
				machine.ChangeState("Fall");
			}
			else if (movement.y < 0) {
				machine.ChangeState("Jump");
			}
			else {
				machine.ChangeState("Run");
			}
		}
		// No horizontal movment, no vertical movement
		else {
			if (Mathf.Abs(movement.y) == 0) {
				if (isAttacking) {
					machine.ChangeState("Attack1");
				}
				else {
					machine.ChangeState("Idle");
				}
			}
			else if (movement.y > 0) {
				machine.ChangeState("Fall");
			}
			else if (movement.y < 0) {
				machine.ChangeState("Jump");
			}
		}
	}

	public void OnExit() {
//		GD.Print("isJumping : " + isJumping + " ; and machine state : " + machine.GetCurrentStateName());
	}
	


	public float GetSpeed() {
		return (Mathf.Abs( velocity.x ) + Mathf.Abs( velocity.y) )/2;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
  	public override void _PhysicsProcess(float delta) {

		OnEnter();

		// velocity est la somme de toutes nos forces
		velocity = new Vector2(0,0);
		velocity.y = gravity;

		if (Input.IsActionPressed("ui_right")) {
			velocity.x += speed;
		}
		if (Input.IsActionPressed("ui_left")) {
			velocity.x -= speed;
		}
		if (Input.IsActionPressed("ui_down")) {
		// TODO crouch on the floor reduce the collisionshape height
		}
		if (Input.IsActionJustPressed("ui_attack1")) {
			isAttacking = true;
		}
		if (
			Input.IsActionJustPressed("ui_jump") && 
			isJumping == false && 
			machine.GetCurrentStateName() != "Fall"	
		) {
			isJumping = true;
		}
		if (Input.IsActionPressed("ui_sword_out")) {

		}

		if (isJumping) {
			velocity.y -= jumpForce;
//			GD.Print(velocity.y);
		}

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
	
		movement = MoveAndSlide(velocity, _UP);
		OnUpdate();
		ChangeAnimation(machine.GetCurrentState());
		
		

		OnExit();
		//machine.Update();
	}
	
	public void Play() {
		
	}
	
	
	private void OnAnimatedSpriteAnimationFinished()
	{		
		if (animatedSprite.Animation == "attack1") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			isAttacking = false;
			machine.ChangeState("Idle");
		}
		else if (animatedSprite.Animation == "jump") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			isJumping = false;
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




