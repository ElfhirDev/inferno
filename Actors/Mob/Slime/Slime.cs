using System;
using Godot;
using Godot.Collections;


public class Slime : Actor
{
	[Export]
	public int jumpForce = 420;

	[Export]
	public float HP = 300.0f;
	
	[Export]
	public float Force = 100.0f;
	
	protected Vector2 speed = new Vector2(120.0f, 200.0f);
	protected Vector2 direction = new Vector2(1, 1);
	
	public AnimationPlayer animationPlayer;
	public AnimationTree animationTree;
	public AnimationNodeStateMachinePlayback stateMachine;	

	public Area2D attackArea;
	public Timer attackTimer;
	public Timer magicTimer;
	public Timer idleTimer;
	public Timer runTimer;
	public Timer hurtTimer;
	public Timer dieTimer;

	public CollisionShape2D collisionShapeStanding;
	
	public Magic magic;
	public RayCast2D rayDetect;
	public Sprite sprite;
	public CollisionShape2D attackShape;
	
	public AudioStreamPlayer sfx_footstep, sfx_attack1, sfx_jump, 
		sfx_hurt, sfx_die;

	public Vector2 screenSize;
	
	
	public string attackAnimation;
	public bool isMagicCasting;
	public const float FLOOR_DETECT_DISTANCE = 50.0f;
	
		
	[Signal]
	public delegate void LifePointEmpty();
	
	[Signal]
	public delegate void LifePointLoss();
	
	public override void _Ready() {
		
		screenSize = GetViewport().Size;
		attackAnimation = "";

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackTimer = GetNode<Timer>("AttackTimer");
		animationTree = GetNode<AnimationTree>("AnimationTree");
		attackArea = GetNode<Area2D>("AttackArea");

		collisionShapeStanding = GetNode<CollisionShape2D>("Standing");

		magicTimer = GetNode<Timer>("MagicTimer");
		idleTimer = GetNode<Timer>("IdleTimer");
		runTimer = GetNode<Timer>("RunTimer");
		hurtTimer = GetNode<Timer>("HurtTimer");
		dieTimer = GetNode<Timer>("DieTimer");
		magic = GetNode<Magic>("Sprite/Magic");
		rayDetect = GetNode<RayCast2D>("RayDetect");
		sprite = GetNode<Sprite>("Sprite");
		stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		
		sfx_footstep = GetNode<AudioStreamPlayer>("Sounds/Slime/Footstep");
		sfx_attack1 = GetNode<AudioStreamPlayer>("Sounds/Slime/Attack");
		sfx_die = GetNode<AudioStreamPlayer>("Sounds/Slime/Die");
//		sfx_magicAttack = GetNode<AudioStreamPlayer>("Sounds/MagicAttack");

		idleTimer.Start();
		GD.Print("Slime Ready");
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

		if (IsOnWall()) {
			SetDirection(direction, true);
		}
		else {
			if (rayDetect.IsColliding()) {
				var collider =  rayDetect.GetCollider();
				
				if (collider is Hero) {
					attackTimer.Start();
				}
			}
		}
		

		
		
		// default gravity is too weak but I want to keep the 9.81 value.
		velocity.y += 4.0f * float.Parse(gravity.ToString()) * delta;
		
		if (!idleTimer.IsStopped()) {
			velocity = CalculateMoveVelocity(
				velocity, direction, 0*speed, false
			);
		}
		else if (!runTimer.IsStopped()) {
			velocity = CalculateMoveVelocity(
				velocity, direction, speed, false
			);
		}
		
		Vector2 snapVector = Vector2.Zero;
		if (direction.y == 0.0) {
			snapVector = Vector2.Down * FLOOR_DETECT_DISTANCE;
		} else {
			snapVector = Vector2.Zero;
		}
				

		// move on surface and snap on it
		velocity = MoveAndSlideWithSnap(
			velocity, snapVector, FLOOR_NORMAL, true, 4, 0.9f, false
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
		
		string animation = "";
		animation = GetNewAnimation();
		
		// if we have a new animation, we will Travel to it
		if (animation != animationPlayer.CurrentAnimation) {

			if (animation == "attack1") {
				Attack(1);
			}
			else if (animation == "idle") {
				Idle();
			}
			else if (animation == "run") {
				Run();
			}

		}
		
	}
	
	private void OnAttackTimerTimeout()
	{
		
	}

	private void OnMagicTimerTimeout()
	{
		
	}
	
	private void OnIdleTimerTimeout()
	{
		runTimer.Start();
	}


	private void OnRunTimerTimeout()
	{
		idleTimer.Start();
	}


	
	private void OnHurtTimerTimeout()
	{
		// Replace with function body.
	}

	private void OnDieTimerTimeout()
	{
		
	}
	
	private void OnAttackAreaBodyEntered(object body)
	{
		if (body is Actor) {
			if (body is Hero) {
				DamageTo(body);
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
		float x = 1, y = 1;	
		
		Vector2 direction = new Vector2(x,y);
		return direction;
	}
	
	public void SetDirection(Vector2 new_dir, bool back) {
		if (back) {
			new_dir = new Vector2(-1*Mathf.Sign(new_dir.x) , new_dir.y);
		}
		direction = new_dir;
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
		
		if (!dieTimer.IsStopped()) {
			return newAnimation;
		}
		
		if (!hurtTimer.IsStopped()) {
			return newAnimation;
		}
		
		// Play attack animation till the end
		if (attackAnimation != "") {
			return attackAnimation;
		}
		// during attack
		if (!attackTimer.IsStopped()) {
			if (IsOnFloor()) {
				newAnimation = "attack1";
			}

			attackAnimation = newAnimation;
		}
		else {
			if (Mathf.Abs(velocity.x) > 0.1f && !runTimer.IsStopped()) {
				newAnimation = "run";
			} 
			else if (!idleTimer.IsStopped()){
				newAnimation = "idle";
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
	}

	public void Idle() {
		stateMachine.Travel("idle");
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
	
	public void LoseHP(float ActorForce) {
		HP -= ActorForce;
		if (HP <= 0.0f) {
			EmitSignal(nameof(LifePointEmpty));
		}
		else {
			EmitSignal(nameof(LifePointLoss));
		}
		GD.Print(HP);
	}

	private void OnLifePointEmpty()
	{
		dieTimer.Start();
		stateMachine.Travel("die");
	}
	
	private void OnLifePointLoss()
	{
		hurtTimer.Start();
		stateMachine.Travel("hurt");
	}
	
	public void DamageTo(object body) {
		if (body is Hero) {
			Hero hero = (Hero)body;
			hero.LoseHP(Force);
		}
	}

}

