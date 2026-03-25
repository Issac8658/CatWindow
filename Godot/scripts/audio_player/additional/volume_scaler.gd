extends Control;

@export var percent_label : Label;
@export var volume_slider : Slider;
@export var volume_spinbox : SpinBox;
@export var unlock_volume_checkbox : CheckBox;
var max_volume : float = 200;

func _ready() -> void:
	gui_input.connect(func(event : InputEvent):
		if (event is InputEventMouseButton):
			if (event.pressed):
				if (event.button_index == MOUSE_BUTTON_WHEEL_DOWN):
					if (Input.is_key_pressed(KEY_CTRL)):
						ctrl_change_volume(-0.06);
					else:
						change_volume(-0.02);
				if (event.button_index == MOUSE_BUTTON_WHEEL_UP):
					if (Input.is_key_pressed(KEY_CTRL)):
						ctrl_change_volume(0.06);
					else:
						change_volume(0.02);
	);
	volume_slider.value_changed.connect(set_volume);
	volume_spinbox.value_changed.connect(set_volume);
	
	unlock_volume_checkbox.toggled.connect(func(toggled):
		max_volume = 2000 if toggled else 200;
		change_volume(0);
	)
	update()

func _process(delta: float) -> void:
	if AudioServer.get_bus_volume_linear(1) <= 0.001:
		percent_label.modulate = Color(1,0.3,0.3);
	percent_label.modulate.a -= delta;


func ctrl_change_volume(modifier : float) -> void:
	var bus_volume := AudioServer.get_bus_volume_linear(1);
	
	bus_volume += modifier;
	bus_volume *= 10;
	bus_volume = round(bus_volume);
	bus_volume /= 10;
	bus_volume = clamp(bus_volume, 0, max_volume / 100.0);
	
	AudioServer.set_bus_volume_linear(1, bus_volume);
	
	update();

func change_volume(modifier : float) -> void:
	var bus_volume := AudioServer.get_bus_volume_linear(1);
	
	bus_volume += modifier;
	bus_volume = clamp(bus_volume, 0, max_volume / 100.0);
	
	AudioServer.set_bus_volume_linear(1, bus_volume);
	
	update();
	
func set_volume(volume : float) -> void:
	var bus_volume := volume / 100;
	bus_volume = clamp(bus_volume, 0, max_volume / 100.0);
	AudioServer.set_bus_volume_linear(1, bus_volume);
	
	update()

func update() -> void:
	var bus_volume := AudioServer.get_bus_volume_linear(1);
	
	percent_label.text = str(roundi(bus_volume * 100)) + "%";
	percent_label.modulate = Color(1, 1, 1, 1);
	
	volume_slider.max_value = max_volume;
	volume_spinbox.max_value = max_volume;
	volume_slider.value = bus_volume * 100;
	volume_spinbox.value = bus_volume * 100;
