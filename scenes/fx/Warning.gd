extends Node2D
class_name Warning

onready var animation_player: AnimationPlayer = $AnimationPlayer

func _ready() -> void:
    animation_player.connect("animation_finished", self, "_animation_finished")

func _animation_finished(name: String) -> void:
    queue_free()