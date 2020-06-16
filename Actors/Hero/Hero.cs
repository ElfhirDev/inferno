using System;
using Godot;
using Godot.Collections;


public class Hero : Actor
{
	[Export]
	public int jumpForce = 420;
	
	[Export]
	public float HP = 1000.0f;
	
	[Export]
	public float Force = 100.0f;
	

	
	public AnimationPlayer animationPlayer;
	public AnimationTree animationTree;
	public AnimationNodeStateMachinePlayback stateMachine;	

	public Area2D attackArea;
	public Timer attackTimer;
	public Timer magicTimer;
	public Camera2D camera;
	public CollisionShape2D collisionShapeStanding, collisionShapeCrouch;
	
	public Magic magic;
	public RayCast2D rayJump;
	public Sprite sprite;
	public CollisionShape2D swordShape;
	
	public AudioStreamPlayer sfx_footstep, sfx_attack1, sfx_jump, 
		sfx_hurt, sfx_die, sfx_magicAttack, sfx_airAttack1, sfx_runAttack1,
		sfx_airAttackEnd;

	public Vector2 screenSize;
	
	
	public string attackAnimation;
	public bool isMagicCasting;
	public const float FLOOR_DETECT_DISTANCE = 50.0f;
	
	public override void _Ready() {
		
		screenSize = GetViewport().Size;
		attackAnimation = "";

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackTimer = GetNode<Timer>("AttackTimer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		attackArea = GetNode<Area2D>("AttackArea");
		camera = GetNode<Camera2D>("Camera2D");
		collisionShapeStanding = GetNode<CollisionShape2D>("Standing");
		collisionShapeCrouch = GetNode<CollisionShape2D>("Crouch");
		magicTimer = GetNode<Timer>("MagicTimer");
		magic = GetNode<Magic>("Sprite/Magic");
		rayJump = GetNode<RayCast2D>("RayJump");
		sprite = GetNode<Sprite>("Sprite");
		stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		
		sfx_footstep = GetNode<AudioStreamPlayer>("Sounds/Footstep");
		sfx_jump = GetNode<AudioStreamPlayer>("Sounds/Jump");
		sfx_attack1 = GetNode<AudioStreamPlayer>("Sounds/Attack1");
		sfx_die = GetNode<AudioStreamPlayer>("Sounds/Die");
		sfx_magicAttack = GetNode<AudioStreamPlayer>("Sounds/MagicAttack");
		sfx_airAttack1 = GetNode<AudioStreamPlayer>("Sounds/AirAttack1");
		sfx_airAttackEnd = GetNode<AudioStreamPlayer>("Sounds/AirAttackEnd");
		sfx_runAttack1 = GetNode<AudioStreamPlayer>("Sounds/RunAttack1");
		
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
		isMagicCasting = false;
		if (Input.IsActionJustPressed("ui_magic")) {
			isMagicCasting = true;
			magicTimer.Start();
			float direc = GetLookingDirection().x;
			
			magic.Shoot(direc);
			if (!sfx_magicAttack.IsPlaying()) {
				sfx_magicAttack.Play();
			}
		}
		
		if (Input.IsActionJustReleased("ui_attack") && attackTimer.IsStopped()) {
			attackTimer.Start();
		}

		string animation = "";
		animation = GetNewAnimation();
		
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
			else if (animation == "magic") {
				Magic();
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
	
	public Vector2 GetLookingDirection() {
		Vector2 movingDirection = GetDirection();
		if (movingDirection.x == 0) {
			movingDirection.x = GetScale().y;
		}	
		return movingDirection;
	}
	


	public string GetNewAnimation() {
		string newAnimation = animationPlayer.CurrentAnimation;
		
		// Play attack animation till the end
		if (attackAnimation != "") {
			return attackAnimation;
		}
		// during attack
		if (!attackTimer.IsStopped()) {
			if (IsOnFloor()) {
				if (Mathf.Abs(velocity.x) > 0.1f) {
					newAnimation = "runAttack1";
				} 
				else {
					newAnimation = "attack1";
				}
			}
			else {
				if (velocity.y > 0) {
					if (Input.IsActionPressed("ui_down")) {
						newAnimation = "airAttackEnd";
					}
					else {
						newAnimation = "airAttack";	
					}
				}
				else if (velocity.y <= 0) {
					newAnimation = "airAttack";	
				}
			}
			
			attackAnimation = newAnimation;
		}
		else if (!magicTimer.IsStopped()) {
//			GD.Print("magic");
			newAnimation = "magic";
		}
		else {
			if (Mathf.Abs(velocity.x) > 0.1f) {
				if (velocity.y > 0) {
					newAnimation = "fall";
				}
				else if (velocity.y < 0) {
					newAnimation = "jump";
				}
				else {
					newAnimation = "run";
				}
			} 
			else {
				if (Input.IsActionPressed("ui_down")) {
					newAnimation = "crouch";
				}
				else if (velocity.y > 0) {
					newAnimation = "fall";
				}
				else if (velocity.y < 0) {
					newAnimation = "jump";
				}
				else {
					newAnimation = "idle";
				}
			}			
		}

		return newAnimation;
	}
	
	public void Attack(int ID) {
		if (ID == 1) {
			if (!sfx_attack1.IsPlaying()) {
				sfx_attack1.Play();
			}
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
			if (!sfx_runAttack1.IsPlaying()) {
				sfx_runAttack1.Play();
			}
			stateMachine.Travel("runAttack1");
		}
		else if (ID == 2) {
			stateMachine.Travel("runAttack2");
		}
	}
	public void AirAttack(int ID) {
		if (ID == 1) {
			if (!sfx_airAttack1.IsPlaying()) {
				sfx_airAttack1.Play();
			}
			stateMachine.Travel("airAttack");
		}
		else if (ID == 0) {
			if (!sfx_airAttackEnd.IsPlaying()) {
				sfx_airAttackEnd.Play();
			}
			stateMachine.Travel("airAttackEnd");
			velocity.y += 200.0f;
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
		if (!sfx_jump.IsPlaying()) {
			sfx_jump.Play();
		}
	}
	public void Magic() {
		stateMachine.Travel("magic_cast");
	}
	public void Run() {
		stateMachine.Travel("run");
		if (!sfx_footstep.IsPlaying()) {
			sfx_footstep.Play();
		}
	}
	
	public void FinishAttackAnimation() {
		attackAnimation = "";
//		attackTimer.Stop();
	}
	
	private void ResetAttackShapes() {
		Godot.Collections.Array attackShapes = attackArea.GetChildren();
		foreach(CollisionShape2D shape in attackShapes) {
			shape.Hide();
		}
		attackArea.Hide();
	}
	private void OnAttackTimerTimeout()
	{
//		GD.Print("OnAttackTimerTimeout");
		ResetAttackShapes();
	}

	private void OnMagicTimerTimeout()
	{
		// Replace with function body.
	}
	
	private void OnAttackAreaBodyEntered(object body)
	{
		if (body is Actor) {
			if (body is Slime) {
				Damage(body);
			}
		}
	}
	
	public void Damage(object body) {
		if (body is Slime) {
			Slime slime = (Slime)body;
			slime.LoseHP(Force);
		}
	}

//	[Signal]
//	public delegate void OnZeroHP() {
//
//	}

}






