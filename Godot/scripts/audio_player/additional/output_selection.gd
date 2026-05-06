extends OptionButton

@export var audio_capture_node : Node

var devices : PackedStringArray = AudioServer.get_output_device_list()
var selected_device : String = "Default"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	for device in devices:
		add_item(device)

	item_selected.connect(func (item):
		selected_device = devices[item]
		AudioServer.output_device = devices[item]
		if (audio_capture_node):
			audio_capture_node.StartCapture(selected_device)
	)
	mouse_entered.connect(func():
		devices = AudioServer.get_output_device_list()
		clear()
		for device in devices:
			add_item(device)
			if device == selected_device:
				select(devices.find(device))
	)
