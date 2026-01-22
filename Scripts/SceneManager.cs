using Godot;
using System;
using System.Threading.Tasks;


public partial class SceneManager : GameData
{
    [Export] NodePath _StartUIPath;
    [Export] NodePath _NextUIPath;
    [Export] NodePath _Txt_ScorePath;
    [Export] NodePath _Txt_LevelPath;
    [Export] NodePath _ProgressLevelPath;

    [Export] NodePath _TimerPath;
    [Export] NodePath _GameProgressPath;
    [Export] NodePath _BackgroundPath;

    Node2D StartUI;
    Node2D NextUI;
    CustomSignals _CustomSignals;
    Label Txt_Score;
    Label Txt_Level;

    Timer GameTimer;
    TextureProgress GameProgress;
    ProgressBar ProgressLevel;
    Sprite Background;


    PackedScene BasicScene_Pref;
    public Node2D BasicScene;
    public int BublinaTypeCount;

    int Score = 0;
    int ScoreMax = 300;
    int ElapsedTime = 0;


    Viewport MyViewport;
    Rect2 ViewportRec;

    public override void _Ready()
    {

        BasicScene_Pref = ResourceLoader.Load<PackedScene>("res://scena_1.tscn");
        StartUI = GetNode<Node2D>(_StartUIPath);
        NextUI = GetNode<Node2D>(_NextUIPath);
        Txt_Score = GetNode<Label>(_Txt_ScorePath);
        Txt_Level = GetNode<Label>(_Txt_LevelPath);
        ProgressLevel = GetNode<ProgressBar>(_ProgressLevelPath);

        GameTimer = GetNode<Timer>(_TimerPath);
        GameProgress = GetNode<TextureProgress>(_GameProgressPath);
        Background = GetNode<Sprite>(_BackgroundPath);
        Background.Texture = ResourceLoader.Load<Texture>($"res://Sprites/{LevelsCollection[CurrentLevel].BackgroundName}");

        Background.GlobalPosition = GetViewport().GetVisibleRect().GetCenter();
        Background.Visible = false;

        _CustomSignals = GetNode<CustomSignals>("/root/CustomSignals");
        _CustomSignals.Connect("TimeUpDelegat", this, nameof(TimerUp));
        _CustomSignals.EmitSignal("PlayMusicDelegat", "Intro");

        GetTree().Root.Connect("size_changed", this, "_on_viewport_size_changed");

        MyViewport = GetTree().Root.GetNode("Main").GetNode("ViewportContainer").GetNode<Viewport>("Viewport");

        MyViewport.GetTexture().Flags = (uint)Texture.FlagsEnum.Filter;
        
    }

    public void OnBtnStartGamePressed()
    {
        StartGame();
    }

    public void OnBtnNextGamePressed()
    {
        Score = 0;
        CurrentLevel = 0;
        BublinaTypeCount = LevelsCollection[CurrentLevel].TypeCount;
        _CustomSignals.EmitSignal("PlayMusicDelegat", LevelsCollection[CurrentLevel].MusicName);
        StartUI.Visible = false;
        NextUI.Visible = false;
        BasicScene.GetNode<GameManager>("GameManager").StartScene(LevelsCollection[CurrentLevel].GridSizeX, LevelsCollection[CurrentLevel].GridSizeY);
        ProgressLevel.MaxValue = LevelsCollection[CurrentLevel].LevelUp;
        GameProgress.MaxValue = LevelsCollection[CurrentLevel].TimeMax;
        IsVisibleGameUI(true);
        GameTimer.Start();
    }

    void StartGame()
    {
        Score = 0;
        CurrentLevel = 0;
        BublinaTypeCount = LevelsCollection[CurrentLevel].TypeCount;
        _CustomSignals.EmitSignal("PlayMusicDelegat", LevelsCollection[CurrentLevel].MusicName);
        StartUI.Visible = false;
        NextUI.Visible = false;
        BasicScene = (Node2D)BasicScene_Pref.Instance();
        BasicScene.Name = "Scena1";
        AddChild(BasicScene);
        // nastav landscape, nebo portrait podle velikosti obrazovky
        _on_viewport_size_changed();
        BasicScene.GetNode<GameManager>("GameManager").StartScene(LevelsCollection[CurrentLevel].GridSizeX, LevelsCollection[CurrentLevel].GridSizeY);
        ProgressLevel.MaxValue = LevelsCollection[CurrentLevel].LevelUp;
        GameProgress.MaxValue = LevelsCollection[CurrentLevel].TimeMax;
        // nastav viditelnost herního pozadí
        IsVisibleGameUI(true);
        GameTimer.Start();
    }

    public async void OnTimerTimeout()
    {
        ElapsedTime++;
        GameProgress.Value = ElapsedTime;
        if (ElapsedTime >= LevelsCollection[CurrentLevel].TimeMax)
        {
            await GameOver();
        }
    }

    public void _on_viewport_size_changed()
    {
        Vector2 screenSize = GetTree().Root.Size;
        ViewportContainer MyViewportContainer = GetTree().Root.GetNode("Main").GetNode<ViewportContainer>("ViewportContainer");
        MyViewport = MyViewportContainer.GetNode<Viewport>("Viewport");

        float scale = MyViewport.Size.y / 720;
        bool isPortrait = screenSize.x < screenSize.y;
        Vector2 resolution;

        if (isPortrait)
        {
            resolution = new Vector2(GAME_HEIGHT, GAME_WIDTH);
            MyViewportContainer.RectPosition = new Vector2(280, 0);
            Background.Scale = new Vector2(scale, scale);
        }
        else
        {
            resolution = new Vector2(GAME_WIDTH, GAME_HEIGHT);
            MyViewportContainer.RectPosition = new Vector2(0, 280);
            Background.Scale = new Vector2(1.15f, 1.15f);
        }

        MyViewportContainer.RectSize = resolution;
        MyViewport.Size = resolution;
        GetTree().SetScreenStretch(SceneTree.StretchMode.Mode2d, SceneTree.StretchAspect.Expand, resolution, 1);
        BasicScene.GetNode<GameManager>("GameManager").CalculateGridPosition();
        BasicScene.GetNode<GameManager>("GameManager").Gridview();
        Background.GlobalPosition = new Vector2(MyViewport.Size.x / 2, MyViewport.Size.y / 2);
    }

    // odečte uplynulému času 5 vteřin - časový bonus
    public void TimerUp()
    {
        ElapsedTime -= 5;
        if (ElapsedTime < 0)
        {
            ElapsedTime = 0;
        }
    }

    public async void CheckLevelUp()
    {
        SceneTreeTimer timer;

        if (Score < LevelsCollection[CurrentLevel].LevelUp)
        {
            Score++;
            Txt_Score.Text = Score.ToString();
            ProgressLevel.Value = Score;
            return;
        }

        if (CurrentLevel >= LevelsCollection.Count - 1)
        {
            await GameWin();
            return;
        }

        CurrentLevel++;

        Score = 0;
        ProgressLevel.MaxValue = LevelsCollection[CurrentLevel].LevelUp;
        BublinaTypeCount = LevelsCollection[CurrentLevel].TypeCount;

        GameTimer.Stop();
        ElapsedTime = 0;
        GameProgress.Value = ElapsedTime;
        GameProgress.MaxValue = LevelsCollection[CurrentLevel].TimeMax;

        // ukončí level
        BasicScene.GetNode<GameManager>("GameManager").EndScene();

        // vykreslení Levelu a pozadí podle levelu
        _CustomSignals.EmitSignal("PlayMusicDelegat", LevelsCollection[CurrentLevel].MusicName);
        timer = GetTree().CreateTimer(1f);
        await ToSignal(timer, "timeout");
        timer.Dispose();

        Txt_Level.Text = "Level " + LevelsCollection[CurrentLevel].LevelID;
        Txt_Level.Visible = true;
        Background.Texture = ResourceLoader.Load<Texture>($"res://Sprites/{LevelsCollection[CurrentLevel].BackgroundName}");

        timer = GetTree().CreateTimer(1.5f);
        await ToSignal(timer, "timeout");
        timer.Dispose();
        BasicScene.GetNode<GameManager>("GameManager").StartScene(LevelsCollection[CurrentLevel].GridSizeX, LevelsCollection[CurrentLevel].GridSizeY);

        Txt_Level.Visible = false;
        GameTimer.Start();
    }

    void IsVisibleGameUI(bool isVisible)
    {
        Background.Visible = isVisible;
        //Txt_Score.Visible = isVisible;
        ProgressLevel.Visible = isVisible;
        GameProgress.Visible = isVisible;
    }

    async Task GameOver()
    {
        await ViewStartScene("Time is up!");
    }

    async Task GameWin()
    {
        await ViewStartScene("You won!");
    }

    async Task<bool> ViewStartScene(string infoText)
    {
        SceneTreeTimer timer;
        
        GameTimer.Stop();
        ElapsedTime = 0;
        GameProgress.Value = ElapsedTime;

        BasicScene.GetNode<GameManager>("GameManager").EndScene();
        Txt_Level.Text = infoText;
        Txt_Level.Visible = true;
        IsVisibleGameUI(false);

        timer = GetTree().CreateTimer(3f);
        await ToSignal(timer, "timeout");
        timer.Dispose();

        Txt_Level.Visible = false;
        NextUI.Visible = true;

        // Vykreslí spouštěcí obrazovku podle orientace obrazovky
        ViewportContainer MyViewportContainer = GetTree().Root.GetNode("Main").GetNode<ViewportContainer>("ViewportContainer");
        MyViewport = MyViewportContainer.GetNode<Viewport>("Viewport");

        float scale;
        bool isPortrait = MyViewport.Size.x < MyViewport.Size.y;
        var button = NextUI.GetNode<Button>("BtnNextGame");
        Vector2 buttonOldPosition = button.RectPosition;

        if (isPortrait)
        {
            scale = GAME_HEIGHT / GAME_WIDTH;
            NextUI.Position = new Vector2(0, (MyViewport.Size.y / 2f) - ((GAME_HEIGHT * scale) / 2f));
            NextUI.GetNode<Sprite>("Introbackground").Visible = false;
            NextUI.GetNode<Sprite>("Logo").Visible = false;
            // Zvětší tlačítko dvojnásobně pro portrait režim (lepší ovládání na mobilu) a přepočte jeho pozici na střed obrazovky
            // MyVieport souřadnice jsou přehozeny, nevím proč, ale funguje to
            button.RectScale = new Vector2(2f, 2f);
            button.RectPosition = new Vector2((MyViewport.Size.y / 2f) - ((button.RectSize.x * 2f) / 2f), (MyViewport.Size.x / 2f) - ((button.RectSize.y * 2f) / 2f));
        }
        else
        {
            scale = 1f;
            NextUI.Position = new Vector2(0, 0);
            NextUI.GetNode<Sprite>("Introbackground").Visible = true;
            NextUI.GetNode<Sprite>("Logo").Visible = true;
            NextUI.GetNode<Button>("BtnNextGame").RectScale = new Vector2(1, 1);
            button.RectPosition = new Vector2((MyViewport.Size.x / 2f) - (button.RectSize.x / 2f), (MyViewport.Size.y / 2f) - (button.RectSize.y / 2f)); 
        }

        NextUI.Scale = new Vector2(scale, scale);

        return true;
    }
}
