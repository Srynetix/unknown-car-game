extends Node2D
class_name Warning

onready var animation_player: AnimationPlayer = $AnimationPlayer

func _ready() -> void:
    animation_player.connect("animation_finished", self, "queue_free")