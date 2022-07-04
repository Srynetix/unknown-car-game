extends Node2D
class_name CarCrash

onready var timer = $Timer

func _ready() -> void:
    timer.connect("timeout", self, "queue_free")