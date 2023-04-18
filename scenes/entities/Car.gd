extends CharacterBody2D
class_name Car

signal crashed()

@export var turn_speed := 70.0
@export var forward_speed := 400.0
@export var brake_speed := 100.0
@export var max_forward_speed := 1500.0
@export var min_forward_speed := 200.0
@export var max_angle := 50.0
@export var touch_input_turn_amount := 100.0

var speed = 0

@onready var _sprite: Sprite2D = $Sprite
@onready var _particles: CPUParticles2D = $Particles

var _moving_forward := false
var _crashed := false
var _last_touch_idx := -1

func _ready() -> void:
    speed = min_forward_speed

func _input(event: InputEvent) -> void:
    if _crashed:
        return

    var size = get_viewport_rect().size

    var delta = get_process_delta_time()
    if event is InputEventScreenTouch:
        var touch_event: InputEventScreenTouch = event
        if touch_event.pressed && _last_touch_idx == -1:
            _last_touch_idx = touch_event.index
        elif !touch_event.pressed && _last_touch_idx == touch_event.index:
            _last_touch_idx = -1

    elif event is InputEventScreenDrag:
        var drag_event: InputEventScreenDrag = event
        if _last_touch_idx == drag_event.index:
            var relative = drag_event.relative
            var ratio = (relative / size).abs() * touch_input_turn_amount

            if relative.x < 0:
                rotation_degrees -= turn_speed * ratio.x * delta
            elif relative.x > 0:
                rotation_degrees += turn_speed * ratio.x * delta

    _moving_forward = _last_touch_idx != -1

func _process(delta: float) -> void:
    if _crashed:
        rotation_degrees += turn_speed * delta * 5
    else:
        _handle_input(delta)
        _handle_movement(delta)
    
    _handle_effects()

func crash() -> void:
    if !_crashed:
        _sprite.visible = false
        _crashed = true
        speed = 0
    
        emit_signal("crashed")

func get_speed_ratio() -> float:
    return speed / max_forward_speed

func _handle_input(delta: float) -> void:
    if Input.is_action_pressed("move_left"):
        rotation_degrees -= turn_speed * delta
    if Input.is_action_pressed("move_right"):
        rotation_degrees += turn_speed * delta
    
    if Input.is_action_pressed("move_up"):
        _moving_forward = true
    elif _last_touch_idx == -1:
        _moving_forward = false

func _handle_movement(delta: float) -> void:
    var size = get_viewport_rect().size

    if _moving_forward:
        speed += forward_speed * delta
    else:
        speed -= forward_speed * delta

    speed = clamp(speed, min_forward_speed, max_forward_speed)
    rotation_degrees = clamp(rotation_degrees, -max_angle, max_angle)

    var x = position.x + speed * rotation * delta
    x = clamp(x, 0, size.x)

    var y = size.y - (50 + (get_speed_ratio() * (size.y / 4 - 50)))
    var offset = Vector2(x, y) - position
    move_and_collide(offset)

func _handle_effects() -> void:
    var ratio = get_speed_ratio()
    var color = _particles.modulate
    color.a = ratio
    
    _particles.modulate = color
    _particles.damping_max = (1.0 - ratio) * 50.0
    _particles.damping_min = (1.0 - ratio) * 50.0
    _particles.radial_accel_min = ratio * 100
    _particles.radial_accel_max = ratio * 100
