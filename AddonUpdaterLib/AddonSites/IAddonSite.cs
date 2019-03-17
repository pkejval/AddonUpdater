using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdaterLogic.AddonSites
{
    /// <summary>
    /// Interface for each addon website
    /// </summary>
    internal interface IAddonSite
    {
        /// <summary>
        /// Parsed response from addon site. Contains newest version string and download URL string.
        /// </summary>
        AddonSiteResponse Response { get; }

        /// <summary>
        /// Parse HTML page for download link and newest version info. If return Addon instance it means that IAddonSite already handled looking up for new version and downloading...
        /// Just remember to set properties from returned Addon instance to parent Addon instance by yourself!
        /// </summary>
        /// <param name="response"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        Addon ParseResponse(string response, Uri url);

        /// <summary>
        /// Returns URL where to lookup for new version info.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string GetURL(Uri url);

        IEnumerable<string> HandleURLs { get; }
    }

    /// <summary>
    /// Information parsed from addon site.
    /// </summary>
    public class AddonSiteResponse
    {
        public string AddonName { get; set; }
        public string Version { get; set; }
        public string DownloadURL { get; set; }
    }
}
