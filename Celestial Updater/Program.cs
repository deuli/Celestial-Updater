using System.Net;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

string url = "https://www.dropbox.com/s/gb9056pklrl0vxs/Nebula.rar?dl=1";
string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string instancePath = roamingPath + @"\PrismLauncher\instances\Nebula SMP";
string savePath = instancePath + @"\Nebula.rar";
string extractPath = instancePath + @"\Nebula";
string minecraftPath = instancePath + @"\.minecraft";
string modsPath = minecraftPath + @"\mods";

Console.WriteLine("This script assumes that the instances of Prism Launcher is located at \"" + Path.GetDirectoryName(instancePath) + "\" and " +
    "that the Nebula SMP instance folder is named \"" + Path.GetFileName(instancePath) + "\".");
Console.WriteLine("Make sure that the folder is correct by pressing the \"Folder\" button inside of Prism Launcher and that the paths are identical.");
Console.WriteLine("If the paths aren't identical, close the program and make the necessary changes if possible.");

Console.WriteLine();
Console.WriteLine("Press enter to update the Nebula SMP pack...");
Console.ReadLine();

try
{
    Console.WriteLine("Downloading the modpack files from " + url + "...");
    using (WebClient client = new WebClient())
    {
        DateTime lastUpdate = DateTime.Now;
        client.DownloadProgressChanged += (sender, e) =>
        {
            if(DateTime.Now - lastUpdate >= TimeSpan.FromSeconds(1))
            {
                lastUpdate = DateTime.Now;
                Console.WriteLine($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes ({e.ProgressPercentage}%)");
            }
        };

        client.DownloadFileAsync(new Uri(url), savePath);
        while (client.IsBusy) { };
        
    }
    Console.WriteLine("Downloaded complete.");

    Console.WriteLine("Extracting all the files into " + extractPath + "...");
    if (!Directory.Exists(extractPath))
        Directory.CreateDirectory(extractPath);

    using (RarArchive archive = RarArchive.Open(savePath))
    {
        foreach (RarArchiveEntry entry in archive.Entries)
        {
            if (!entry.IsDirectory)
                entry.WriteToDirectory(extractPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
            Console.WriteLine(entry.Key + " extracted");
        }
    }
    Console.WriteLine("Files have been extraced.");

    if (Directory.Exists(modsPath))
    {
        Console.WriteLine("Deleting the mods folder...");
        Directory.Delete(modsPath, true);
        Console.WriteLine("mods folder has been deleted.");
    }

    Console.WriteLine("Moving files to their location...");
    foreach (string sourceFilePath in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
    {
        string destinationDirectory = Path.GetDirectoryName(sourceFilePath.Replace(extractPath, minecraftPath));
        if (!Directory.Exists(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        Console.WriteLine(Path.GetFileName(sourceFilePath) + " -> " + destinationDirectory);
        File.Move(sourceFilePath, Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath)), true);
    }
    Console.WriteLine("Files moved to their destination.");

    Console.WriteLine("Deleting downloaded files...");
    File.Delete(savePath);
    Directory.Delete(extractPath, true);
    Console.WriteLine("Downloaded files deleted.");

    Console.WriteLine();
    Console.WriteLine("Nebula SMP successfully updated!");
    Console.WriteLine("Press any key to exit...");

    Console.ReadKey();
}
catch(Exception e)
{
    Console.Error.WriteLine("An error has occured!");
    Console.Error.WriteLine(e);
}