extends Control

onready var _animation_player: AnimationPlayer = $AnimationPlayer
var _will_load_game := false

func _input(event: InputEvent) -> void:
    if event is InputEventKey:
        var key_event: InputEventKey = event
        if key_event.pressed && key_event.scancode == KEY_ENTER:
            _load_game()

    elif event is InputEventScreenTouch:
        var touch_event: InputEventScreenTouch = event
        if touch_event.pressed:
            _load_game()

func _load_game() -> void:
    if !_will_load_game:
        _will_load_game = true
        _animation_player.play("fade")

        yield(_animation_player, "animation_finished")
        get_tree().change_scene("res://scenes/screens/Game.tscn")