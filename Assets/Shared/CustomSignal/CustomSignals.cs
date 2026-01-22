using Godot;
using System;

public partial class CustomSignals : Node
{
	[Signal]
	public delegate void PlayMusicDelegat(string music);
	[Signal]
	public delegate void TimeUpDelegat();
}
