# Celestial-Updater
A script to automatically update a private Minecraft modpack

---

Celestial Updater is a script written in C# that by default automatically updates a private Minecraft modpack used among my friends but can be configured to be used for other modpacks.

It assumes that the modpack is being ran using Prism Launcher.

By default, it updates the instance inside of a "Nebula SMP" folder located inside of "%appdata%/PrismLauncher/instances" but this can be changed with a config file named "celestialconfig.json", which the program can automatically generate for the user.

It works by
- Downloading the modpack's RAR file
- Extracting the RAR file
- Deleting the mods folder
- Moving all of the files to their destination
- Deletes the RAR folder and the extracted folder

## Dependencies
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) is used to extract files from the downloaded RAR file
- [Newtonsoft.Json](https://www.newtonsoft.com/json) is used to read and write JSON files for the config of the script
