using Godot;

public class Splash : Control
{
    private bool _WillLoadGame;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            if (key.Scancode == (int)KeyList.Enter)
            {
                LoadGame();
            }
        }

        else if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed)
            {
                LoadGame();
            }
        }
    }

    private async void LoadGame()
    {
        if (!_WillLoadGame)
        {
            _WillLoadGame = true;
            var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animPlayer.Play("fade");

            await ToSignal(animPlayer, "animation_finished");
            GetTree().ChangeScene("res://scenes/screens/Game.tscn");
        }
    }
}
