using Godot;
using FxPlugin;

public class Game : Control
{
    [Export]
    public float TileMapScrollSpeed = 100.0f;
    [Export]
    public int CellRowsToSkip = 4;
    [Export]
    public int InitialRemainingTime = 30;

    private const float KM_H_COEF = 6;

    private Car _Car;
    private TileMap _TileMap;
    private Label _SpeedLabel;
    private Label _ScoreLabel;
    private Label _TimeLabel;
    private Timer _SpawnTimer;
    private Timer _SpawnBlockingTimer;
    private Timer _ChronoTimer;
    private Camera2D _Camera;
    private Node2D _ObstaclesContainer;
    private MotionBlur _MotionBlur;
    private Vignette _Vignette;
    private Shockwave _Shockwave;
    private GameOver _GameOver;
    private AnimationPlayer _TimeAnimationPlayer;

    private Vector2 _TileSize;
    private float _NextTimeout;
    private int _Score;
    private int _RemainingTime;
    private float _BlockingWaitTime = 5.0f;

    public override void _Ready()
    {
        LoadCache.GetInstance().StoreScene<CarCrash>("res://scenes/fx/CarCrash.tscn");
        LoadCache.GetInstance().StoreScene<Sparkle>("res://scenes/fx/Sparkle.tscn");
        LoadCache.GetInstance().StoreScene<Warning>("res://scenes/fx/Warning.tscn");
        LoadCache.GetInstance().StoreScene<Obstacle>("res://scenes/entities/Obstacle.tscn");
        LoadCache.GetInstance().StoreScene<Chronometer>("res://scenes/entities/Chronometer.tscn");
        LoadCache.GetInstance().StoreScene<BlockingObstacle>("res://scenes/entities/BlockingObstacle.tscn");

        _TileMap = GetNode<TileMap>("Background");
        _ScoreLabel = GetNode<Label>("UITop/Score");
        _SpeedLabel = GetNode<Label>("UIBottom/Speed");
        _TimeLabel = GetNode<Label>("UIBottom/Time");
        _SpawnTimer = GetNode<Timer>("SpawnTimer");
        _SpawnBlockingTimer = GetNode<Timer>("SpawnBlockingTimer");
        _ChronoTimer = GetNode<Timer>("ChronoTimer");
        _Camera = GetNode<Camera2D>("Camera");
        _Car = GetNode<Car>("Car");
        _ObstaclesContainer = GetNode<Node2D>("Obstacles");
        _MotionBlur = GetNode<MotionBlur>("MotionBlur");
        _Vignette = GetNode<Vignette>("Vignette");
        _Shockwave = GetNode<Shockwave>("Shockwave");
        _GameOver = GetNode<GameOver>("GameOver");
        _TimeAnimationPlayer = GetNode<AnimationPlayer>("UIBottom/TimeAnimation");

        _SpawnTimer.Connect("timeout", this, nameof(SpawnTimeOut));
        _ChronoTimer.Connect("timeout", this, nameof(ChronoTimeOut));
        _SpawnBlockingTimer.Connect("timeout", this, nameof(BlockingTimeOut));
        _Car.Connect(nameof(Car.crash), this, nameof(GameOver));

        _TileSize = _TileMap.CellSize * _TileMap.Scale;
        _NextTimeout = _SpawnTimer.WaitTime;
        _SpawnBlockingTimer.WaitTime = _BlockingWaitTime;
        _SpawnBlockingTimer.Start();

        var size = GetViewportRect().Size;
        _Car.Position = new Vector2(size.x / 2, size.y - 100);

        _RemainingTime = InitialRemainingTime;
        UpdateTimeLabel();
    }

    public override void _Process(float delta)
    {
        UpdateCar();
        UpdateTileMap(delta);
        UpdateEffects();
    }

    #region Update methods

    private void UpdateTileMap(float delta)
    {
        _TileMap.Position += new Vector2(0, _Car.Speed * delta);

        var limit = CellRowsToSkip * _TileSize;
        if (_TileMap.Position.y >= limit.y)
        {
            _TileMap.Position -= new Vector2(0, limit.y);
        }
    }

    private void UpdateCar()
    {
        _SpeedLabel.Text = $"{(int)(_Car.Speed / KM_H_COEF)} km/h";
        _SpeedLabel.RectPosition = _Car.Position - _SpeedLabel.RectSize / 2;
    }

    private void UpdateEffects()
    {
        float ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        float coef = (float)GD.RandRange(-1.0f, 1.0f) * 2 * ratio;
        _Camera.Offset = new Vector2(coef, coef);

        // Prepare blur depending on car speed
        _MotionBlur.Strength = ratio * 0.015f;
        _Vignette.Ratio = ratio * 0.25f;

        if (ratio == 1.0f)
        {
            if (!_Shockwave.IsRunning())
            {
                var center = _Car.Position / GetViewportRect().Size;
                center = new Vector2(center.x, 1.0f - center.y);
                _Shockwave.Start(center);
            }
        }

        _NextTimeout = 0.25f + ((1.0f - ratio) * 2.0f);
    }

    #endregion

    #region Callbacks

    private void SpawnTimeOut()
    {
        if ((int)GD.RandRange(0, 10) == 0)
        {
            SpawnChronometer();
        }
        else
        {
            SpawnObstacle();
        }

        _SpawnTimer.WaitTime = _NextTimeout;
        _SpawnTimer.Start();
    }

    private void ChronoTimeOut()
    {
        _RemainingTime -= 1;
        UpdateTimeLabel();

        if (_RemainingTime == 0)
        {
            _Car.Crash();
        }
    }

    private async void BlockingTimeOut()
    {
        var size = GetViewportRect().Size;
        var off = 10;
        var x = (float)GD.RandRange(off, size.x - off);

        var inst = LoadCache.GetInstance().InstantiateScene<Warning>();
        inst.Position = new Vector2(x, 50);
        AddChild(inst);

        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
        SpawnBlockingObstacle(inst.Position + Vector2.Up * 150);

        _BlockingWaitTime -= 1.0f;
        _BlockingWaitTime = Mathf.Max(_BlockingWaitTime, 0.1f);
        _SpawnBlockingTimer.WaitTime = _BlockingWaitTime;
        _SpawnBlockingTimer.Start();
    }

    private async void TimePicked()
    {
        _ChronoTimer.Stop();

        _TimeAnimationPlayer.Play("bump");
        await ToSignal(_TimeAnimationPlayer, "animation_finished");

        _ChronoTimer.Start();
    }

    public void AddOneSecondToTimer()
    {
        _RemainingTime += 1;
        UpdateTimeLabel();
    }

    private void UpdateTimeLabel()
    {
        float timeRatio = (float)_RemainingTime / (float)InitialRemainingTime;
        float scaleValue = 1 + (1.0f - timeRatio) * 2;
        _TimeLabel.Text = $"{_RemainingTime}";
        _TimeLabel.RectScale = new Vector2(scaleValue, scaleValue);
    }

    private void AddScore()
    {
        var ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        _Score += (int)(ratio * 1000.0f);
        _ScoreLabel.Text = $"{_Score}$";
    }

    private void GameOver()
    {
        var inst = LoadCache.GetInstance().InstantiateScene<CarCrash>();
        inst.Position = _Car.Position;
        AddChild(inst);

        var center = _Car.Position / GetViewportRect().Size;
        center = new Vector2(center.x, 1.0f - center.y);

        _ChronoTimer.Stop();
        _SpawnTimer.Stop();
        _Shockwave.Start(center);
        _GameOver.Start();
    }

    private void SpawnObstacle()
    {
        var size = GetViewportRect().Size;
        var offset = 100;

        var inst = LoadCache.GetInstance().InstantiateScene<Obstacle>();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);
        inst.Connect(nameof(Obstacle.hit), this, nameof(AddScore));

        _ObstaclesContainer.AddChild(inst);
    }

    private void SpawnBlockingObstacle(Vector2 pos)
    {
        var inst = LoadCache.GetInstance().InstantiateScene<BlockingObstacle>();
        inst.Car = _Car;
        inst.Position = pos;

        _ObstaclesContainer.AddChild(inst);
    }

    private void SpawnChronometer()
    {
        var size = GetViewportRect().Size;
        var offset = 100;

        var inst = LoadCache.GetInstance().InstantiateScene<Chronometer>();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);
        inst.Connect(nameof(Chronometer.picked), this, nameof(TimePicked));

        _ObstaclesContainer.AddChild(inst);
    }

    #endregion
}
