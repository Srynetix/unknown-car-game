using Godot;

public class Warning : Node2D
{
    public async override void _Ready()
    {
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();
    }
}
