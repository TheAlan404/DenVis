# DenVis

Lightweight Windows audio visualizer made in C#

### Features

- Audio Visualizer
- Snow (no UI yet, [enable here](https://denvis.glitch.me/))
- WebSocket API

### Userscript

DenVis now has an [userscript](https://raw.githubusercontent.com/TheAlan404/DenVis/master/Extensions/denvis.user.js)!
- Connects to the WebSocket API
- Shows you what's playing while you arent looking
- Supports [Youtube](https://www.youtube.com/) only (for now)

### Installation

**Only works in Windows**
Tested Windows versions: 8.1 and 10

[Download from the releases page](https://github.com/TheAlan404/DenVis/releases)

**"Permanent" installation**
1. Download a release from the releases page
2. Extract to somewhere (I recommend `C:/Program Files/DenVis` etc)
3. `win+R` (or open explorer etc)
4. Go to (file path) `shell:startup`
5. Create a shortcut in that folder to `DenVis.exe`
6. ~~Profit~~ Now DenVis will start automatically when Windows starts!

### Building

Required nuget packages:
- CSCore
- GameOverlay.NET
- Fleck

### Contributing

Pull requests etc in all forms are welcome :3

Thanks to [Armagan](https://github.com/TheArmagan) for helping me lmao im dumb

### Screenshot

(a.0.1)
![denvis](https://user-images.githubusercontent.com/43997085/149615757-8a0efe5e-5c4e-4297-b6d3-ff3278bc4f3c.png)
