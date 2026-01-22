using Godot;
using System;


public partial class ParticleHvezdy : CPUParticles2D
{
	public override void _Ready()
	{
		this.Emitting = true;
		TimeToEnd();
	}

	
	async void TimeToEnd()
	{
		SceneTreeTimer timer = GetTree().CreateTimer(1);
        await ToSignal(timer, "timeout");
        timer.Dispose();
		this.QueueFree();
		this.Dispose();
	}
}
