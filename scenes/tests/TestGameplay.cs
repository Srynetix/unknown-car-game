using Godot;

public class TestGameplay : Control
{
    [Export]
    public float TileMapScrollSpeed = 100.0f;
    [Export]
    public int CellRowsToSkip = 4;

    private const float KM_H_COEF = 6;

    private Car _Car;
    private TileMap _TileMap;
    private Label _SpeedLabel;
    private Label _ScoreLabel;
    private Timer _Timer;
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

    public override void _Ready()
    {
        _TileMap = GetNode<TileMap>("TileMap");
        _SpeedLabel = GetNode<Label>("CanvasLayer/Speed");
        _ScoreLabel = GetNode<Label>("GameOver/Score");
        _Timer = GetNode<Timer>("Timer");
        _Camera = GetNode<Camera2D>("Camera");
        _Car = GetNode<Car>("Car");
        _ObstaclesContainer = GetNode<Node2D>("Obstacles");
        _MotionBlur = GetNode<MotionBlur>("MotionBlur");
        _Vignette = GetNode<Vignette>("Vignette");
        _Shockwave = GetNode<Shockwave>("Shockwave");
        _AnimationPlayer = GetNode<AnimationPlayer>("GameOver/AnimationPlayer");
        _RestartButton = GetNode<Button>("GameOver/Play");

        _TileSize = _TileMap.CellSize * _TileMap.Scale;
        _Timer.Connect("timeout", this, nameof(TimeOut));
        _NextTimeout = _Timer.WaitTime;
        _Car.Connect(nameof(Car.crash), this, nameof(GameOver));
        _RestartButton.Connect("pressed", this, nameof(RestartGame));

        var size = GetViewportRect().Size;
        _Car.Position = new Vector2(size.x / 2, size.y - 100);
    }

    public override void _Process(float delta)
    {
        UpdateCar(delta);
        UpdateTileMap(delta);
        UpdateCamera(delta);
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

    private void UpdateCamera(float delta) {
        float ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        float coef = (float)GD.RandRange(-1.0f, 1.0f) * 2 * ratio;
        _Camera.Offset = new Vector2(coef, coef);

        // Prepare blur depending on car speed
        // Best range in (0, 0.015)
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
        if ((int)GD.RandRange(0, 10) == 0) {
            SpawnBlockingObstacle();
        } else {
            SpawnObstacle();
        }

        _Timer.WaitTime = _NextTimeout;
        _Timer.Start();
    }

    private void CarHit() {
        var ratio = _Car.Speed / _Car.CarMaxForwardSpeed;
        _Score += (int)(ratio * 1000.0f);
        _ScoreLabel.Text = $"{_Score}$";
    }

    private void GameOver() {
        GetNode<GaussianBlur>("GameOver/GaussianBlur").Visible = true;
        _AnimationPlayer.Play("idle");
        _RestartButton.GrabFocus();
    }

    private void SpawnObstacle() {
        var size = GetViewportRect().Size;
        var offset = 100;

        var scene = GD.Load<PackedScene>("res://scenes/tests/Obstacle.tscn");
        var inst = (Obstacle)scene.Instance();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);
        inst.Connect(nameof(Obstacle.hit), this, nameof(CarHit));

        _ObstaclesContainer.AddChild(inst);
    }

    private void SpawnBlockingObstacle() {
        var size = GetViewportRect().Size;
        var offset = 100;

        var scene = GD.Load<PackedScene>("res://scenes/tests/BlockingObstacle.tscn");
        var inst = (BlockingObstacle)scene.Instance();
        inst.Car = _Car;
        inst.Position = new Vector2((float)GD.RandRange(offset, size.x - offset), -100);

        _ObstaclesContainer.AddChild(inst);
    }

    private void RestartGame() {
        GetTree().ReloadCurrentScene();
    }
}
