(dev)

## TrickDetect 

CS:Sharp Plugin for trick surf

# Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
3. Download [Plugin](https://github.com/TrickSurfCS2/trick-detect-plugin/releases/)
4. Unzip the archive and upload it to the game server (`/csgo/addons/counterstrikesharp/plugins`)

# Config
The config is created automatically in (`/csgo/addons/counterstrikesharp/configs/plugins/{PluginName}`)
> You can see the default values `TrickDetect.example.json`
```json
{
  "DatabaseHost": "localhost",
  "DatabaseName": "surfgxds_dev",
  "DatabaseUser": "surfgxds",
  "DatabasePassword": "surfgxds",
  "DatabasePort": "5432"
}
```

# Commands

| Command          | Description                   |
|------------------|-------------------------------|
| > **saveloc**                                        |
| `saveloc`        | `Save current location`       |
| `tploc`          | `Teleport to current saved location` |
| `prevloc`        | `Teleport to previous saved location and remove current` |
| `backloc`        | `Teleport to previous saved location` |
| `nextloc`        | `Teleport to next saved location` |
| `clearloc`       | `Clear all saved locations`   |
| `toloc`          | `Teleport to a specific saved location by index` |
| > **noclip**                                        |
| `noclipme`          | `Toggle noclip mode` |

# Plugin modularity

You can find implementation pieces of some functionality in separate repositories

- [`saveloc-plugin`](https://github.com/TrickSurfCS2/saveloc-plugin)
- [`noclipme-plugin`](https://github.com/TrickSurfCS2/noclipme-plugin)
