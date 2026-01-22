using Godot;
using System;
using System.Collections.Generic;

[System.Serializable]
public partial class Level
{
	public int LevelID {get; set;}
	public int TypeCount {get; set;}
	public string MusicName {get; set;}
	public string BackgroundName {get; set;}
	public int GridSizeX {get; set;}
	public int GridSizeY {get; set;}
	public int TimeMax {get; set;}
	public int LevelUp {get; set;}
	public bool BonusTimer {get; set;}
	public bool BonusBomb {get; set;}
}

[System.Serializable]
public class Levels
{
	public List<Level> LevelsCollection { get; set; }
	public Levels()
	{
		LevelsCollection = new List<Level>();
	}
}