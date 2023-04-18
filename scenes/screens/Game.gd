extends Control

@export var tilemap_scroll_speed := 100.0
@export var cell_rows_to_skip := 4
@export var initial_remaining_time := 30

const KM_H_COEF = 6.0

@onready var _tilemap: TileMap = $Background
@onready var _score_label: Label = $UITop/Score
@onready var _time_label: Label = $UIBottom/Time
@onready var _spawn_timer: Timer = $SpawnTimer
@onready var _spawn_blocking_timer: Timer = $SpawnBlockingTimer
@onready var _chrono_timer: Timer = $ChronoTimer
@onready var _camera: Camera2D = $Camera
@onready var _car: Car = $Car
@onready var _obstacles_container: Node2D = $Obstacles
@onready var _motion_blur: SxFxMotionBlur = $MotionBlur
@onready var _vignette: SxFxVignette = $VignetteLayer/Vignette
@onready var _shockwave: SxFxShockwave = $Shockwave
@onready var _game_over: GameOver = $GameOver
@onready var _time_animation_player: AnimationPlayer = $UIBottom/TimeAnimation

var _tile_size := Vector2.ZERO
var _next_timeout := 0.0
var _score := 0
var _remaining_time := 0
var _blocking_wait_time := 5.0
var _is_game_over := false

func _ready() -> void:
    _spawn_timer.timeout.connect(_spawn_timeout)
    _chrono_timer.timeout.connect(_chrono_timeout)
    _spawn_blocking_timer.timeout.connect(_blocking_timeout)
    _car.crashed.connect(_show_game_over)

    _tile_size = Vector2(_tilemap.tile_set.tile_size) * _tilemap.scale
    _next_timeout = _spawn_timer.wait_time
    _spawn_blocking_timer.wait_time = _blocking_wait_time
    _spawn_blocking_timer.start()

    var vp_size = get_viewport_rect().size
    _car.position = Vector2(vp_size.x / 2, vp_size.y - 100)

    _remaining_time = initial_remaining_time
    _update_time_label()

func _process(delta: float) -> void:
    _update_tilemap(delta)
    _update_effects()

func _update_tilemap(delta: float) -> void:
    _tilemap.position += Vector2(0, _car.speed * delta)

    var limit = cell_rows_to_skip * _tile_size
    if _tilemap.position.y >= limit.y:
        _tilemap.position -= Vector2(0, limit.y)

func _update_effects() -> void:
    var ratio = _car.get_speed_ratio()
    var coef = randf_range(-1.0, 1.0) * 2 * ratio
    _camera.offset = Vector2(coef, coef)

    _motion_blur.strength = ratio * 0.015
    _vignette.vignette_ratio = ratio * 0.25

    if ratio == 1.0:
        if !_shockwave.wave_is_running():
            var center = _car.position / get_viewport_rect().size
            center = Vector2(center.x, center.y)
            _shockwave.start_wave(center)

    _next_timeout = 0.25 + ((1.0 - ratio) * 2.0)

func _spawn_timeout() -> void:
    if SxRand.range_i(0, 10) == 0:
        _spawn_chronometer()
    else:
        _spawn_obstacle()

    _spawn_timer.wait_time = _next_timeout
    _spawn_timer.start()

func _chrono_timeout() -> void:
    _remaining_time -= 1
    _update_time_label()

    if _remaining_time == 0:
        _car.crash()

func _blocking_timeout() -> void:
    var vp_size = get_viewport_rect().size
    var off = 10
    var x = randf_range(off, vp_size.x - off)

    var inst = GameLoadCache.instantiate_scene("Warning")
    inst.position = Vector2(x, 50)
    add_child(inst)

    await get_tree().create_timer(1.5).timeout
    if _is_game_over:
        return

    _spawn_blocking_obstacle(inst.position + Vector2.UP * 150)

    _blocking_wait_time = max(_blocking_wait_time - 1, 0.1)
    _spawn_blocking_timer.wait_time = _blocking_wait_time
    _spawn_blocking_timer.start()

func _time_picked() -> void:
    _chrono_timer.stop()
    _time_animation_player.play("bump")

    await _time_animation_player.animation_finished
    if _is_game_over:
        return
    
    _chrono_timer.start()

func _add_one_second_to_timer() -> void:
    _remaining_time += 1
    _update_time_label()

func _spawn_chronometer() -> void:
    var vp_size = get_viewport_rect().size
    var offset = 100

    var inst = GameLoadCache.instantiate_scene("Chronometer")
    inst.car = _car
    inst.position = Vector2(randf_range(offset, vp_size.x - offset), -100)
    inst.picked.connect(_time_picked)
    _obstacles_container.add_child(inst)

func _spawn_obstacle() -> void:
    var vp_size = get_viewport_rect().size
    var offset = 100

    var inst = GameLoadCache.instantiate_scene("Obstacle")
    inst.car = _car
    inst.position = Vector2(randf_range(offset, vp_size.x - offset), -100)
    inst.hit.connect(_add_score)
    _obstacles_container.add_child(inst)

func _spawn_blocking_obstacle(pos: Vector2) -> void:
    var inst = GameLoadCache.instantiate_scene("BlockingObstacle")
    inst.car = _car
    inst.position = pos

    _obstacles_container.add_child(inst)

func _show_game_over() -> void:
    var inst = GameLoadCache.instantiate_scene("CarCrash")
    inst.position = _car.position
    add_child(inst)

    _is_game_over = true

    var center = _car.position / get_viewport_rect().size
    center = Vector2(center.x, center.y)

    _chrono_timer.stop()
    _spawn_timer.stop()
    _spawn_blocking_timer.stop()
    _shockwave.start_wave(center)
    _game_over.start()

func _update_time_label() -> void:
    var time_ratio = float(_remaining_time) / float(initial_remaining_time)
    var scale_value = 1 + (1.0 - time_ratio) * 2
    _time_label.text = "%d" % _remaining_time
    _time_label.scale = Vector2(scale_value, scale_value)

    await get_tree().process_frame
    _time_label.pivot_offset = _time_label.size / 2

func _add_score() -> void:
    var ratio = _car.get_speed_ratio()
    _score += int(ratio * 1000.0)
    _score_label.text = "%d$" % _score
