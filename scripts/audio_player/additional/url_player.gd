extends Control

@export var Player : FFmpegPlayer;
@export var PlayButton : Button;
@export var InputLineEdit : LineEdit;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	PlayButton.pressed.connect(func ():
		Player.Play(InputLineEdit.text);
	)
