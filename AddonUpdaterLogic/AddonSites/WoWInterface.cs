using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdaterLogic.AddonSites
{
    /// <summary>
    /// Definition for wowinterface.com
    /// </summary>
    public class WoWInterface : IAddonSite
    {
        public AddonSiteResponse Response { get; private set; }

        public Addon ParseResponse(string response, Uri url)
        {
            Response = new AddonSiteResponse();

            var r = new Regex(@"<h1>(.*?)\&nbsp.*Version\:\s(.*?)(?:\s|\<).*fileid=(\d+)", RegexOptions.Singleline | RegexOptions.Compiled);
            Match m = r.Match(response);
            if (m.Success)
            {
                Response.AddonName = m.Groups[1].Value;
                Response.Version = m.Groups[2].Value;
                Response.DownloadURL = $"https://cdn.wowinterface.com/downloads/file{m.Groups[3].Value}/";
            }

            return null;
        }

        public string GetURL(Uri url)
        {
            return $"{url}#info";
        }

        public IEnumerable<string> HandleURLs { get { return new string[] { "wowinterface.com" }; } }
    }
}