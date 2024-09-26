# TrickDetect 

CS:Sharp Plugin for trick surf

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
3. Download [Plugin](https://github.com/TrickSurfCS2/trick-detect-plugin/releases/)
4. Unzip the archive and upload it to the game server (`/csgo/addons/counterstrikesharp/plugins`)

## Config
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

## Commands

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
| > **helpers**                                        |
| `noclipme`          | `Toggle noclip mode` |
| `map`          | `Select map` |
| `hud`          | `Toggle hud visibility` |
| `debug`          | `Toggle debug mode` |
| `info`          | `Print current info player` |
| `fov`          | `Sets the player's FOV` |
| > **admin utils**                                        |
| `update_tricks`          | `Update tricks list data` |


## TrickSurf ecosystem (Related section)

### Plugin modularity

You can find implementation pieces of some functionality in separate repositories

- [`saveloc-plugin`](https://github.com/TrickSurfCS2/saveloc-plugin)
- [`noclipme-plugin`](https://github.com/TrickSurfCS2/noclipme-plugin)

### Frontend

Web Client that allows you to display and search tricks for specific maps

- [`trick-surf-front`](https://github.com/TrickSurfCS2/trick-surf-front)

### Backend

Web server part that implements receiving tricks via RestApa for further use in, for example, *trick-surf-front*

- [`trick-surf-back`](https://github.com/TrickSurfCS2/trick-surf-back)
