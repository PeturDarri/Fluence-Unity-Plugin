# Fluence-Unity-Plugin
This is an example Unity (2017.3.1f1) project that uses Google's Fluence plugin to render lightfields.

![Preview](https://i.imgur.com/4haqRsY.png)
[Preview GIF](https://i.imgur.com/tN1Z5cj.gifv)

## Quick Setup
This repository does not have all the required files to work. Here's how to get it working:
1. Install [Welcome to Light Fields on Steam](http://store.steampowered.com/app/771310/Welcome_to_Light_Fields/).
2. Navigate to the install folder.
3. Copy `WTLF_Data/Plugins/fluence_plugin.dll` to the `Assets/Plugins` folder of the project.
4. Copy any .lf files you want to use in `WTLF_Data/StreamingAssets/Lightfields` to the `Assets/StreamingAssets/Lightfields` folder in the project.
5. In the `example.unity` scene, create an empty gameobject and add the FluenceLightfield component to it (or use the existing Beach Condo Lightfield gameobject in the scene).
6. Set the Lightfield Path to which lightfield you want rendered. Make sure to include the Lightfields folder. Example: `Lightfields/Lightfield.lf`.
7. Press Play!

## Disclaimer
This project is **not** meant to be a fully functioning plugin for Fluence. It's only an example of how to get it working. It's expected you make changes to it where you need to.
