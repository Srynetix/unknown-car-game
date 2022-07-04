extends Node2D
class_name CarCrash

func _ready() -> void:
    yield(get_tree().create_timer(0.75), "timeout")
    queue_free()