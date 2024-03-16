extends Node



@onready var bomb_timer = $BombTimer
@onready var line_edit = $LineEdit

var onesecond_timer

func _ready():
	DisplayServer.window_set_flag(DisplayServer.WINDOW_FLAG_TRANSPARENT, true)
	
	onesecond_timer = Timer.new()
	onesecond_timer.wait_time = 1.0
	onesecond_timer.one_shot = false
	onesecond_timer.timeout.connect(tick)
	add_child(onesecond_timer)
	bomb_timer.timeout.connect(onesecond_timer.stop)
	
	start()
	pass


func _process(delta):
	
	pass
	

func tick():
	line_edit.text = get_time_str()

func start():
	bomb_timer.start()
	onesecond_timer.start()

func get_time_str():
	var minutes:int = bomb_timer.time_left / 60
	var seconds:int = bomb_timer.time_left - minutes
	
	return format_time(minutes) + ":" + format_time(seconds)

func format_time(time:int):
	return ("0%d" % [time]) if time / 10 == 0 else "%d" % [time]
