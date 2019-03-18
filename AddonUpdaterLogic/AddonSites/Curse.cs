using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdaterLogic.AddonSites
{
    /// <summary>
    /// Definition for wow.cursedforge.com
    /// </summary>
    public class Curse : IAddonSite
    {
        public AddonSiteResponse Response { get; private set; }

        public Addon ParseResponse(string response, Uri url)
        {
            Response = new AddonSiteResponse();
            var r = new Regex(@"overflow-tip\""\>(.*?)\<.*?release-phase.*?\/files\/(\d+).*?data-name\=\""(.*?)\""", RegexOptions.Singleline | RegexOptions.Compiled);

            Match m = r.Match(response);
            if (m.Success)
            {
                Response.AddonName = m.Groups[1].Value;
                Response.DownloadURL = $"{url.Scheme}://{url.Host}{url.LocalPath}{(!url.LocalPath.Contains("/files") ? "/files" : "")}/{m.Groups[2].Value}/download";
                Response.Version = m.Groups[3].Value;
            }

            return null;
        }

        public string GetURL(Uri url)
        {
            return $"{url}/files?sort=releasetype";
        }

        public IEnumerable<string> HandleURLs { get { return new string[] { "wow.curseforge.com", "www.wowace.com" }; } }
    }
}
