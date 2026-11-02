# RainRemoverMod

Removes rain in Gorilla Tag. Simple as that.

## Features

- Rain is disabled automatically when you load in
- Press **R** on your keyboard to bring it back (and R again to turn it off)
- Top-left of your monitor shows **Rain: On** or **Rain: Off** in green/red
- Auto-updates — just install it once and it keeps itself up to date

## Requirements

- [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases)
- Gorilla Tag (Steam / SteamVR)

## Installation

1. Install BepInEx if you haven't already
2. Drop `RainRemoverMod.dll` into `Gorilla Tag/BepInEx/plugins/RainRemoverMod/`
3. Launch the game — done

Future updates download and apply themselves when you restart the game, so you never have to touch it again.

## Building

Requires .NET Framework 4.7.2 and the game + BepInEx DLLs at the paths in the `.csproj`.

```
dotnet build -c Release
```

## Credits

Made by AxolotlGT
