extends Control;

@export var percent_label : Label;
@export var max_volume : float = 100;

func _ready() -> void:
	gui_input.connect(func(event : InputEvent):
		if (event is InputEventMouseButton):
			if (event.button_index == MOUSE_BUTTON_WHEEL_DOWN):
				change_volume(-0.01);
			if (event.button_index == MOUSE_BUTTON_WHEEL_UP):
				change_volume(0.01);
	)

func _process(delta: float) -> void:
	if AudioServer.get_bus_volume_linear(1) <= 0.001:
		percent_label.modulate = Color(1,0.3,0.3);
	percent_label.modulate.a -= delta;

func change_volume(modifier : float) -> void:
	var bus_volume := AudioServer.get_bus_volume_linear(1);
	
	bus_volume += modifier;
	bus_volume = clamp(bus_volume, 0, max_volume / 100.0);
	
	AudioServer.set_bus_volume_linear(1, bus_volume);
	
	percent_label.text = str(roundi(bus_volume * 100)) + "%";
	percent_label.modulate = Color(1, 1, 1, 1);
