# Cat Window
Simple audioplayer with cute interface :3c
> You need him. Don't ask me why.

## Features
- Supports all formats played by FFmpeg
- Correct audio wave visualizator
- Seamless looping
- .m3u and .m3u8 Playlists (wip)
- Max volume up to 2000%
- Cute window with ears and paws :3c

## Expoting
0. Download the project in any convenient way

1. Godot
Main part is made in [Godot Mono](https://godotengine.org/download/) 4.6.1 and it is used to export this part.

- Download and install [Godot Mono 4.6.1](https://godotengine.org/download/archive/4.6.1-stable/) (.Net version, not standart)
- Open `Godot_v4.6.1-stable_mono_win64.exe` and import project in `/Godot/project.godot`
- Change it as you need
- Export and get executable file

2. Bootstrap
It is needed for a single-instance, this is a mini DotNet project, vs code with the appropriate plugins is used for building

- Download and install [DotNet 10.0](https://dotnet.microsoft.com/download)
### Windows:
- Run `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true` in bootstrap folder
- Done, bootstrap will be stored in `bin\Release\net10.0\win-x64\publish`
### Linux:
- Run `dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true` in bootstrap folder
- Done, bootstrap will be stored in `bin\Release\net10.0\linux-x64\publish`

3. Packing
- Create any folder anywhere.
- Move the following to this folder:

    - Export results of Godot part and rename the executable file to CatWindow.bin (or another name if you changed it in Bootstrap, but Windows correct working only with .bin)
    - Bootstrap executable file (and dependencies, if any)
    - *(Windows only)* ffmpeg and ffprobe binaries(You can use other versions of ffmpeg, the main thing is that ffmpeg.exe and ffprobe.exe are in the folder)
    
- Launch Bootstrap and use

---

# [FFmpeg](https://ffmpeg.org)
This project uses FFmpeg under the [LGPL v2.1](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) license.
FFmpeg is not modified and is distributed as a separate binary.
Source code of FFmpeg can be found [here](https://www.ffmpeg.org/download.html#get-sources)
