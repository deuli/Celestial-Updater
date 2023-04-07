using System.Net;
using System.Text.Json.Serialization;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using Newtonsoft.Json;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

public class CelestialConfig
{
    public string instanceID = "Nebula SMP";
    public string url = "https://www.dropbox.com/s/gb9056pklrl0vxs/Nebula.rar?dl=1";
    public string instancesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PrismLauncher\instances\";
    [JsonIgnore] public string instancePath;
    [JsonIgnore] public string savePath;
    [JsonIgnore] public string extractPath;
    [JsonIgnore] public string minecraftPath;
    [JsonIgnore] public string modsPath;

    public CelestialConfig()
    {
        instancePath = instancesPath + instanceID;
        savePath = instancePath + @"\DownloadedFiles.rar";
        extractPath = instancePath + @"\ExtractedFiles";
        minecraftPath = instancePath + @"\.minecraft";
        modsPath = minecraftPath + @"\mods";
    }
}

public class Program
{
    private static string configFile = "celestialconfig.json";
    private static string modlistFile = ".modlist.txt";

    static void Main(string[] args)
    {
        CelestialConfig? config;
        if(File.Exists(configFile))
        {
            Console.WriteLine("Config file found. Loading values...");
            config = JsonConvert.DeserializeObject<CelestialConfig>(File.ReadAllText("celestialconfig.json"));

            if(config == null)
            {
                Console.WriteLine("An error occured while reading " + configFile + ". Is the file correctly formatted?");
                return;
            }

            Console.WriteLine("\nLoaded values:");
            Console.WriteLine("Instance ID: " + config.instanceID);
            Console.WriteLine("URL: " + config.url);
            Console.WriteLine("Instances Path: " + config.instancesPath);
        }
        else
        {
            Console.WriteLine("Config file not found. Would you like to generate one? [Y/N]");
            ConsoleKeyInfo key;
            do
                key = Console.ReadKey();
            while (key.Key.ToString() != "Y" && key.Key.ToString() != "N");

            config = new CelestialConfig();
            switch (key.Key.ToString())
            {
                case "Y":
                    config.instanceID = "Insert Instance ID here (Folder name)";
                    config.url = "Insert files URL here";
                    config.instancesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PrismLauncher\instances\";
                    File.WriteAllText("celestialconfig.json", JsonConvert.SerializeObject(config, Formatting.Indented));

                    Console.WriteLine("\nFile generated as " + configFile + ". Please fill in the correct values and restart the script.");
                    Console.ReadLine();
                    return;

                case "N":
                    Console.WriteLine("\nUsing default values.");
                    break;

                //default:
                //    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("The script is going to update the modpack inside the \"" + config.instanceID + "\" folder that is located at \"" + config.instancePath + "\".");
        Console.WriteLine("Make sure that the folder is correct by pressing the \"Folder\" button inside of Prism Launcher and that the paths are identical.");
        Console.WriteLine("If the paths aren't identical, close the program and make the necessary changes if possible.");

        Console.WriteLine();
        Console.WriteLine("Press enter to update " + config.instanceID + "...");
        Console.ReadLine();

        try
        {
            Console.WriteLine("Downloading the modpack files from " + config.url + "...");
            using (WebClient client = new WebClient())
            {
                DateTime lastUpdate = DateTime.Now;
                client.DownloadProgressChanged += (sender, e) =>
                {
                    if (DateTime.Now - lastUpdate >= TimeSpan.FromSeconds(1))
                    {
                        lastUpdate = DateTime.Now;
                        Console.WriteLine($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes ({e.ProgressPercentage}%)");
                    }
                };

                client.DownloadFileAsync(new Uri(config.url), config.savePath);
                while (client.IsBusy) { };

            }
            Console.WriteLine("Downloaded complete.");

            Console.WriteLine("Extracting all the files into " + config.extractPath + "...");
            if (!Directory.Exists(config.extractPath))
                Directory.CreateDirectory(config.extractPath);

            using (RarArchive archive = RarArchive.Open(config.savePath))
            {
                foreach (RarArchiveEntry entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                        entry.WriteToDirectory(config.extractPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    Console.WriteLine(entry.Key + " extracted");
                }
            }
            Console.WriteLine("Files have been extraced.");

            if (Directory.Exists(config.modsPath))
            {
                if(File.Exists(Path.Combine(config.modsPath, modlistFile)))
                {
                    Console.WriteLine("Deleting previous mods...");
                    string[] modlist = File.ReadAllLines(Path.Combine(config.modsPath, modlistFile));
                    string[] modfiles = Directory.GetFiles(config.modsPath);
                    foreach (string modfile in modfiles)
                    {
                        Console.WriteLine(Path.GetFileName(modfile) + ": " + modlist.Contains(Path.GetFileName(modfile)));
                        if (modlist.Contains(Path.GetFileName(modfile)))
                        {
                            File.Delete(modfile);
                            Console.WriteLine("Deleted " + Path.GetFileName(modfile));
                        }
                        else
                            Console.WriteLine("Skipped " + Path.GetFileName(modfile));
                    }
                    Console.WriteLine("Previous mods deleted.");
                }
                else
                {
                    Console.WriteLine(modlistFile + " not found. Deleting the mods folder...");
                    Directory.Delete(config.modsPath, true);
                    Console.WriteLine("mods folder has been deleted.");
                }
            }

            Console.WriteLine("Creating a mods list text file and moving mod files...");
            if(Directory.Exists(Path.Combine(config.extractPath, "mods")))
            {
                string[] modlist = { };
                string[] modfiles = Directory.GetFiles(Path.Combine(config.extractPath, "mods"));

                if (!Directory.Exists(config.modsPath))
                    Directory.CreateDirectory(config.modsPath);

                foreach (string modfile in modfiles)
                {
                    Array.Resize(ref modlist, modlist.Length + 1);
                    modlist[modlist.Length - 1] = Path.GetFileName(modfile);
                    File.Move(modfile, Path.Combine(config.modsPath, Path.GetFileName(modfile)), true);
                    Console.WriteLine("Added " + Path.GetFileName(modfile));
                }

                File.WriteAllLines(Path.Combine(config.modsPath, modlistFile), modlist);
            }
            Console.WriteLine("mods list text file created and mod files moved successfully.");

            Console.WriteLine("Moving rest of the files to their location...");
            foreach (string sourceFilePath in Directory.GetFiles(config.extractPath, "*", SearchOption.AllDirectories))
            {
                string destinationDirectory = Path.GetDirectoryName(sourceFilePath.Replace(config.extractPath, config.minecraftPath));
                if (!Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                Console.WriteLine(Path.GetFileName(sourceFilePath) + " -> " + destinationDirectory);
                File.Move(sourceFilePath, Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath)), true);
            }
            Console.WriteLine("Files moved to their destination.");

            Console.WriteLine("Deleting downloaded files...");
            File.Delete(config.savePath);
            Directory.Delete(config.extractPath, true);
            Console.WriteLine("Downloaded files deleted.");

            Console.WriteLine();
            Console.WriteLine("Nebula SMP successfully updated!");
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("An error has occured!");
            Console.Error.WriteLine(e);
        }
    }
}

