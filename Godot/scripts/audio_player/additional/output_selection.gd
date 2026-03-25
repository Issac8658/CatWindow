extends OptionButton

var devices : PackedStringArray = AudioServer.get_output_device_list()
var selected_device : String = "Default"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	for device in devices:
		add_item(device)
	item_selected.connect(func (item):
		selected_device = devices[item]
		AudioServer.output_device = devices[item]
	)
	mouse_entered.connect(func():
		devices = AudioServer.get_output_device_list()
		clear()
		for device in devices:
			add_item(device)
			if device == selected_device:
				select(devices.find(device))
	)
