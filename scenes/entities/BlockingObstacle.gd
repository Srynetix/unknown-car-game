extends Obstacle
class_name BlockingObstacle

func _ready() -> void:
    initial_speed = 0

func _car_collision(target_car: Car) -> void:
    target_car.crash()

func _move_after_collision() -> void:
    pass
