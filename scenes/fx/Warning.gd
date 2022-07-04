extends Node2D
class_name Warning

onready var animation_player: AnimationPlayer = $AnimationPlayer

func _ready() -> void:
    yield(animation_player, "animation_finished")
    queue_free()