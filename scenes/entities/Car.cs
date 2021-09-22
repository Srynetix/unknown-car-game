using Godot;

public class Car : KinematicBody2D {
    [Signal]
    public delegate void crash();

    [Export]
    public float CarTurnSpeed = 70.0f;
    [Export]
    public float CarForwardSpeed = 400.0f;
    [Export]
    public float CarBrakeSpeed = 100.0f;

    [Export]
    public float CarMaxForwardSpeed = 1500.0f;
    [Export]
    public float CarMinForwardSpeed = 200.0f;
    [Export]
    public float CarMaxAngle = 50.0f;

    public float Speed {
        get => _CarSpeed;
        set {
            _CarSpeed = value;
        }
    }

    private Sprite _Sprite;
    private bool _Crashed;
    private CPUParticles2D _Particles;
    private bool _MovingForward;

    private float _CarSpeed;
    private int _lastTouchIdx = -1;

    public override void _Ready()
    {
        _Sprite = GetNode<Sprite>("Sprite");
        _Particles = GetNode<CPUParticles2D>("Particles");

        _CarSpeed = CarMinForwardSpeed;
    }

    public override void _Input(InputEvent @event)
    {
        if (_Crashed) {
            return;
        }

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
                var ratio = (relative / GetViewportRect().Size).Abs() * 100;

                if (relative.x < 0) {
                    RotationDegrees += -CarTurnSpeed * ratio.x * delta;
                } else if (relative.x > 0) {
                    RotationDegrees += CarTurnSpeed * ratio.x * delta;
                }
            }
        }

        if (_lastTouchIdx != -1) {
            _MovingForward = true;
        } else {
            _MovingForward = false;
        }
    }

    private void HandleInput(float delta) {
        if (Input.IsActionPressed("move_left")) {
            RotationDegrees += -CarTurnSpeed * delta;
        }

        if (Input.IsActionPressed("move_right")) {
            RotationDegrees += CarTurnSpeed * delta;
        }

        if (Input.IsActionPressed("move_up")) {
            _MovingForward = true;
        } else if (_lastTouchIdx == -1) {
            _MovingForward = false;
        }

        if (_MovingForward) {
            _CarSpeed += CarForwardSpeed * delta;
        } else {
            _CarSpeed += -CarForwardSpeed * delta;
        }

        _CarSpeed = Mathf.Min(_CarSpeed, CarMaxForwardSpeed);
        _CarSpeed = Mathf.Max(_CarSpeed, CarMinForwardSpeed);
        RotationDegrees = Mathf.Min(RotationDegrees, CarMaxAngle);
        RotationDegrees = Mathf.Max(RotationDegrees, -CarMaxAngle);
    }

    private void HandleMovement(float delta) {
        var size = GetViewportRect().Size;
        var ratio = _CarSpeed / CarMaxForwardSpeed;

        var x = Position.x + _CarSpeed * Rotation * delta;
        x = Mathf.Min(x, size.x);
        x = Mathf.Max(x, 0);

        var y = 50 + (ratio * (size.y / 4 - 50));
        y = size.y - y;

        var offset = new Vector2(x, y) - Position;
        MoveAndCollide(offset);
    }

    private void HandleEffects() {
        var ratio = _CarSpeed / CarMaxForwardSpeed;
        var color = _Particles.Modulate;
        color.a = ratio;
        _Particles.Modulate = color;
        _Particles.Damping = (1.0f - ratio) * 50.0f;
        _Particles.RadialAccel = ratio * 100;
    }

    public override void _Process(float delta)
    {
        if (_Crashed) {
            RotationDegrees += CarTurnSpeed * delta * 5;
        } else {
            HandleInput(delta);
            HandleMovement(delta);
        }

        HandleEffects();
    }

    public void Crash() {
        if (!_Crashed) {
            _Sprite.Visible = false;
            _Crashed = true;
            _CarSpeed = 0;

            EmitSignal(nameof(crash));
        }
    }
}
