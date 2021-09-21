using Godot;

public class CarCrash : Node2D
{
    public async override void _Ready() {
        await ToSignal(GetTree().CreateTimer(0.75f), "timeout");
        QueueFree();
    }
}
