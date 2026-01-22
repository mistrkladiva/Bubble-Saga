//using Godot;
//using System;
//using System.Collections.Generic;


//public partial class ItemList : Godot.ItemList
//{
//	Levels Levely = new Levels();
//	[Export] Label _TxtLabel;
//	public override void _Ready()
//	{
//		// Level prvnilevel = new Level();
//		// prvnilevel.LevelID = 400;
//		// Level druhylevel = new Level(){LevelID = 200};
//		// Levely.LevelsCollection.Add(prvnilevel);
//		// Levely.LevelsCollection.Add(druhylevel);
//		// Utilities.Save("pokus", Levely);

//		Levels Levely2 = Utilities.Load<Levels>("pokus");
//		if(Levely2 == default)
//		{
//			GD.Print("Levely nenalezeny");
//			return;
//		}
//		GD.Print(Levely2.LevelsCollection[0].LevelID);
//		_TxtLabel.Text = "LevelID: " + Levely2.LevelsCollection[0].LevelID;
//	}
//}



//[System.Serializable]
//public class Levels
//{
//	public List<Level> LevelsCollection {get; set;}
//	public Levels()
//	{
//		LevelsCollection = new List<Level>();
//	}
//}
