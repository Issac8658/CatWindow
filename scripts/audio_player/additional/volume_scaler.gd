extends Control;

@export var percent_label : Label;
@export var volume_slider : Slider;
@export var volume_spinbox : SpinBox;
@export var unlock_volume_checkbox : CheckBox;
var max_volume : float = 200;

func _ready() -> void:
	gui_input.connect(func(event : InputEvent):
		if (event is InputEventMouseButton):
			if (event.button_index == MOUSE_BUTTON_WHEEL_DOWN):
				change_volume(-0.01);
			if (event.button_index == MOUSE_BUTTON_WHEEL_UP):
				change_volume(0.01);
	)
	volume_slider.value_changed.connect(set_volume);
	volume_spinbox.value_changed.connect(set_volume);
	change_volume(0);
	
	unlock_volume_checkbox.toggled.connect(func(toggled):
		max_volume = 2000 if toggled else 200;
		change_volume(0);
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
	
	volume_slider.max_value = max_volume;
	volume_spinbox.max_value = max_volume;
	volume_slider.value = bus_volume * 100;
	volume_spinbox.value = bus_volume * 100;
	
func set_volume(volume : float) -> void:
	var bus_volume = volume / 100;
	bus_volume = clamp(bus_volume, 0, max_volume / 100.0);
	
	AudioServer.set_bus_volume_linear(1, bus_volume);
	
	percent_label.text = str(roundi(bus_volume * 100)) + "%";
	percent_label.modulate = Color(1, 1, 1, 1);
	
	volume_slider.max_value = max_volume;
	volume_spinbox.max_value = max_volume;
	volume_slider.value = bus_volume * 100;
	volume_spinbox.value = bus_volume * 100;
