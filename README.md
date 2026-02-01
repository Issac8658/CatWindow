# Cat Window
Cute cat-like audioplayer :3c

(I'll make normal description later.)

# [FFmpeg](https://ffmpeg.org)
This project uses FFmpeg under the [LGPL v2.1](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) license.
FFmpeg is not modified and is distributed as a separate binary.
Source code of FFmpeg can be found [here](https://www.ffmpeg.org/download.html#get-sources)
## Windows
FFmpeg is packaged as a resource and exported to the user folder (`%APPDATA%/CatWindow`) when required by the cat window if ffmpeg.exe is missing. It can be replaced with any other FFmpeg in the user folder; there are no strict checks.
## Linux
FFmpeg is NOT EXPORTED for linux. To use the program as an audio player, you need to install it manually (it is enough that the ffmpeg and ffprobe commands are available from any directory)