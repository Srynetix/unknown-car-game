using Godot;

public class Chronometer : Obstacle
{
    [Signal]
    public delegate void picked();

    protected override void ObstacleCollision(Obstacle obs) {
    }

    protected override void CarCollision(Car car)
    {
        EmitSignal(nameof(picked));
        QueueFree();
    }
}
