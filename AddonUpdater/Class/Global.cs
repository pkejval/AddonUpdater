using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdater.Class
{
    public static class Global
    {
        /// <summary>
        /// Holds all supported addon sites and its types
        /// </summary>
        public static Dictionary<string, Type> AddonSites = new Dictionary<string, Type>()
        {
            { "wow.curseforge.com", typeof(AddonSites.Curse) },
            { "www.wowace.com", typeof(AddonSites.Curse) }, // wowace is the same logic as curseforge with another skin/domain
            { "www.curseforge.com", typeof(AddonSites.MetaCurseForgePage) }, // just lookup link for wow.curseforge.com or www.wowace.com
            { "wowinterface.com", typeof(AddonSites.WoWInterface)},
            { "www.tukui.org", typeof(AddonSites.TukUI) }
        };

        /// <summary>
        /// Holds information about installed addons (URLs) and its versions
        /// </summary>
        public static Dictionary<string, string> InstalledAddons = new Dictionary<string, string>();

        public static string WoWPath;
        public static string AddonUpdaterFilePath;
        public static bool InteractiveMode = true;
    }
}