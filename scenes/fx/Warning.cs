using Godot;

public class Warning : Node2D
{
    public async override void _Ready()
    {
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
        QueueFree();
    }
}
