using AddonUpdaterLogic.AddonSites;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AddonUpdaterLogic
{
    public static class Utils
    {
        /// <summary>
        /// Returns all classes implementing IAddonSite interface. Key = site URL, Value = Type
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Type> GetAllAddonSites()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            var assembly = Assembly.GetEntryAssembly();
            //var assemblies = assembly.GetReferencedAssemblies();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("AddonUpdater"));

            foreach (var assemblyName in assemblies)
            {
                assembly = Assembly.Load(assemblyName.FullName);

                foreach (var ti in assembly.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(IAddonSite)))
                    {
                        var instance = (IAddonSite)assembly.CreateInstance(ti.FullName);
                        foreach (var url in instance.HandleURLs)
                        {
                            types.Add(url, instance.GetType());
                        }
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// Returns Tuple which Item1 = list of addons URL, Item2 = path to WoW Interface/Addons folder
        /// </summary>
        /// <param name="ConfigFilePath"></param>
        /// <returns></returns>
        public static Tuple<IEnumerable<string>, string, string> ParseConfigFile(string ConfigFilePath)
        {
            List<string> addons = new List<string>();
            string WoWPath = "";
            string CnfPath = "";

            if (!File.Exists(ConfigFilePath) || new FileInfo(ConfigFilePath).Length == 0)
            {
                File.WriteAllText(ConfigFilePath, Global.ExampleConfig);
                throw new FileNotFoundException("Config file not found");
                //Exit($"Config file wasn't found at {ConfigFilePath}! Creating example file. Please open it and set WOW_PATH and addons URLs.", 1);
            }
            else
            {
                // parse config.txt
                using (var sr = new StreamReader(ConfigFilePath))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) { continue; }

                        // WoW path
                        if (line.StartsWith("WOW_PATH="))
                        {
                            WoWPath = line.Replace("WOW_PATH=", "");
                        }
                        // AddonUpdater.json path
                        else if (line.StartsWith("CNF_PATH="))
                        {
                            CnfPath = line.Replace("CNF_PATH=", "");
                        }
                        // Addon URL
                        else { addons.Add(line); }
                    }
                }

                if (string.IsNullOrEmpty(WoWPath)) { throw new Exception("WoWPath not set in config file!"); }
                else { WoWPath = Path.Combine(WoWPath.Replace("WOW_PATH=", ""), "_retail_", "Interface", "Addons"); }
            }

            return new Tuple<IEnumerable<string>, string, string>(addons.Distinct(), WoWPath, string.IsNullOrEmpty(CnfPath) ? Path.Combine(WoWPath, "AddonUpdater.json") : CnfPath);
        }

        /// <summary>
        /// Parse saved AddonUpdater.json. Returns empty dict if file not found or corrupted.
        /// </summary>
        /// <param name="AddonUpdaterFilePath"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseAddonUpdaterFile(string AddonUpdaterFilePath)
        {
            Dictionary<string, string> installed_addons = new Dictionary<string, string>();

            if (File.Exists(AddonUpdaterFilePath))
            {
                try { installed_addons = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Global.AddonUpdaterFilePath)); }
                catch { installed_addons = new Dictionary<string, string>(); }

                // reinit dict when file is empty or currupted = variable is null
                if (installed_addons == null)
                {
                    installed_addons = new Dictionary<string, string>();
                }
            }

            return installed_addons;
        }

        public static string Desc(this Enum value)
        {
            // get attributes
            var field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            // return description
            return attributes.Any() ? ((DescriptionAttribute)attributes.ElementAt(0)).Description : "Description Not Found";
        }
    }
}