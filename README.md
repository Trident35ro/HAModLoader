![](https://raw.githubusercontent.com/Trident35ro/HAModLoader/blob/8ffdbb3b04d90908c435156f025ad9a0cf93a816/haml_logo.png)

#HAModLoader

![GitHub License](https://img.shields.io/github/license/Trident35ro/HAModLoader) ![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/Trident35ro/HAModLoader/latest/total) ![GitHub Release](https://img.shields.io/github/v/release/Trident35ro/HAModLoader) 
HAModLoader (HAML) is an multiplatform, community driven mod loader for the popular mobile game, Hybrid Animals. It is based on a very old version of game (v185) and modified to have mod loading capabilities and a proper API for modding.

**DISCLAIMER: THIS TOOL WASN'T MADE FOR EXPLOITING VULNABILITIES/HACKING/BREAKING THE GAME'S ECONOMY. THIS TOOL IS ONLY FOR ADDING CUSTOM CONTENT INTO THE GAME OR CHANGING ALREADY EXISTENT CONTENT.**

*This tool isn't enforced or officially supported by Abstract Software Inc.*

**Table of Contents**

[TOC]

## Setup for players:

### Android

#### Install
1. Download the latest [released](https://github.com/Trident35ro/HAModLoader/releases/latest/) `.apk` file
2. Go to your downloads
3. Open the file (for some devices Android will warn you that you need to allow downloading apps from unknown sources, in that case it will redirect you to the setting and allow from there)
4. Install the app
5. Open the game to set up its files and folders

#### Other info

To manage the mods and logs open your prefered file manager, go to your internal memory then Android -> obb -> com.Trident35ro.HAModLoader (`/storage/emulated/0/Android/obb/com.Trident35ro.HAModLoader`) and there you have the `mods` and `logs` folders.
To install mods simply put the `.dll` files downloaded from the mod creator the `mods` folder.
Tha game's save files are stored in `/storage/emulated/0/Android/data/com.Trident35ro.HAModLoader` (Android -> media -> com.Trident35ro.HAModLoader)

### Windows

#### Install

1. Download the latest [release](https://github.com/Trident35ro/HAModLoader/releases/latest/) for Windows
2. Go to your downloads
3. Unzip the `.zip` archive downloaded earlier
4. Run `HAModLoader.exe`
5.  Let it set up its files and folders

#### Other info

In Windows, the `mods` and `logs` folders are inside the same folder as `HAModLoader.exe`.
To install mods simply put the `.dll` files downloaded from the mod creator the `mods` folder.
The game's save files are inside `C:/Users/<your-username>/AppData/LocalLow/Trident35ro/HAModLoader` or type `%localappdata%/Low/Trident35ro/HAModLoader` directly in Windows Explorer adress tab or in "Run" window (if you don't see the `AppData` folder enable "Show hidden files and folders" in Windows Explorer options).

### MacOS

#### Install

1. Download the latest [release](https://github.com/Trident35ro/HAModLoader/releases/latest/) for MacOS
2. Go to your downloads
3. Unzip the `.zip` archive downloaded earlier
4. Run `HAModLoader.dmg`
5. Drag the HAModLoader logo into the `Aplications` folder in the opened windows
6. Run the game and let it set up its files and folders

#### Other info

You can find the `Mods` and `Logs` folders on MacOS at `/Users/<your_username>/Library/Application Support/HAModLoader`.
To install mods simply put the `.dll` files downloaded from the mod creator the `mods` folder.
You can find your save files at ``.

### Linux (any distribution)

#### Install 

1. Download the latest [release](https://github.com/Trident35ro/HAModLoader/releases/latest/) for Linux
2. Go to your downloads
3. Unzip the `.zip` archive downloaded earlier
4. Run `HAModLoader.x86_64` (or open an console window at that path and run `./HAModLoader.x86_64`) and let the game set up its files and folders

#### Other info

In Linux, the `Mods` and `Logs` folders are at `~/.local/share/HAModLoader`.
To install mods simply put the `.dll` files downloaded from the mod creator the `mods` folder.
The game's save files are inside `C:/Users/<your-username>/AppData/LocalLow/Trident35ro/HAModLoader` or type `%localappdata%/Low/Trident35ro/HAModLoader` directly in Windows Explorer adress tab or in "Run" window (if you don't see the `AppData` folder enable "Show hidden files and folders" in Windows Explorer options).

## Setup for modders

Depending on your OS or preferences get yourself an IDE which allows compiling C# code (ex. Visual Studio, Rider, etc.) or compile your mods in the terminal (only have `dotnet` installed and a simple text editor). To make mods, regarding of the development medium, you should have [.NET Framework 4.7.2](http://https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472 ".NET Framework 4.7.2") installed.
After you set up your coding environment, make an simple class project and make sure to have as output an `.dll` file. Reference Unity libraries and `HAModLoaderAPI.dll` files you find in the game's files (depends where they are and how do you get them on the platform) and add `HAMod` interface after your class's name.
Now you can start using the API's classes and methods or Unity's inside your mod.
A more detailed tutorial and documentation will come in the future.

## Setup for contribuitors

To start contributing to this project clone this repo with `git clone https://github.com/Trident35ro/HAModLoader.git` or download the source code directly in GitHub. You should have Unity 2021.3.45f2 installed and a text editor (or an IDE like VSCode, Visual Studio or Rider).
To publish your changes make a pull request with your code and wait until it is reviewed.

## Credits

- Eris3DS: Made the original HAModLoader
- TheFirstHybrid: Made Hybrid Animals in the first place

And other members of the Hybrid Animals community I haven't mentioned here.

## License

This project is protected trought the [GNU General Public License V3.0(GPL)](http://https://github.com/Trident35ro/HAModLoader/blob/main/LICENSE "GNU General Public License V3.0").

