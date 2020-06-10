using System;
using Godot;
using Godot.Collections;


public class Hero : Actor
{
	[Export]
	public int jumpForce = 420;
	
	public AnimationPlayer animationPlayer;
	public AnimationTree animationTree;
	public AnimationNodeStateMachinePlayback stateMachine;	

	public Area2D attackArea;
	public Timer attackTimer;
	public Camera2D camera;
	public CollisionShape2D collisionShapeStanding, collisionShapeCrouch;
//	public Timer magicTimer;
	public RayCast2D rayJump;
	public Sprite sprite;
	public CollisionShape2D swordShape;

	public Vector2 screenSize;
	
	public const float FLOOR_DETECT_DISTANCE = 50.0f;
	
	public override void _Ready() {
		
		screenSize = GetViewport().Size;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackTimer = GetNode<Timer>("AttackTimer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		attackArea = GetNode<Area2D>("AttackArea");
		camera = GetNode<Camera2D>("Camera2D");
		collisionShapeStanding = GetNode<CollisionShape2D>("Standing");
		collisionShapeCrouch = GetNode<CollisionShape2D>("Crouch");
//		magicTimer = GetNode<Timer>("MagicAnimation");
		rayJump = GetNode<RayCast2D>("RayJump");
		sprite = GetNode<Sprite>("Sprite");
		stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		
		GD.Print("Hero Ready");
//		GD.Print(stateMachine);
	}
	
	// We use separate functions to calculate the direction and velocity to make this one easier to read.
	// At a glance, you can see that the physics process loop:
	// 1. Calculates the move direction.
	// 2. Calculates the move velocity.
	// 3. Moves the character.
	// 4. Updates the sprite direction.
	// 5. Shoots bullets.
	// 6. Updates the animation.
	public override void _PhysicsProcess(float delta) {
		Vector2 direction = GetDirection();
		
		// default gravity is too weak but I want to keep the 9.81 value.
		velocity.y += 4.0f * float.Parse(gravity.ToString()) * delta;
		
		bool isJumpInterrupted = (
			Input.IsActionJustPressed("ui_jump")
		);
		
		velocity = CalculateMoveVelocity(
			velocity, direction, speed, isJumpInterrupted
		);
		
		Vector2 snapVector = Vector2.Zero;
		if (direction.y == 0.0) {
			snapVector = Vector2.Down * FLOOR_DETECT_DISTANCE;
		} else {
			snapVector = Vector2.Zero;
		}
				
		bool isOnPlatform = rayJump.IsColliding();
		
		// move on surface and snap on it
		velocity = MoveAndSlideWithSnap(
			velocity, snapVector, FLOOR_NORMAL, !isOnPlatform, 4, 0.9f, false
		);
		
		// flip sprite on left/right
		if (direction.x != 0) {
			Vector2 initialScale = GetScale();
			
			if (direction.x > 0) {
//				sprite.FlipH = false;
				Vector2 newScale = new Vector2(
					Mathf.Sign(initialScale.y) * initialScale.x, 
					initialScale.y
				);
					
				SetScale(newScale);
			}
			else {
//				sprite.FlipH = true;
				Vector2 newScale = new Vector2(
					-1.0f * Mathf.Sign(initialScale.y) * initialScale.x, 
					initialScale.y
				);
				SetScale(newScale);
			}
			
		}
		
		
		// TODO casting magic 
		bool isMagicCasting = false;
//		if (Input.IsActionJustPressed("ui_magic")) {
//			isMagicCasting = true; 
//		}
		
		if (Input.IsActionJustReleased("ui_attack") && attackTimer.IsStopped()) {
			attackTimer.Start();
		}

		string animation = GetNewAnimation();
		
		// if we have a new animation, we will Travel to it
		if (animation != animationPlayer.CurrentAnimation) {
			if (!collisionShapeCrouch.IsDisabled()) {
				collisionShapeCrouch.Disabled = true;
			}
			if (collisionShapeStanding.IsDisabled()) {
				collisionShapeStanding.Disabled = false;
			}

			if (animation == "attack1") {
				Attack(1);
			}
			else if (animation == "airAttack") {
				AirAttack(1);
			}
			else if (animation == "airAttackEnd") {
				AirAttack(0);
			}
			else if (animation == "crouch") {
				Crouch();
			}
			else if (animation == "fall") {
				Fall();
			}
			else if (animation == "idle") {
				Idle();
			}
			else if (animation == "jump") {
				Jump();
			}
			else if (animation == "run") {
				Run();
			}
			else if (animation == "runAttack1") {
				RunAttack(1);
			}
		}
		
	}
	
	public Vector2 CalculateMoveVelocity(
		Vector2 linearVelocity, 
		Vector2 direction, 
		Vector2 speed, 
		bool isJumpInterrupted
	) {
		
		Vector2 velocity = linearVelocity;
		velocity.x = speed.x * direction.x;
		
		if (direction.y != 0.0f) {
			velocity.y = speed.y * direction.y;
		}
//		if (isJumpInterrupted) {
//			velocity.y = 0.0f;
//		}
		return velocity;
	}
	
	public Vector2 GetDirection() {
		
		float x = 0;
		x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		float y = 0;
		
		if (IsOnFloor() && Input.IsActionJustPressed("ui_jump")) {
			y =  - 1;
		}
		else {
			y = 0;
		}
		
		Vector2 direction = new Vector2(x,y);
		return direction;
	}
	
	public string GetNewAnimation() {
		string newAnimation = animationPlayer.CurrentAnimation;
		if (IsOnFloor()) {
			
			if (Mathf.Abs(velocity.x) > 0.1f) {
				if (!attackTimer.IsStopped()) {
					newAnimation = "runAttack1";
				}
				else {
					newAnimation = "run";
				}
			} 
			else {
				if (!attackTimer.IsStopped()) {
					newAnimation = "attack1";
				}
				else if (Input.IsActionPressed("ui_down")) {
					newAnimation = "crouch";
				}
				else {
					newAnimation = "idle";
				}
			}
		}
		else {
			if (velocity.y > 0) {
				if (!attackTimer.IsStopped()) {
					newAnimation = "airAttackEnd";
				}
				else {
					newAnimation = "fall";
				}
			}
			else {
				if (!attackTimer.IsStopped()) {
					newAnimation = "airAttack";
				}
				else {
					newAnimation = "jump";
				}
				
			}
		}

		return newAnimation;
	}
	
	public void Attack(int ID) {
		if (ID == 1) {
			stateMachine.Travel("attack1");
		}
		else if (ID == 2) {
			stateMachine.Travel("attack2");
		}
		else if (ID == 3) {
			stateMachine.Travel("attack3");
		}
	}
	public void RunAttack(int ID) {
		if (ID == 1) {
			stateMachine.Travel("runAttack1");
		}
		else if (ID == 2) {
			stateMachine.Travel("runAttack2");
		}
	}
	public void AirAttack(int ID) {
		if (ID == 1) {
			stateMachine.Travel("airAttack");
		}
		else if (ID == 0) {
			stateMachine.Travel("airAttackEnd");
		}
	}
	public void Crouch() {
		stateMachine.Travel("crouch");
		if (collisionShapeCrouch.IsDisabled()) {
			collisionShapeCrouch.Disabled = false;
		}
		if (!collisionShapeStanding.IsDisabled()) {
			collisionShapeStanding.Disabled = true;
		}
	}	
	public void Fall() {
		stateMachine.Travel("fall");
	}
	public void Idle() {
		stateMachine.Travel("idle");
	}
	public void Jump() {
		stateMachine.Travel("jump");
	}
	public void Run() {
		stateMachine.Travel("run");
	}
	
	private void OnAttackTimerTimeout()
	{
		Godot.Collections.Array attackShapes = attackArea.GetChildren();
		foreach(CollisionShape2D shape in attackShapes) {
			shape.Hide();
		}
	}
}



