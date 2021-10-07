using Godot;
using SxGD;

public class GameOver : CanvasLayer
{
    private Button _RestartButton;
    private AnimationPlayer _AnimationPlayer;
    private BetterBlur _BetterBlur;

    public override void _Ready()
    {
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _RestartButton = GetNode<Button>("Play");
        _BetterBlur = GetNode<BetterBlur>("BetterBlur");

        _RestartButton.Connect("pressed", this, nameof(RestartGame));
    }

    public void Start()
    {
        _BetterBlur.Visible = true;
        _AnimationPlayer.Play("idle");
        _RestartButton.GrabFocus();
    }

    private void RestartGame()
    {
        GetTree().ReloadCurrentScene();
    }
}
