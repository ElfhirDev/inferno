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
	public Camera2D camera;
	public CollisionShape2D collisionShape;
	public Timer magicTimer;
	public RayCast2D rayJump;
	public Sprite sprite;
	public CollisionShape2D swordShape;

	public Vector2 screenSize;
	public Vector2 movement = new Vector2(0,0);
	
	public const float FLOOR_DETECT_DISTANCE = 50.0f;
	
	public override void _Ready() {
		
		screenSize = GetViewport().Size;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		attackArea = GetNode<Area2D>("AttackArea");
		camera = GetNode<Camera2D>("Camera2D");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		magicTimer = GetNode<Timer>("MagicAnimation");
		rayJump = GetNode<RayCast2D>("RayJump");
		sprite = GetNode<Sprite>("Sprite");
		swordShape = GetNode<CollisionShape2D>("AttackArea/SwordShape");
		
		stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/StateMachine/playback");
		
		GD.Print("Hero Ready");
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
		
		velocity.y += float.Parse(gravity.ToString()) * delta;
		GD.Print(velocity.y);
		
		bool isJumpInterrupted = (Input.IsActionJustReleased("ui_jump") && (velocity.y < 0.0));
		velocity =  CalculateMoveVelocity(velocity, direction, speed, isJumpInterrupted);
		
		Vector2 snapVector = Vector2.Zero;
		if (direction.y == 0.0) {
			snapVector = Vector2.Down * FLOOR_DETECT_DISTANCE;
		} else {
			snapVector = Vector2.Zero;
		}
				
		bool isOnPlatform = rayJump.IsColliding();
		
		GD.Print(isOnPlatform);
		
		// move on surface and snap on it
		velocity = MoveAndSlideWithSnap(
			velocity, snapVector, FLOOR_NORMAL, !isOnPlatform, 4, 0.9f, false
		);
		
		GD.Print(velocity);
		
		// flip sprite on left/right
		if (direction.x != 0) {
			float current_x = sprite.Scale.x;
			float current_y = sprite.Scale.y;
			
			if (direction.x > 0) {
				sprite.SetScale(new Vector2(1 * current_x, current_y));
			}
			else {
				sprite.SetScale(new Vector2(-1 * current_x, current_y));
			}
		}
		
		// TODO casting magic 
		bool isMagicCasting = false;
//		if (Input.is_action_just_pressed("ui_magic" + action_suffix)) {
//			isMagicCasting = 
//		}

		string animation = GetNewAnimation(isMagicCasting);
				
		// && magicTimer.IsStopped()
		if (animation != animationPlayer.CurrentAnimation) {
			
//			GD.Print(
//				"animation " + animation + 
//				" ; animationPlayer.CurrentAnimation " + 
//				animationPlayer.CurrentAnimation
//			);

			if (isMagicCasting) {
				magicTimer.Start();
			}
			animationPlayer.Play(animation);
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
		if (isJumpInterrupted) {
			velocity.y = 0.0f;
		}
		return velocity;
	}
	
	public Vector2 GetDirection() {
		
		float x = 0;
		x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		float y = 0;
		
		if (IsOnFloor() && Input.IsActionJustPressed("ui_jump")) {
			y = -1;
		}
		else {
			y = 0;
		}
		
		Vector2 direction = new Vector2(x,y);
		return direction;
	}
	
	public string GetNewAnimation(bool isMagicCasting = false) {
		string newAnimation = "";
		if (IsOnFloor()) {
			
			GD.Print("Is on floor");
			newAnimation = "idle";
			
			if (Mathf.Abs(velocity.x) > 0.1f) {
				newAnimation = "run";
			} 
			else {
				newAnimation = "idle";
			}
		}
		else {
			
			
			if (velocity.y > 0) {
				newAnimation = "fall";
			}
			else {
				newAnimation = "jump";
			}
		}
		
		if (isMagicCasting) {
			newAnimation = "magic";
		}
		return newAnimation;
	}
	
	public void Run() {
		stateMachine.Travel("run");
	}

}
