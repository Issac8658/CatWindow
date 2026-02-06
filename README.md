# Cat Window
Simple audioplayer with cute interface :3c
You need him. Don't ask me why.

### Features
- Supports all formats played by FFmpeg
- Correct audio wave visualizator
- Seamless looping
- .m3u and .m3u8 Playlists (wip)
- Changable playback speed (wip)
- Max volume up to 2000%
- Cute window with ears and paws :3c

## Expoting

This project uses Godot Engine Mono 4.6, you don't need any other programm to edit and export CatWindow

## [FFmpeg](https://ffmpeg.org)
This project uses FFmpeg under the [LGPL v2.1](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) license.
FFmpeg is not modified and is distributed as a separate binary.
Source code of FFmpeg can be found [here](https://www.ffmpeg.org/download.html#get-sources)
### Windows
FFmpeg is packaged as a resource and exported to the user folder (%APPDATA%/CatWindow) when required by the cat window if ffmpeg.exe is missing. It can be replaced with any other FFmpeg in the user folder; there are no strict checks.
### Linux
FFmpeg is NOT EXPORTED for linux. To use the program as an audio player, you need to install it manually (it is enough that the ffmpeg and ffprobe commands are available from any directory)