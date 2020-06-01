using Godot;
using StateMachine;
using System;

public class Mob : KinematicBody2D
{
	public bool alive = true;
	public int speed = 5;
	public int gravity = 4;
	
	public AnimatedSprite animatedSprite;
	public CollisionShape2D collisionShape;
	public Vector2 screenSize;
	public Vector2 velocity = new Vector2(0,0);
	
	public String[] statesInfo;
	public FSM machine;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		GD.Print("Ready Mob");
		screenSize = GetViewport().Size;
		statesInfo = new String[]{"Idle","Run","Attack","Hurt","Dying","Dead"};
		machine = new FSM(statesInfo);
	}
	
	
	public float GetSpeed() {
		return (Mathf.Abs( velocity.x ) + Mathf.Abs( velocity.y) )/2;
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

	// ATTACK
	 public void OnEnterAttack() {
		//GD.Print("on enter run");
	 	animatedSprite.Animation = "attack";
	 }
	 public void OnUpdateAttack() {

	 }
	 public void OnExitAttack() {
		//GD.Print("on exit Attack");
	 }	

	// HURT
	 public void OnEnterHurt() {
		//GD.Print("on enter run");
	 	animatedSprite.Animation = "hurt";
	 }
	 public void OnUpdateHurt() {

	 }
	 public void OnExitHurt() {
		//GD.Print("on exit Attack");
	 }

	// DYING
	 public void OnEnterDying() {
		//GD.Print("on enter run");
	 	animatedSprite.Animation = "dying";
	 }
	 public void OnUpdateDying() {
		if (!alive) {
			machine.ChangeState("Dead");
		}
	 }
	 public void OnExitDying() {
		//GD.Print("on exit Attack");
	 }
	
	// DEAD
	 public void OnEnterDead() {
		//GD.Print("on enter run");
	 	animatedSprite.Animation = "dead";
	 }
	 public void OnUpdateDead() {
	 }
	 public void OnExitDead() {
		//GD.Print("on exit Attack");
	 }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(float delta)
	{
		// Creation d'une variable de "vitesse" sur le plan 2D
		velocity = new Vector2(0,0);
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

		if (machine.GetCurrentStateName() == "Idle") {
			OnEnterIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnEnterRun();
		}
		else if (machine.GetCurrentStateName() == "Attack") {
			OnEnterAttack();
		}
		else if (machine.GetCurrentStateName() == "Hurt") {
			OnEnterHurt();
		}
		else if (machine.GetCurrentStateName() == "Dying") {
			OnEnterDying();
		}
		else if (machine.GetCurrentStateName() == "Dead") {
			OnEnterDead();
		}	

		
		animatedSprite.Play();
		

		if (Input.IsActionPressed("ui_mob_right")) {
			velocity.x += 50;
		}
		if (Input.IsActionPressed("ui_mob_left")) {
			velocity.x -= 50;
		}
		if (Input.IsActionPressed("ui_mob_attack")) {
			machine.ChangeState("Attack");
		}
		if (Input.IsActionPressed("ui_mob_hurt")) {
			machine.ChangeState("Hurt");
		}
		if (Input.IsActionPressed("ui_mob_dying")) {
			machine.ChangeState("Dying");
		}
		
		//velocity += velocity * delta;
		if (velocity.Length() > 0) {
			velocity = velocity.Normalized() * speed;
		}
		velocity.y = gravity;
		
		// move on left or right
		if (velocity.x != 0) {
			animatedSprite.FlipV = false;
			if (velocity.x > 0)
			{
				animatedSprite.FlipH = true;
			}
			else
			{
				animatedSprite.FlipH = false;
			}
		}

//		GD.Print("Position : " + Position);
//		GD.Print("Velocity : " + velocity);

		var move = MoveAndCollide(velocity);
//		var move = MoveAndSlide(100*velocity);

		
		if (machine.GetCurrentStateName() == "Idle") {
			OnUpdateIdle();
		}
		else if (machine.GetCurrentStateName() == "Run") {
			OnUpdateRun();
		}
		else if (machine.GetCurrentStateName() == "Attack") {
			OnUpdateAttack();
		}
		else if (machine.GetCurrentStateName() == "Hurt") {
			OnUpdateHurt();
		}
		else if (machine.GetCurrentStateName() == "Dying") {
			OnUpdateDying();
		}
		else if (machine.GetCurrentStateName() == "Dead") {
			OnUpdateDead();
		}
	}

	private void OnAnimatedSpriteAnimationFinished()
	{		
		if (animatedSprite.Animation == "attack") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			machine.ChangeState("Idle");
		}
		else if (animatedSprite.Animation == "hurt") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			machine.ChangeState("Idle");
		}
		else if (animatedSprite.Animation == "dying") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			alive = false;
			machine.ChangeState("Dead");
		}
		else if (animatedSprite.Animation == "dead") {
//			GD.Print("Animation named " + animatedSprite.Animation + " finished");
			QueueFree();
		}
	}

	public void OnVisibilityScreenExited()
	{
		QueueFree();
	}
}


