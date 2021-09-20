using Godot;
using System;

public class TestGameplay : Node2D
{
    public class Obstacle : Area2D {
        public Car Car;

        private Sprite _Sprite;

        public override void _Ready()
        {
            _Sprite = new Sprite();
            _Sprite.Texture = GD.Load<Texture>("res://assets/packs/kenney_racingpack_updated/PNG/Objects/barrel_red.png");
            AddChild(_Sprite);

            var size = GetViewportRect().Size;
            Position = new Vector2((float)GD.RandRange(0, size.x), 0);
        }

        public override void _Process(float delta)
        {
            var size = GetViewportRect().Size;
            Position += new Vector2(0, Car.Speed * delta); 

            if (Position.y > size.y) {
                QueueFree();
            }
        }
    }

    public class Car : Area2D {
        [Export]
        public float CarTurnSpeed = 70.0f;
        [Export]
        public float CarForwardSpeed = 200.0f;
        [Export]
        public float CarBrakeSpeed = 100.0f;
        
        [Export]
        public float CarMaxForwardSpeed = 1500.0f;
        [Export]
        public float CarMinForwardSpeed = 10.0f;
        [Export]
        public float CarMaxAngle = 50.0f;
        
        public float Speed {
            get => _CarSpeed;
            set {
                _CarSpeed = value;
            }
        }

        private Sprite _Sprite;
        private float _CarSpeed;
        private int _lastTouchIdx = -1;

        public override void _Ready()
        {
            _Sprite = new Sprite() {
                Texture = GD.Load<Texture>("res://assets/packs/kenney_racingpack_updated/PNG/Cars/car_red_3.png"),
                Scale = new Vector2(0.5f, 0.5f)
            };
            AddChild(_Sprite);

            _CarSpeed = CarMinForwardSpeed;
        }

        public override void _Input(InputEvent @event)
        {
            var delta = GetProcessDeltaTime();

            if (@event is InputEventScreenTouch touch) {
                if (touch.Pressed && _lastTouchIdx == -1) {
                    _lastTouchIdx = touch.Index;
                }

                else if (!touch.Pressed && _lastTouchIdx == touch.Index) {
                    _lastTouchIdx = -1;
                }
            }

            if (@event is InputEventScreenDrag drag) {
                if (_lastTouchIdx == drag.Index) {
                    var relative = drag.Relative;
                    if (relative.x < 0) {
                        RotationDegrees += -CarTurnSpeed * delta;
                    } else if (relative.x > 0) {
                        RotationDegrees += CarTurnSpeed * delta;
                    }

                    if (relative.y < 0) {
                        _CarSpeed += CarForwardSpeed * delta;
                    } else if (relative.y > 0) {
                        _CarSpeed += -CarForwardSpeed * delta;
                    }
                }
            }
        }

        public override void _Process(float delta)
        {
            var size = GetViewportRect().Size;

            if (Input.IsActionPressed("move_left")) {
                RotationDegrees += -CarTurnSpeed * delta;
            }

            if (Input.IsActionPressed("move_right")) {
                RotationDegrees += CarTurnSpeed * delta;
            }

            if (Input.IsActionPressed("move_up")) {
                _CarSpeed += CarForwardSpeed * delta;
            }

            if (Input.IsActionPressed("move_down")) {
                _CarSpeed -= CarForwardSpeed * delta;
            }

            _CarSpeed = Mathf.Min(_CarSpeed, CarMaxForwardSpeed);
            _CarSpeed = Mathf.Max(_CarSpeed, CarMinForwardSpeed);
            RotationDegrees = Mathf.Min(RotationDegrees, CarMaxAngle);
            RotationDegrees = Mathf.Max(RotationDegrees, -CarMaxAngle);

            var x = Position.x + _CarSpeed * Rotation * delta;
            x = Mathf.Min(x, size.x);
            x = Mathf.Max(x, 0);

            Position = new Vector2(x, Position.y);
        }
    }

    [Export]
    public float TileMapScrollSpeed = 100.0f;
    [Export]
    public int CellRowsToSkip = 4;

    private const float KM_H_COEF = 12;

    private Car _Car;
    private TileMap _TileMap;
    private Label _SpeedLabel;
    private Timer _Timer;
    private Camera2D _Camera;

    private Vector2 _TileSize;

    public override void _Ready()
    {
        _TileMap = GetNode<TileMap>("TileMap");
        _SpeedLabel = GetNode<Label>("CanvasLayer/Speed");
        _Timer = GetNode<Timer>("Timer");
        _Camera = GetNode<Camera2D>("Camera");

        _TileSize = _TileMap.CellSize * _TileMap.Scale;
        _Timer.Connect("timeout", this, nameof(SpawnObstacle));

        _Car = new Car();
        AddChild(_Car);

        var size = GetViewportRect().Size;
        _Car.Position = new Vector2(size.x / 2, size.y - 50);
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
        float coef = (float)GD.RandRange(-1.0f, 1.0f) * 5 * (_Car.Speed / _Car.CarMaxForwardSpeed);
        _Camera.Offset = new Vector2(coef, coef);
    }

    private void SpawnObstacle() {
        var obs = new Obstacle() {
            Car = _Car
        };

        AddChild(obs);
    }
}
