public class BlockingObstacle : Obstacle
{
    public override void _Ready()
    {
        base._Ready();

        InitialSpeed = 0;
    }

    protected override void CarCollision(Car car)
    {
        car.Crash();
    }
}
