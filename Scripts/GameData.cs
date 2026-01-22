using Godot;
using System;
using System.Collections.Generic;

public class GameData : Node
{
    public const string LEVEL_DATA_PATH = "res://";
    public const float GAME_WIDTH = 1280;
    public const float GAME_HEIGHT = 720;
    public const float GAME_WIDTH_PORTRAIT = 720;
    public const float GAME_HEIGHT_PORTRAIT = 1280;

    //public CustomSignals _CustomSignals;

    public List<Bublina> BublinaPools = new List<Bublina>();
    
	public Bublina[] ItemsForChange = new Bublina[2];

    int[] AtlasPositionX = new int[8];
    int[] AtlasPositionY = new int[5];

    PackedScene Bublina_Pref;
    public static List<Level> LevelsCollection = new List<Level>();
    public static int CurrentLevel;


    public override void _Ready()
    {
        Bublina_Pref = GD.Load<PackedScene>("res://bublina.tscn");
        LevelsCollection = Utilities.Load<Levels>("pokus").LevelsCollection;
        //_CustomSignals = GetNode<CustomSignals>("/root/CustomSignals");
        
        FillBublinaPool();
        CurrentLevel = 0;
    }

    void FillBublinaPool()
    {
        // vyplni pole souradnic regionu atlasu
        int zeroPositionAtlasX = 107;
        for (int i = 0; i < AtlasPositionX.Length; i++)
        {
            AtlasPositionX[i] = 212 * i + zeroPositionAtlasX;
        }
        int zeroPositionAtlasY = 77;
        for (int i = 0; i < AtlasPositionY.Length; i++)
        {
            AtlasPositionY[i] = 216 * i + zeroPositionAtlasY;
        }

        // naplni pool
        BublinaPools.Clear();
        for (int pocetBublin = 0; pocetBublin < 200; pocetBublin++)
        {
            BublinaPools.Add((Bublina)Bublina_Pref.Instance());
        }
    }

    public void SetItemsForChange(Bublina item1, Bublina item2)
    {
        ItemsForChange[0] = item1;
        ItemsForChange[1] = item2;
        item1.SetIsMoved(true);
        item2.SetIsMoved(true);
    }

    public Rect2 GetAtlasPositon(int x, int y)
    {
        return new Rect2(AtlasPositionX[x], AtlasPositionY[y], 200, 200);
    }

    public int GetAtlasPositionX()
    {
        return AtlasPositionX.Length;
    }

}
