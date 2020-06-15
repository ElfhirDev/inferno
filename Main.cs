using Godot;
using System;

public class Main : Node2D
{
	
	[Export]
	public PackedScene Slime;

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
		GetNode<Timer>("SlimeTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();
	}
	
	public void NewGame() {
		_score = 0;
		
//		var player = GetNode<Player>("Player");
//		var startPosition = GetNode<Position2D>("StartPosition");
//		player.Start(startPosition.Position);
		
		GetNode<Timer>("StartTimer").Start();
	}
	
	
	
	private void OnSlimeTimerTimeout() {
		GD.Print("OnSlimeTimerTimeout");
		// Choose a random location on Path2D.

		var slimeStartPosition = GetNode<Position2D>("SlimeStartPosition");
		
		// Create a Slime instance and add it to the scene.
		var slimeInstance = (KinematicBody2D)Slime.Instance();
		AddChild(slimeInstance);
		
		// Set the slime's position to a random location.
		slimeInstance.Position = slimeStartPosition.Position;
		
		// Add some randomness to the direction.
//		direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
//		slimeInstance.Rotation = direction;
		
		// Choose the velocity.
//		slimeInstance.SetLinearVelocity(new Vector2(RandRange(150f, 250f), 0));
	}
	
	
	private void OnScoreTimerTimeout() {
		_score++;
	}
	
	
	private void OnStartTimerTimeout() {
		GetNode<Timer>("SlimeTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();
	}

}


