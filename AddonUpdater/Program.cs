using System;
using System.IO;
using System.Threading.Tasks;
using AddonUpdater.Class;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace AddonUpdater
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("*******************");
            Console.WriteLine("*WoW Addon updater*");
            Console.WriteLine("*******************\n");

            Global.InteractiveMode = !(args.Count() > 0 && args[0] == "--script");
            var addons = new List<Addon>();
#if RELEASE
            var cnf_path = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.txt");
#else
            var cnf_path = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "dev_config.txt");
#endif
            if (!File.Exists(cnf_path))
            {
                File.WriteAllText(cnf_path, "# Set path to your World of Warcraft installation\nWOW_PATH=C:\\Program Files (x86)\\Battle.NET\\World of Warcraft\n\n# Set list of addons URLs - delimited by new line (ENTER)\n#https://wow.curseforge.com/projects/plater-nameplates\n##https://www.tukui.org/download.php?ui=elvui");
                Exit($"Config file wasn't found at {cnf_path}! Creating example. Please set WOW_PATH and addons URLs.", 1);
            }
            else
            {
                // parse config.txt
                using (var sr = new StreamReader(cnf_path))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) { continue; }

                        // WoW path
                        if (line.StartsWith("WOW_PATH="))
                        {
                            if (!string.IsNullOrEmpty(Global.WoWPath)) { Console.WriteLine("Warning! You have multiple WOW_PATH set in config.txt! Using last found path."); }
                            Global.WoWPath = Path.Combine(line.Substring(9, line.Length - 9), "_retail_", "Interface", "Addons");
                            if (!Directory.Exists(Global.WoWPath)) { Exit("WoW cannot be found!", 1); }
                            Global.AddonUpdaterFilePath = Path.Combine(Global.WoWPath, "AddonUpdater.json");
                        }
                        // Addon URL
                        else { addons.Add(new Addon(line)); }
                    }
                }

                if (string.IsNullOrEmpty(Global.WoWPath)) { Exit("WOW_PATH isn't set in config file!", 1); }
            }

            // load AddonUpdater saved version file
            if (File.Exists(Global.AddonUpdaterFilePath))
            {
                try { Global.InstalledAddons = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Global.AddonUpdaterFilePath)); }
                catch { Global.InstalledAddons = new Dictionary<string, string>(); }
            }

            Console.WriteLine("Fetching new updates...");

            // execute update task for each addon with 2 minutes timeout
            var tasks = addons.Select(x => x.Update());
            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(120000));

            // update version dict
            addons.ForEach(x => { Global.InstalledAddons[x.URL.OriginalString] = x.Response?.Version; });
            // save versions dict to file
            File.WriteAllText(Global.AddonUpdaterFilePath, JsonConvert.SerializeObject(Global.InstalledAddons));

            Console.WriteLine($"\n{addons.Count(x => !x.New && !x.Updated)} up-to-date | {addons.Count(x => x.New)} installed | {addons.Count(x => x.Updated)} updated | {addons.Count(x => x.Error)} errors");
            Exit("", 0);
        }

        public static void Exit(string msg = "", int code = 0)
        {
            if (!string.IsNullOrEmpty(msg)) { Console.WriteLine(msg + "\n"); }
            if (Global.InteractiveMode)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            Environment.Exit(code);
        }
    }
}