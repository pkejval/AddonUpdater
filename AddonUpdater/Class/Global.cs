using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdater.Class
{
    public static class Global
    {
        public static Dictionary<string, string> InstalledAddons = new Dictionary<string, string>();
        public static string WoWPath;
        public static string AddonUpdaterFilePath;
    }
}