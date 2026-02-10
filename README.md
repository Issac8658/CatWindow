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
1. Godot
Main part is made on Godot Mono 4.6 and it is used to export this part.

2. Bootstrap
It is needed for a single-instance, this is a mini DotNet project, vs code with the appropriate plugins is used for building

3. Packing
- Create any folder anywhere.
- Move the following to this folder:
+ Export results of Godot part and rename the executable file to CatWindow.bin (or another name if you changed it in Bootstrap)
+ Bootstrap executable file (and dependencies, if any)
+ ffmpeg and ffprobe binaries(You can use other versions of ffmpeg, the main thing is that ffmpeg.exe and ffprobe.exe are in the folder)
- Launch Bootstrap and use

## [FFmpeg](https://ffmpeg.org)
This project uses FFmpeg under the [LGPL v2.1](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) license.
FFmpeg is not modified and is distributed as a separate binary.
Source code of FFmpeg can be found [here](https://www.ffmpeg.org/download.html#get-sources)