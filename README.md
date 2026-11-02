# RainRemoverMod

A BepInEx mod for Gorilla Tag that removes rain from the game. Press **R** on your keyboard to toggle it back on or off whenever you want.

## Requirements

- [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases)
- Gorilla Tag (Steam)

## Installation

1. Make sure BepInEx is installed in your Gorilla Tag folder
2. Drop `RainRemoverMod.dll` into `Gorilla Tag/BepInEx/plugins/`
3. Launch the game

## Usage

Rain is disabled by default when the mod loads. While the game is running, press **R** on your keyboard to toggle rain on or off.

## Building from source

Requires .NET Framework 4.7.2 and the Gorilla Tag + BepInEx DLLs in the paths referenced by the .csproj.

```
dotnet build -c Release
```
