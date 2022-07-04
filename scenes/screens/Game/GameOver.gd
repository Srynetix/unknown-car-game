extends CanvasLayer
class_name GameOver

onready var _restart_button: Button = $Play
onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _better_blur: SxBetterBlur = $BetterBlur

func _ready() -> void:
    _restart_button.connect("pressed", self, "_restart_game")

func start() -> void:
    _better_blur.visible = true
    _animation_player.play("idle")
    _restart_button.grab_focus()

func _restart_game() -> void:
    get_tree().reload_current_scene()