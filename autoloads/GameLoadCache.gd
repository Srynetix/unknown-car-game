extends SxLoadCache

func load_resources():
    var logger = SxLog.get_logger("SxLoadCache")
    logger.set_max_log_level(SxLog.LogLevel.INFO)

    store_scene("CarCrash", "res://scenes/fx/CarCrash.tscn")
    store_scene("Sparkle", "res://scenes/fx/Sparkle.tscn")
    store_scene("Warning", "res://scenes/fx/Warning.tscn")
    store_scene("Obstacle", "res://scenes/entities/Obstacle.tscn")
    store_scene("BlockingObstacle", "res://scenes/entities/BlockingObstacle.tscn")
    store_scene("Chronometer", "res://scenes/entities/Chronometer.tscn")