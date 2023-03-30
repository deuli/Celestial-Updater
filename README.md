# Celestial-Updater
A script to automatically update a private Minecraft modpack

---

Celestial Updater is a script written in C# that automatically updates a private Minecraft modpack used among my friends.

It assumes that the modpack is being ran using Prism Launcher and that the instance is located inside of a "Nebula SMP" folder inside of "%appdata%/PrismLauncher/instances".

It works by
- Downloading the modpack's RAR file
- Extracting the RAR file
- Deleting the mods folder
- Moving all of the files to their destination
- Deletes the RAR folder and the extracted folder
