using AddonUpdaterLogic.AddonSites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace AddonUpdaterLogic
{
    public class AddonUpdater
    {
        public delegate void AddonProgressUpdatedEventHandler(Object sender, Object addon);

        /// <summary>
        /// Fires everytime Addon instance status is updated.
        /// </summary>
        public event AddonProgressUpdatedEventHandler AddonProgressUpdated;

        public List<Addon> Addons { get; private set; } = new List<Addon>();

        private AddonUpdater()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
        }

        /// <summary>
        /// Creates new AddonUpdater instance WITHOUT loading any config file. WoWPath and AddonUpdaterFilePath must be provided to constructor. Any addon URL must be added by AddUrl method.
        /// </summary>
        /// <param name="WoWPath"></param>
        /// <param name="AddonUpdaterFilePath"></param>
        public AddonUpdater(string WoWPath, string AddonUpdaterFilePath) : this()
        {
            Global.WoWPath = WoWPath;
            Global.AddonUpdaterFilePath = AddonUpdaterFilePath;
            Global.InstalledAddons = Utils.ParseAddonUpdaterFile(Global.AddonUpdaterFilePath);
        }

        /// <summary>
        /// Creates new AddonUpdater instance and loads all information from config file provided to constructor.
        /// </summary>
        /// <param name="ConfigFilePath"></param>
        public AddonUpdater(string ConfigFilePath) : this()
        {
            var parse = Utils.ParseConfigFile(ConfigFilePath);

            foreach (var url in parse.Item1)
            {
                Addons.Add(new Addon(url));
            }

            Global.WoWPath = parse.Item2;
            Global.AddonUpdaterFilePath = parse.Item3;
            Global.InstalledAddons = Utils.ParseAddonUpdaterFile(Global.AddonUpdaterFilePath);
        }

        /// <summary>
        /// Clears whole addon collection.
        /// </summary>
        public void ClearUrls()
        {
            Addons.Clear();
        }

        /// <summary>
        /// Adds new Addon to addon collection.
        /// </summary>
        /// <param name="url"></param>
        public void AddUrl(string url)
        {
            if (Addons.Count(x => x.URL.OriginalString == url) == 0)
            {
                Addons.Add(new Addon(url));
            }
        }

        /// <summary>
        /// Starts .Update() method for every Addon instance in addons collection in parallel. Progress is reported by OnAddonProgressUpdate event.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAll()
        {
            if (Addons == null || Addons.Count == 0) { throw new Exception("Addon list is empty!"); }

            foreach (var addon in Addons)
            {
                addon.PropertyChanged += Addon_PropertyChanged;
            }

            // execute update task for each addon with 2 minutes timeout
            var tasks = Addons.Distinct().Select(x => x.Update());
            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(120000));

            Save();
        }

        /// <summary>
        /// Saves all installed addons and their version into JSON file.
        /// </summary>
        private void Save()
        {
            // update version dict
            Addons.ForEach(x => { if (x.Response != null && !string.IsNullOrEmpty(x.Response.Version)) Global.InstalledAddons[x.URL.OriginalString] = x.Response?.Version; });
            // save versions dict to file
            File.WriteAllText(Global.AddonUpdaterFilePath, JsonConvert.SerializeObject(Global.InstalledAddons));
        }

        /// <summary>
        /// Returns summary of how much addons up-to-date, installed, updated and errors.
        /// </summary>
        /// <returns></returns>
        public string Summary()
        {
            return $"\n{Addons.Count(x => x.Status == AddonStatus.UpToDate)} up-to-date | {Addons.Count(x => x.Status == AddonStatus.New)} installed | {Addons.Count(x => x.Status == AddonStatus.Updated)} updated | {Addons.Count(x => x.Status == AddonStatus.Error || x.Progress == AddonProgress.Error)} errors";
        }

        protected virtual void OnAddonProgressUpdate(Object addon)
        {
            AddonProgressUpdatedEventHandler handler = AddonProgressUpdated;
            if (handler != null)
            {
                handler(this, addon);
            }
        }

        private void Addon_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnAddonProgressUpdate(sender);
        }
    }
}