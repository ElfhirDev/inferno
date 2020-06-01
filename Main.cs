using Godot;
using System;

public class Main : Node2D
{
	
	[Export]
	public PackedScene Mob;

	private int _score;

	// We use 'System.Random' as an alternative to GDScript's random methods.
	private Random _random = new Random();


	public override void _Ready() {
		GD.Print("Main Ready");
		NewGame();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		
	  	if (Input.IsActionPressed("ui_end")) {
			GD.Print("Main Process : ui_end");
			GetTree().Quit();
		}
	}
	
	private float RandRange(float min, float max) {
		return (float)_random.NextDouble() * (max - min) + min;
	}

	private void GameOver() {
		GetNode<Timer>("MobTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();
	}
	
	public void NewGame() {
		_score = 0;
		
		var player = GetNode<Player>("Player");
		var startPosition = GetNode<Position2D>("StartPosition");
		player.Start(startPosition.Position);
		
		GetNode<Timer>("StartTimer").Start();
	}
	
	
	
	private void OnMobTimerTimeout() {
		GD.Print("OnMobTimerTimeout");
		// Choose a random location on Path2D.

		var mobStartPosition = GetNode<Position2D>("MobStartPosition");
		
		// Create a Mob instance and add it to the scene.
		var mobInstance = (KinematicBody2D)Mob.Instance();
		AddChild(mobInstance);
		
		// Set the mob's position to a random location.
		mobInstance.Position = mobStartPosition.Position;
		
		// Add some randomness to the direction.
//		direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
//		mobInstance.Rotation = direction;
		
		// Choose the velocity.
//		mobInstance.SetLinearVelocity(new Vector2(RandRange(150f, 250f), 0));
	}
	
	
	private void OnScoreTimerTimeout() {
		_score++;
	}
	
	
	private void OnStartTimerTimeout() {
		GetNode<Timer>("MobTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();
	}

}


