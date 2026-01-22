using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    SceneManager _SceneManager;
    public Bublina[,] Grid = new Bublina[8, 8];
    public Bublina SelectedItem;
    int GridPositionOffsetX;
    int GridPositionOffsetY;
    int ItemsMargin = 65;
    bool IsEnding;
    List<Bublina> volneBubliny = new List<Bublina>();
    List<Bublina> trojice = new List<Bublina>();
    Bublina jednotka = new Bublina();
    SceneTreeTimer timer { get; set; }
    GameData _GameData;
    Vector2 PositionSelectedItemOld;
    int BonusType;
    int pomocny = 0;


    public override void _Ready()
    {
        //System.Diagnostics.Debugger.Break();
        _SceneManager = GetTree().Root.GetNode("Main").GetNode("ViewportContainer").GetNode("Viewport").GetNode<SceneManager>("SceneManager");
        _GameData = GetNode<GameData>("/root/GameData");

        CalculateGridPosition();
        //StartScene(GameData.LevelsCollection[GameData.CurrentLevel].GridSizeX, GameData.LevelsCollection[GameData.CurrentLevel].GridSizeY);
    }

    // spousti scenemanager
    public void StartScene(int gridX, int gridY)
    {
        IsEnding = false;
        Grid = new Bublina[gridX, gridY];
        CalculateGridPosition();
        CheckUpperItem();
    }

    public void EndScene()
    {
        IsEnding = true;
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                if (Grid[x, y] != null)
                {
                    Grid[x, y].Delete();
                    Grid[x, y] = null;
                }
            }
        }
    }

    void GenerateItem(int x, int y)
    {
        volneBubliny = _GameData.BublinaPools.Where(bublina => bublina.IsInsideTree() == false).ToList();
        if (volneBubliny.Count == 0)
        {
            return;
        }

        jednotka = volneBubliny[(int)(GD.Randi() % volneBubliny.Count)];

        int xPosition = (int)(x * ItemsMargin + GridPositionOffsetX);
        int yPosition = (int)(y * ItemsMargin + GridPositionOffsetY);

        AddChild(jednotka);
        jednotka.Respawn(new Vector2(x, y), new Vector2(xPosition, yPosition), _SceneManager.BublinaTypeCount);

        Grid[x, y] = jednotka;
    }

    public void Gridview()
    {
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                if (Grid[x, y] == null)
                    continue;
                if (Grid[x, y].Destroy == true)
                {
                    Grid[x, y].Delete();
                    Grid[x, y] = null;

                    if (!IsEnding)
                    {
                        _SceneManager.CheckLevelUp();
                    }
                    continue;
                }
                int xPosition = (int)(Grid[x, y].PositionInGrid.x * ItemsMargin + GridPositionOffsetX);
                int yPosition = (int)(Grid[x, y].PositionInGrid.y * ItemsMargin + GridPositionOffsetY);
                //Grid[x, y].GetChild<CircleShape2D>(2).GetRadius();
                Grid[x, y].Target = new Vector2(xPosition, yPosition);
                Grid[x, y].IsMoving = true;
            }
        }
    }

    public void SetGridSize(int x, int y)
    {
        Grid = new Bublina[x, y];
    }

    void CheckGrid()
    {
        int itemType = 0;

        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1) - 2; y++)
            {
                if (Grid[x, y] == null)
                {
                    continue;
                }
                itemType = Grid[x, y].Type;

                for (int i = y; i < y + 3; i++)
                {
                    if (Grid[x, i] == null)
                    {
                        continue;
                    }
                    trojice.Add(Grid[x, i]);
                }

                if (trojice.Count == 3)
                {
                    if (trojice.All(prvek => prvek.Type == itemType))
                    {
                        foreach (var item in trojice)
                        {
                            item.Destroy = true;
                        }
                    }
                    else if (itemType == 17)
                    {
                        var ostatniPrvky = trojice.Where(type => type.Type != itemType).ToList();
                        if (ostatniPrvky.All(id => id.Type == ostatniPrvky.First().Type) && trojice.Any(ismoving => ismoving.IsMoved))
                        {
                            foreach (var item in trojice)
                            {
                                item.Destroy = true;
                            }
                        }
                    }
                    else if (itemType == 18)
                    {
                        var ostatniPrvky = trojice.Where(type => type.Type != itemType).ToList();
                        if (ostatniPrvky.All(id => id.Type == ostatniPrvky.First().Type) && trojice.Any(ismoving => ismoving.IsMoved))
                        {
                            foreach (var item in trojice)
                            {
                                item.Destroy = true;
                            }
                        }
                    }
                    else if (trojice.All(prvek => prvek.Type == itemType || prvek.Type == 17 || prvek.Type == 18) && trojice.Any(ismoving => ismoving.IsMoved))
                    {
                        foreach (var item in trojice)
                        {
                            item.Destroy = true;
                        }
                    }
                }
                trojice.Clear();
            }
        }

        for (int x = 0; x < Grid.GetLength(0) - 2; x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                if (Grid[x, y] == null)
                {
                    continue;
                }
                itemType = Grid[x, y].Type;

                for (int i = x; i < x + 3; i++)
                {
                    if (Grid[i, y] == null)
                    {
                        continue;
                    }
                    trojice.Add(Grid[i, y]);
                }

                if (trojice.Count == 3)
                {
                    if (trojice.All(prvek => prvek.Type == itemType))
                    {
                        foreach (var item in trojice)
                        {
                            item.Destroy = true;
                        }
                    }
                    else if (itemType == 17)
                    {
                        var ostatniPrvky = trojice.Where(type => type.Type != itemType).ToList();
                        if (ostatniPrvky.All(id => id.Type == ostatniPrvky.First().Type) && trojice.Any(ismoving => ismoving.IsMoved))
                        {
                            foreach (var item in trojice)
                            {
                                item.Destroy = true;
                            }
                        }
                    }
                    else if (itemType == 18)
                    {
                        var ostatniPrvky = trojice.Where(type => type.Type != itemType).ToList();
                        if (ostatniPrvky.All(id => id.Type == ostatniPrvky.First().Type) && trojice.Any(ismoving => ismoving.IsMoved))
                        {
                            foreach (var item in trojice)
                            {
                                item.Destroy = true;
                            }
                        }
                    }
                    else if (trojice.All(prvek => prvek.Type == itemType || prvek.Type == 17 || prvek.Type == 18) && trojice.Any(ismoving => ismoving.IsMoved))
                    {
                        foreach (var item in trojice)
                        {
                            item.Destroy = true;
                        }
                    }
                }
                trojice.Clear();
            }
        }

        SetAllItemsUnmvoned();
    }

    void SetAllItemsUnmvoned()
    {
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                if (Grid[x, y] == null)
                    continue;
                Grid[x, y].SetIsMoved(false);
            }
        }
    }

    bool IsGridFull()
    {
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                if (Grid[x, y] == null)
                    return false;
            }
        }
        return true;
    }

    public void ChangeItems()
    {
        SelectedItem = null;

        PositionSelectedItemOld = _GameData.ItemsForChange[0].PositionInGrid;
        //Vector2 selectedItem = item1.PositionInGrid;

        Grid[(int)_GameData.ItemsForChange[1].PositionInGrid.x, (int)_GameData.ItemsForChange[1].PositionInGrid.y] = _GameData.ItemsForChange[0];
        Grid[(int)_GameData.ItemsForChange[0].PositionInGrid.x, (int)_GameData.ItemsForChange[0].PositionInGrid.y] = _GameData.ItemsForChange[1];
        _GameData.ItemsForChange[0].PositionInGrid = _GameData.ItemsForChange[1].PositionInGrid;
        _GameData.ItemsForChange[1].PositionInGrid = PositionSelectedItemOld;

        _GameData.ItemsForChange[0].SelectorVisibility();
        //Gridview();
        CheckUpperItem();
    }

    public void CalculateGridPosition()
    {
        int widthGrid = (Grid.GetLength(0) - 1) * ItemsMargin;
        int heightGrid = (Grid.GetLength(1) - 1) * ItemsMargin;
        Vector2 cameraCenter = GetViewport().GetVisibleRect().GetCenter();
        GridPositionOffsetX = (int)cameraCenter.x - (widthGrid / 2);
        GridPositionOffsetY = (int)cameraCenter.y - (heightGrid / 2);
    }


    public async void CheckUpperItem()
    {
        do
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = Grid.GetLength(1) - 1; y >= 0; y--)
                {
                    Gridview();
                    if (Grid[x, y] == null)
                    {
                        // Pokud jsi našel prázdné pole, posuň všechna pole nad tímto polem o jednu dolů
                        for (int sloupec = y; sloupec > 0; sloupec--)
                        {
                            if (IsEnding)
                                return;
                            Grid[x, sloupec] = Grid[x, sloupec - 1];
                            if (Grid[x, sloupec] != null)
                            {
                                Grid[x, sloupec].PositionInGrid = new Vector2(x, sloupec);
                            }
                        }
                        Grid[x, 0] = null;
                        //Gridview();
                        // Vygeneruj novou bublinu na horním řádku sloupce, pokud je horní řádek prázdný
                        GenerateItem(x, 0);
                    }

                }
            }

            CheckGrid();
            //timer = GetTree().CreateTimer(0.2f);
            //await ToSignal(timer, "timeout");
            //timer.Dispose();
            Gridview();
            timer = GetTree().CreateTimer(0.2f);
            await ToSignal(timer, "timeout");
            timer.Dispose();
            GC.Collect();
        } while (!IsGridFull() && !IsEnding);
        //GD.Print("Pryč ze smyčky");
    }
}
