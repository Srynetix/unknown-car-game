using Godot;

public class Obstacle : KinematicBody2D {
    [Signal]
    public delegate void hit();

    [Export]
    public float InitialSpeed = -50;

    public Car Car;

    protected Vector2 _Impulse;
    protected Vector2 _Velocity;
    protected float _AngularVelocity;
    private bool _Hit;

    public override void _Ready()
    {
        // Connect("body_entered", this, nameof(AreaEntered));
        _Velocity = new Vector2(0, InitialSpeed);
        _AngularVelocity = (float)GD.RandRange(-10.0f, 10.0f);
        RotationDegrees = _AngularVelocity;
    }

    public override void _Process(float delta)
    {
        var size = GetViewportRect().Size;
        _Velocity = new Vector2(0, Car.Speed * delta) + _Impulse;

        if (!_Hit) {
            var col = MoveAndCollide(_Velocity);
            if (col != null) {
                if (col.Collider is Car car) {
                    _Hit = true;
                    CarCollision(car);
                } else if (col.Collider is Obstacle obs) {
                    _Hit = true;
                    ObstacleCollision(obs);
                }
            }
        } else {
            _Velocity = MoveAndSlide(_Velocity);
        }

        if (Position.y > size.y + 200 || Position.y < -200 || Position.x < -200 || Position.x > size.x + 200) {
            QueueFree();
        }
    }

    private void AreaEntered(PhysicsBody2D other) {
        if (_Hit) {
            return;
        }

        if (other is Car car) {
            _Hit = true;
            CarCollision(car);
        }

        else if (other is Obstacle obs) {
            _Hit = true;
            ObstacleCollision(obs);
        }
    }

    protected virtual void CarCollision(Car car) {
        var ratio = car.Speed / car.CarMaxForwardSpeed;
        var dir = (Position - car.Position).Normalized();
        var half = (Position - car.Position) / 2;
        _Impulse = dir * ratio * 500;
        SpawnSparkles(Position + half);

        EmitSignal(nameof(hit));
    }

    protected virtual void ObstacleCollision(Obstacle obs) {
        var dir = (Position - obs.Position).Normalized();
        var half = (Position - obs.Position) / 2;
        _Impulse = dir * 500;
        SpawnSparkles(Position + half);
    }

    protected void SpawnSparkles(Vector2 pos) {
        var scene = LoadCache.GetInstance().LoadScene("Sparkle");
        var inst = scene.Instance<Sparkle>();

        inst.Position = pos;
        GetParent().AddChild(inst);
    }
}
