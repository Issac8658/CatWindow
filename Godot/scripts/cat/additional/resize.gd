extends Control;

const DEFAULT_SIZE := Vector2i(250, 200)

@onready var window := get_window()

var mouse_offset := Vector2i(0,0);
var is_dragging := false;

@export var win_control : Node;
@export var min_size : Vector2i;
@export var max_size : Vector2i;


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	mouse_entered.connect(func ():
		modulate = Color.WHITE;
	);
	mouse_exited.connect(func ():
		modulate = Color.TRANSPARENT;
	);
	

	gui_input.connect(func (Event):
		if (Event is InputEventMouseButton):
			if ((Event as InputEventMouseButton).button_index == MOUSE_BUTTON_LEFT):
				if (Event.pressed):
					mouse_offset = DisplayServer.mouse_get_position() - (window.position + window.size);
					is_dragging = true;
				else:
					is_dragging = false;
			elif ((Event as InputEventMouseButton).button_index == MOUSE_BUTTON_RIGHT):
				win_control.WindowSize = DEFAULT_SIZE;
	);

func _process(_delta: float) -> void:
	if is_dragging:
		var result := DisplayServer.mouse_get_position() - window.position - mouse_offset;
		result = result.clamp(min_size, DisplayServer.screen_get_size(DisplayServer.SCREEN_PRIMARY) - Vector2i(1,1))
		win_control.WindowSize = result
