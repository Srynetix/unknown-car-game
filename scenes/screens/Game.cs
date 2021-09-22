using Godot;

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
    private Timer _ChronoTimer;
    private Camera2D _Camera;
    private Node2D _ObstaclesContainer;
    private MotionBlur _MotionBlur;
    private Vignette _Vignette;
    private Shockwave _Shockwave;
    private AnimationPlayer _AnimationPlayer;
    private Button _RestartButton;

    private Vector2 _TileSize;
    private float _NextTimeout;
    private int _Score;
    private int _RemainingTime;

    public override void _Ready()
    {
        LoadCache.GetInstance().StoreScene("CarCrash", "res://scenes/fx/CarCrash.tscn");
        LoadCache.GetInstance().StoreScene("Sparkle", "res://scenes/fx/Sparkle.tscn");
        LoadCache.GetInstance().StoreScene("Obstacle", "res://scenes/entities/Obstacle.tscn");
        LoadCache.GetInstance().StoreScene("Chronometer", "res://scenes/entities/Chronometer.tscn");
        LoadCache.GetInstance().StoreScene("BlockingObstacle", "res://scenes/entities/BlockingObstacle.tscn");

        _TileMap = GetNode<TileMap>("TileMap");
        _SpeedLabel = GetNode<Label>("CanvasLayer/Speed");
        _ScoreLabel = GetNode<Label>("GameOver/Score");
        _TimeLabel = GetNode<Label>("CanvasLayer/Time");
        _SpawnTimer = GetNode<Timer>("SpawnTimer");
        _ChronoTimer = GetNode<Timer>("ChronoTimer");
        _Camera = GetNode<Camera2D>("Camera");
        _Car = GetNode<Car>("Car");
        _ObstaclesContainer = GetNode<Node2D>("Obstacles");
        _MotionBlur = GetNode<MotionBlur>("MotionBlur");
        _Vignette = GetNode<Vignette>("Vignette");
        _Shockwave = GetNode<Shockwave>("Shockwave");
        _AnimationPlayer = GetNode<AnimationPlayer>("GameOver/AnimationPlayer");
        _RestartButton = GetNode<Button>("GameOver/Play");

        _TileSize = _TileMap.CellSize * _TileMap.Scale;
        _SpawnTimer.Connect("timeout", this, nameof(TimeOut));
        _NextTimeout = _SpawnTimer.WaitTime;
        _ChronoTimer.Connect("timeout", this, nameof(ChronoTimeOut));
        _Car.Connect(nameof(Car.crash), this, nameof(GameOver));
        _RestartButton.Connect("pressed", this, nameof(RestartGame));

        var size = GetViewportRect().Size;
        _Car.Position = new Vector2(size.x / 2, size.y - 100);

        _RemainingTime = InitialRemainingTime;
        _TimeLabel.Text = $"{_RemainingTime}";
    }

    public override void _Process(float delta)
    {
        UpdateCar(delta);
        UpdateTileMap(delta);
        UpdateEffects(delta);
    }

    private void UpdateTileMap(float delta) {
        _TileMap.Position += new Vector2(0, _Car.Speed * delta);

        var limit = CellRowsToSkip * _TileSize;
        if (_TileMap.Position.y >= limit.y) {
            _TileMap.Position -= new Vector2(0, limit.y);
        }
    }

    private void UpdateCar(float delta) {
        _SpeedLabel.Text = $"{(int)(_Car.Speed / KM_H_COEF)} km/h";
        _SpeedLabel.RectPosition = _Car.Position - _SpeedLabel.RectSize / 2;
    }

    private void UpdateEffects(float delta) {
        float ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        float coef = (float)GD.RandRange(-1.0f, 1.0f) * 2 * ratio;
        _Camera.Offset = new Vector2(coef, coef);

        // Prepare blur depending on car speed
        _MotionBlur.Strength = ratio * 0.015f;
        _Vignette.Ratio = ratio * 0.25f;

        if (ratio == 1.0f) {
            if (!_Shockwave.IsRunning()) {
                var center = _Car.Position / GetViewportRect().Size;
                center = new Vector2(center.x, 1.0f - center.y);
                _Shockwave.Start(center);
            }
        }

        _NextTimeout = 0.15f + ((1.0f - ratio) * 2.0f);
    }

    private void TimeOut() {
        if ((int)GD.RandRange(0, 20) == 0) {
            SpawnBlockingObstacle();
        } else if ((int)GD.RandRange(0, 20) == 0) {
            SpawnChronometer();
        } else {
            SpawnObstacle();
        }

        _SpawnTimer.WaitTime = _NextTimeout;
        _SpawnTimer.Start();
    }

    private void ChronoTimeOut() {
        _RemainingTime -= 1;
        UpdateTimeLabel();

        if (_RemainingTime == 0) {
            _Car.Crash();
        }
    }

    private void TimePicked() {
        _RemainingTime += 3;
        UpdateTimeLabel();
    }

    private void UpdateTimeLabel() {
        float timeRatio = (float)_RemainingTime / (float)InitialRemainingTime;
        float scaleValue = 1 + (1.0f - timeRatio) * 2;
        _TimeLabel.Text = $"{_RemainingTime}";
        _TimeLabel.RectScale = new Vector2(scaleValue, scaleValue);
    }

    private void CarHit() {
        var ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        _Score += (int)(ratio * 1000.0f);
        _ScoreLabel.Text = $"{_Score}$";
    }

    private void GameOver() {
        var scene = LoadCache.GetInstance().LoadScene("CarCrash");
        var inst = scene.Instance<CarCrash>();
        inst.Position = _Car.Position;
        AddChild(inst);

        var center = _Car.Position / GetViewportRect().Size;
        center = new Vector2(center.x, 1.0f - center.y);

        _ChronoTimer.Stop();
        _SpawnTimer.Stop();
        _Shockwave.Start(center);

        GetNode<GaussianBlur>("GameOver/GaussianBlur").Visible = true;
        _AnimationPlayer.Play("idle");
        _RestartButton.GrabFocus();
    }

    private void SpawnObstacle() {
        var size = GetViewportRect().Size;
        var offset = 100;

        var scene = LoadCache.GetInstance().LoadScene("Obstacle");
        var inst = scene.Instance<Obstacle>();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);
        inst.Connect(nameof(Obstacle.hit), this, nameof(CarHit));

        _ObstaclesContainer.AddChild(inst);
    }

    private void SpawnBlockingObstacle() {
        var size = GetViewportRect().Size;
        var offset = 10;

        var scene = LoadCache.GetInstance().LoadScene("BlockingObstacle");
        var inst = scene.Instance<BlockingObstacle>();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);

        _ObstaclesContainer.AddChild(inst);
    }

    private void SpawnChronometer() {
        var size = GetViewportRect().Size;
        var offset = 100;

        var scene = LoadCache.GetInstance().LoadScene("Chronometer");
        var inst = scene.Instance<Chronometer>();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);
        inst.Connect(nameof(Chronometer.picked), this, nameof(TimePicked));

        _ObstaclesContainer.AddChild(inst);
    }

    private void RestartGame() {
        GetTree().ReloadCurrentScene();
    }
}
