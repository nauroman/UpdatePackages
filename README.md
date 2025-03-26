# Git Package Updater

**Git Package Updater** is a Unity editor tool that updates all git packages in the project.

## Features

- Automatically updates git packages in the `manifest.json` file.
- Skips Unity packages (`com.unity.*`).
- Appends a random query parameter to force Unity to update the package.

## Installation

1. Use a Package Manager to add the package to your project by installing it from the Git URL: https://github.com/nauroman/UpdatePackages.git.

## Usage

1. Open Unity.
2. Go to `Tools` > `Update all git packages` in the Unity menu.
3. The tool will update the `manifest.json` file and force Unity to refresh the packages.

## Requirements

- Unity 2021.3.24f1 or later.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.