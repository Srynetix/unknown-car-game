extends Node2D
class_name Warning

@onready var animation_player: AnimationPlayer = $AnimationPlayer

func _ready() -> void:
    animation_player.animation_finished.connect(_animation_finished)

func _animation_finished(_name: String) -> void:
    queue_free()