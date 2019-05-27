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

        private enum ReleaseType
        {
            Release,
            Beta,
            Alpha
        }

        public Addon ParseResponse(string response, Uri url)
        {
            Response = new AddonSiteResponse();

            foreach (string type in Enum.GetNames(typeof(ReleaseType)))
            {
                var r = new Regex(string.Format(@"overflow-tip\""\>(.*?)\<.*?{0}-phase.*?\/files\/(\d+).*?data-name\=\""(.*?)\""", type.ToLower()), RegexOptions.Singleline | RegexOptions.Compiled);

                Match m = r.Match(response);
                if (m.Success)
                {
                    Response.AddonName = m.Groups[1].Value;
                    Response.DownloadURL = $"{url.Scheme}://{url.Host}{url.LocalPath}{(!url.LocalPath.Contains("/files") ? "/files" : "")}/{m.Groups[2].Value}/download";
                    Response.Version = m.Groups[3].Value;

                    break;
                }
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