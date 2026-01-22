using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Bublina : StaticBody2D
{
	public int Type;
	[Export] PackedScene _Particle_Pref;
	[Export] NodePath _SoundDeletePath;
    [Export] NodePath _SoundBombPath;
    [Export] NodePath _AnimationPath;
	[Export] NodePath _Sprite_SprtPath;
	[Export] NodePath _SelectorPath;

	AudioStreamPlayer SoundDelete;
    AudioStreamPlayer SoundBomb;
    AnimationPlayer AnimationBublina;
	Sprite Sprite_Sprt;
	Sprite Selector;
	CustomSignals _CustomSignals;

	AtlasTexture Atlas_sprt = new AtlasTexture();
	public Vector2 PositionInGrid;
	public bool IsMoving;
	public Vector2 Target;
	public bool Destroy;
	GameManager _GameManager;
	GameData _GameData;
	public bool IsDelete;
	int MoveSpeed = 1200;
	public bool IsMoved;

	public override void _Ready()
	{
		_CustomSignals = GetNode<CustomSignals>("/root/CustomSignals");
		_GameData = GetNode<GameData>("/root/GameData");

		SoundDelete = GetNode<AudioStreamPlayer>(_SoundDeletePath);
        SoundBomb = GetNode<AudioStreamPlayer>(_SoundBombPath);
        Sprite_Sprt = GetNode<Sprite>(_Sprite_SprtPath);
		Selector = GetNode<Sprite>(_SelectorPath);
		AnimationBublina = GetNode<AnimationPlayer>(_AnimationPath);

		_GameManager = GetParent().GetNode<GameManager>("/root/Main/ViewportContainer/Viewport/SceneManager/Scena1/GameManager");

		Atlas_sprt.Atlas = GD.Load<Texture>("res://Sprites/minh-truong-bubble-shooter-balls.png");
	}

	public override void _Process(float delta)
	{
		if (this.IsMoving && IsInsideTree())
		{
			this.GlobalPosition = this.GlobalPosition.MoveToward(Target, (float)delta * MoveSpeed);
            //if (this.GlobalPosition == Target || !AnimationBublina.IsPlaying())
            if (this.GlobalPosition == Target)
			{
				this.IsMoving = false;
				// pokud má bublina příznak delete tak ji po dosazeni targetu vymaz ze stromu
				if (IsDelete)
				{
					_GameManager.RemoveChild(this);
				}
			}
		}
	}

    public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
	{
		// pokud bublina neni ve stromu, nebo ma priznak delete tak nelze kliknout
		if(!this.IsInsideTree() || IsDelete)
		{
            @event.Dispose();
            viewport.Dispose();
            return;
        }
			
		if (@event is InputEventMouseButton mouseButton)
		{
			if ((ButtonList)mouseButton.ButtonIndex == ButtonList.Left && mouseButton.Pressed)
			{
                mouseButton.Dispose();
                @event.Dispose();
                viewport.Dispose();
                // pokud žádná bublina není označená, tak označ tuto
                if (!Selector.Visible && _GameManager.SelectedItem == null)
				{
					_GameManager.SelectedItem = this;
					SelectorVisibility();
					return;
				}
				// pokud je tato bublina již označená tak ji odznač
				else if (Selector.Visible && _GameManager.SelectedItem == this)
				{
					_GameManager.SelectedItem = null;
					SelectorVisibility();
					return;
				}
				//pokud je kliknutá bublina vedle označené bubliny můžeš pokračovat, jinak nic
				else if (!Selector.Visible && _GameManager.SelectedItem != null)
				{
                    // souřadnice bublin, kolem označené
                    if (this.PositionInGrid != _GameManager.SelectedItem.PositionInGrid + Vector2.Up
					&& this.PositionInGrid != _GameManager.SelectedItem.PositionInGrid + Vector2.Down
					&& this.PositionInGrid != _GameManager.SelectedItem.PositionInGrid + Vector2.Left
					&& this.PositionInGrid != _GameManager.SelectedItem.PositionInGrid + Vector2.Right)
					{
						return;
					}
					_GameData.SetItemsForChange(_GameManager.SelectedItem, this);

					_GameManager.ChangeItems();
                }
			}
		}
		@event.Dispose();
		viewport.Dispose();
	}

	public void Respawn(Vector2 positionInGrid, Vector2 globalPosition, int typeCount)
	{
        IsDelete = false;
        Destroy = false;
        MoveSpeed = 1200;

		PositionInGrid = positionInGrid;
		GlobalPosition = globalPosition;

        int bonusType = (int)(GD.Randi() % 25);

        if (GameData.LevelsCollection[GameData.CurrentLevel].BonusTimer == true && bonusType == 17)
        {
            Atlas_sprt.Region = _GameData.GetAtlasPositon(5,3);
            Sprite_Sprt.Texture = Atlas_sprt;
            Type = bonusType;
        }
		else if (GameData.LevelsCollection[GameData.CurrentLevel].BonusBomb == true && bonusType == 18)
        {
            Atlas_sprt.Region = _GameData.GetAtlasPositon(3, 3);
            Sprite_Sprt.Texture = Atlas_sprt;
            Type = bonusType;
        }
        else
        {
            int type = (int)(GD.Randi() % typeCount);
            Atlas_sprt.Region = _GameData.GetAtlasPositon(type, 1);
            Sprite_Sprt.Texture = Atlas_sprt;
            Type = type;
        }
        AnimationBublina.Play("Scale_UP");
    }

	public void SetIsMoved(bool isMoved)
	{
		IsMoved = isMoved;
	}

	public void SelectorVisibility()
	{
        Selector.Visible = !Selector.Visible;
    }

	public void Delete()
	{
		// bublina je označena k vymazání
		IsDelete = true;
		// bublina už byla přesunuta
		SetIsMoved(false);
		// pokud je bonus time spusť v SceneManager TimerUp 
		if (Type == 17)
		{
			_CustomSignals.EmitSignal("TimeUpDelegat");
		}
		// pokud je bonus bomb spust vymazani do krize vsech bublin bedle teto
        if (Type == 18)
        {
			BonusBomb();
        }

        // spustí odsun mrtvé bubliny
        MoveSpeed = 200;
		Target = new Vector2(this.GlobalPosition.x, this.GlobalPosition.y - 100);
        IsMoving = true;

		// efekty
		Node2D particle = (Node2D)_Particle_Pref.Instance();
		particle.GlobalPosition = this.GlobalPosition;
		_GameManager.AddChild(particle);
		// nastaveni rychlosti prehravani zvuku aby to nebylo porad stejne
		SoundDelete.PitchScale = (float)GD.RandRange(0.8, 1.2);

        SoundDelete.Play();
		AnimationBublina.Play("Scale_DOWN");
	}

	void BonusBomb()
	{
		for (int x = 0; x < _GameManager.Grid.GetLength(0); x++)
		{
			// pokud je prvek null nebo tento, tak pokračuj jinak vymaz radu
			if(_GameManager.Grid[x, (int)this.PositionInGrid.y] == null
			|| _GameManager.Grid[x, (int)this.PositionInGrid.y] == this
			|| _GameManager.Grid[x, (int)this.PositionInGrid.y].IsDelete == true)
			{
				continue;
			}

			_GameManager.Grid[x, (int)this.PositionInGrid.y].Delete();
            _GameManager.Grid[x, (int)this.PositionInGrid.y] = null;
        }
        for (int y = 0; y < _GameManager.Grid.GetLength(1); y++)
        {
            // pokud je prvek null nebo tento, tak pokračuj jinak vymaz sloupec
            if (_GameManager.Grid[ (int)this.PositionInGrid.x, y] == null
			|| _GameManager.Grid[(int)this.PositionInGrid.x, y] == this
            || _GameManager.Grid[(int)this.PositionInGrid.x, y].IsDelete == true)
            {
                continue;
            }

            _GameManager.Grid[(int)this.PositionInGrid.x, y].Delete();
            _GameManager.Grid[(int)this.PositionInGrid.x, y] = null;
        }

		SoundBomb.Play();
    }

}
