using Godot;

public class GameOver : CanvasLayer
{
    private Button _RestartButton;
    private AnimationPlayer _AnimationPlayer;
    private GaussianBlur _GaussianBlur;

    public override void _Ready()
    {
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _RestartButton = GetNode<Button>("Play");
        _GaussianBlur = GetNode<GaussianBlur>("GaussianBlur");

        _RestartButton.Connect("pressed", this, nameof(RestartGame));
    }

    public void Start()
    {
        _GaussianBlur.Visible = true;
        _AnimationPlayer.Play("idle");
        _RestartButton.GrabFocus();
    }

    private void RestartGame()
    {
        GetTree().ReloadCurrentScene();
    }
}
