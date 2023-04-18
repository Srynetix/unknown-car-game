extends CharacterBody2D
class_name Obstacle

signal hit()

@export var initial_speed := -50.0
@export var initial_max_rotation := 10.0

var car: Car

var _impulse := Vector2.ZERO
var _angular_velocity := 0.0
var _hit := false

func _ready() -> void:
    rotation_degrees = randf_range(-initial_max_rotation, initial_max_rotation)

func _process(delta: float) -> void:
    var size = get_viewport_rect().size
    velocity = Vector2(0, car.speed * delta) + _impulse + Vector2.DOWN * initial_speed * delta

    if !_hit:
        var collision := move_and_collide(velocity)
        if collision != null:
            var collider = collision.get_collider()
            if collider is Car:
                var target_car: Car = collider
                _hit = true
                _car_collision(target_car)
    else:
        _move_after_collision()

    rotation_degrees += _angular_velocity

    if position.y > size.y + 200 || position.y < -200 || position.x > size.x + 200:
        queue_free()

func _move_after_collision():
    move_and_slide()

func _car_collision(target_car: Car) -> void:
    var ratio = target_car.speed / target_car.max_forward_speed
    var diff = position - target_car.position
    var dir = diff.normalized()
    var half = (position - target_car.position) / 2
    _impulse = dir * ratio * 500
    _spawn_sparkles(position + half)

    var dot = dir.dot(Vector2.RIGHT)
    _angular_velocity = ratio * 10 if dot > 0 else ratio * -10 

    emit_signal("hit")

func _spawn_sparkles(pos: Vector2) -> void:
    var inst: Sparkle = GameLoadCache.instantiate_scene("Sparkle")
    inst.position = pos
    get_parent().add_child(inst)
