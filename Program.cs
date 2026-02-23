using Microsoft.FSharp.Core;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text.Json;

// Lotus No-Bloat

class Program
{

    const int GWL_STYLE = -16;
    const int WS_SIZEBOX = 0x00040000; // stile ridimensionabile
    const int WS_MAXIMIZEBOX = 0x00010000; // bottone massimizza

    [DllImport("kernel32.dll", ExactSpelling = true)]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    static void BlockResize()
    {
        var handle = GetConsoleWindow();
        int style = GetWindowLong(handle, GWL_STYLE);
        style &= ~WS_SIZEBOX;     // rimuove ridimensionamento manuale
        style &= ~WS_MAXIMIZEBOX; // rimuove bottone massimizza
        SetWindowLong(handle, GWL_STYLE, style);
    }

    static string loadedScriptPath = null;
    static dynamic scriptLoadInstance = null;
    static dynamic scriptDetailsInstance = null;

    static void Main()
    {
        while (true)
        {
            Console.Clear();

            if (!IsWindows11())
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("This tool is only supported on Windows 11.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            string gui = @"
/$$                   /$$                               /$$   /$$                   /$$$$$$$  /$$                       /$$    
| $$                  | $$                              | $$$ | $$                  | $$__  $$| $$                      | $$    
| $$        /$$$$$$  /$$$$$$   /$$   /$$  /$$$$$$$      | $$$$| $$  /$$$$$$         | $$  \ $$| $$  /$$$$$$   /$$$$$$  /$$$$$$  
| $$       /$$__  $$|_  $$_/  | $$  | $$ /$$_____/      | $$ $$ $$ /$$__  $$ /$$$$$$| $$$$$$$ | $$ /$$__  $$ |____  $$|_  $$_/  
| $$      | $$  \ $$  | $$    | $$  | $$|  $$$$$$       | $$  $$$$| $$  \ $$|______/| $$__  $$| $$| $$  \ $$  /$$$$$$$  | $$    
| $$      | $$  | $$  | $$ /$$| $$  | $$ \____  $$      | $$\  $$$| $$  | $$        | $$  \ $$| $$| $$  | $$ /$$__  $$  | $$ /$$
| $$$$$$$$|  $$$$$$/  |  $$$$/|  $$$$$$/ /$$$$$$$/      | $$ \  $$|  $$$$$$/        | $$$$$$$/| $$|  $$$$$$/|  $$$$$$$  |  $$$$/ 
|________/ \______/    \___/   \______/ |_______/       |__/  \__/ \______/         |_______/ |__/ \______/  \_______/   \___/                                                                                                                                                         
";

            int maxWidth = 150; // larghezza massima fissa
            int guiWidth = Math.Min(gui.Length + 5, maxWidth);
            int height = 40;

            Console.SetWindowSize(guiWidth, height);
            Console.SetBufferSize(maxWidth, height);

            BlockResize();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(gui);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Lotus No-Bloat by MasterSharp, 85cs, LightExists, Itelcan3 - using Airone - https://discord.gg/4KqWrwbNFy");
            Console.ResetColor();

            Console.WriteLine("\nPress D to start debloat or R to restore or L to load F# script.");

            if (scriptDetailsInstance != null)
            {
                Console.WriteLine($"> Loaded Script: {scriptDetailsInstance.Name} v{scriptDetailsInstance.Version} by {scriptDetailsInstance.Author}");
            }

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.L)
            {
                Console.Write("\nEnter full path of F# script (.fsx): ");
                string path = Console.ReadLine().Trim().Trim('"'); // rimuove i doppi apici

                if (File.Exists(path))
                {
                    loadedScriptPath = path;
                    LoadFSharpScript(path);
                    Console.WriteLine("> Script loaded successfully.");
                }
                else
                {
                    Console.WriteLine("> File not found.");
                }

                Console.WriteLine("Press Enter to return to home...");
                Console.ReadLine();
                continue;
            }

            if (key.Key == ConsoleKey.R)
            {
                RestoreApps();
                Console.WriteLine("\n> Restore completed.");
                Console.ReadLine();
                continue;
            }

            string aironeList = @"
Get-AppxPackage *Xbox* | Remove-AppxPackage
Get-AppxPackage *BingNews* | Remove-AppxPackage
Get-AppxPackage *BingWeather* | Remove-AppxPackage
Get-AppxPackage *MicrosoftSolitaireCollection* | Remove-AppxPackage
Get-AppxPackage *People* | Remove-AppxPackage
Get-AppxPackage *GetHelp* | Remove-AppxPackage
Get-AppxPackage *Getstarted* | Remove-AppxPackage
Get-AppxPackage *ZuneMusic* | Remove-AppxPackage
Get-AppxPackage *ZuneVideo* | Remove-AppxPackage
Get-AppxPackage *SkypeApp* | Remove-AppxPackage
Get-AppxPackage *Clipchamp* | Remove-AppxPackage
Get-AppxPackage *YourPhone* | Remove-AppxPackage
Get-AppxPackage *MicrosoftTeams* | Remove-AppxPackage
";

            if (key.Key == ConsoleKey.D)
            {
                BackupCurrentApps("backup_apps.json");

                if (scriptLoadInstance != null)
                {
                    var apps = scriptLoadInstance.RemoveApps as string[];

                    if (apps != null && apps.Length > 0)
                    {
                        string script = string.Join("\n",
                            apps.Select(app =>
                                $"Get-AppxPackage *{app}* | Remove-AppxPackage"));

                        PowerShellRunner.Run(script);
                    }

                    CleanCache(new CleanOptions
                    {
                        Temp = scriptLoadInstance.CleanTemp,
                        Prefetch = scriptLoadInstance.CleanPrefetch,
                        INetCache = scriptLoadInstance.CleanINet,
                        DXCache = scriptLoadInstance.CleanDXCache,
                        WUCache = scriptLoadInstance.CleanWUCache
                    });
                }
                else
                {
                    PowerShellRunner.Run(aironeList);
                    CleanCache(new CleanOptions { Temp = true, Prefetch = true, INetCache = true, DXCache = true, WUCache = true });
                }

                Console.WriteLine("\nDebloat completed.");
                Console.ReadLine();
            }
        }
    }

    static void LoadFSharpScript(string path)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"fsi \"{path}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // ScriptDetails
            string name = lines.FirstOrDefault(l => l.StartsWith("Name:"))?.Substring(5).Trim() ?? "";
            string author = lines.FirstOrDefault(l => l.StartsWith("Author:"))?.Substring(7).Trim() ?? "";
            string version = lines.FirstOrDefault(l => l.StartsWith("Version:"))?.Substring(8).Trim() ?? "";

            scriptDetailsInstance = new
            {
                Name = name,
                Author = author,
                Version = version
            };

            // ScriptLoad
            var removeAppsLine = lines.FirstOrDefault(l => l.StartsWith("RemoveApps:"));
            string[] removeApps = removeAppsLine != null
                ? removeAppsLine.Substring(11).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray()
                : new string[0];

            scriptLoadInstance = new
            {
                RemoveApps = removeApps,
                CleanTemp = lines.Any(l => l.StartsWith("CleanTemp:true")),
                CleanPrefetch = lines.Any(l => l.StartsWith("CleanPrefetch:true")),
                CleanINet = lines.Any(l => l.StartsWith("CleanINet:true")),
                CleanDXCache = lines.Any(l => l.StartsWith("CleanDXCache:true")),
                CleanWUCache = lines.Any(l => l.StartsWith("CleanWUCache:true"))
            };
        }
    }

    static bool IsWindows11()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    }

    static void BackupCurrentApps(string path)
    {
        using (PowerShell ps = PowerShell.Create())
        {
            ps.AddScript("Get-AppxPackage -AllUsers | Select Name, PackageFullName");
            var results = ps.Invoke();

            var apps = results.Select(r => new
            {
                Name = r.Members["Name"].Value?.ToString(),
                PackageFullName = r.Members["PackageFullName"].Value?.ToString()
            }).ToList();

            File.WriteAllText(path, JsonSerializer.Serialize(apps, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    static void CleanCache(CleanOptions options)
    {
        if (options.Temp)
        {
            string path = Path.GetTempPath();
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
        }

        if (options.Prefetch)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
        }

        if (options.INetCache)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache");
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
        }

        if (options.DXCache)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "DXCache");
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
        }

        if (options.WUCache)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
        }
    }

    static void RestoreApps()
    {
        string script = @"
Get-AppxPackage -AllUsers | Foreach {
    Add-AppxPackage -DisableDevelopmentMode -Register ""$($_.InstallLocation)\AppXManifest.xml"" -ErrorAction SilentlyContinue
}
";

        using (PowerShell ps = PowerShell.Create())
        {
            ps.AddScript(script);
            ps.Invoke();
        }
    }
}

public class CleanOptions
{
    public bool Temp { get; set; }
    public bool Prefetch { get; set; }
    public bool INetCache { get; set; }
    public bool DXCache { get; set; }
    public bool WUCache { get; set; }
}

public static class PowerShellRunner
{
    public static void Run(string script)
    {
        using (PowerShell ps = PowerShell.Create())
        {
            ps.AddScript(script);
            ps.Invoke();
        }
    }
}