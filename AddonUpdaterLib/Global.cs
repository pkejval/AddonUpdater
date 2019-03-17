using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdaterLogic
{
    public static class Global
    {
        /// <summary>
        /// Holds all supported addon sites and its types.
        /// </summary>
        public static Dictionary<string, Type> AddonSites = new Dictionary<string, Type>();

        /// <summary>
        /// Holds information about installed addons (URLs) and its versions.
        /// </summary>
        public static Dictionary<string, string> InstalledAddons = new Dictionary<string, string>();

        /// <summary>
        /// Path to World of Warcraft/_retail_/Interface/Addons.
        /// </summary>
        public static string WoWPath;

        /// <summary>
        /// Path to AddonUpdaterFilePath DB file.
        /// </summary>
        public static string AddonUpdaterFilePath;
    }
}
