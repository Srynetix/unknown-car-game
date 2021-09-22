using Godot;

public class BlockingObstacle : Obstacle
{
    public override void _Ready()
    {
        base._Ready();

        _Velocity = Vector2.Zero;
    }

    protected override void CarCollision(Car car) {
        car.Crash();
    }

    protected override void ObstacleCollision(Obstacle obs) {
        var half = (Position - obs.Position) / 2;
        SpawnSparkles(Position + half);
    }
}
