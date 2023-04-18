extends Obstacle

signal picked()

func _car_collision(_car: Car) -> void:
    emit_signal(picked.get_name())
    queue_free()