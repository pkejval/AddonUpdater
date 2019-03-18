using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdaterLogic.AddonSites
{
    /// <summary>
    /// Definition for tukui.org
    /// </summary>
    public class TukUI : IAddonSite
    {
        public AddonSiteResponse Response { get; private set; }

        public Addon ParseResponse(string response, Uri url)
        {
            Response = new AddonSiteResponse();

            if (url.LocalPath == "/addons.php")
            {
                var r = new Regex(@"(?:<span class=""Member"">(.*?)\<|The latest version of this addon is <b class=""VIP"">(.*?)\<\/b>)", RegexOptions.Compiled);
                var m = r.Matches(response);

                if (m.Count >= 2)
                {
                    Response.AddonName = m[0].Groups[1].Value;
                    Response.DownloadURL = url.ToString().Replace("id=", "download=");
                    Response.Version = m[1].Groups[2].Value;
                }
            }
            else if (url.LocalPath == "/download.php")
            {
                var r = new Regex(@"(?:\"">(.*?)<\/h1>|(?:\/downloads\/)(.*?)\""|Premium\""\>(.*?)\<\/b\>)", RegexOptions.Compiled);
                var m = r.Matches(response);
                if (m.Count > 2)
                {
                    Response.AddonName = m[0].Groups[1].Value;
                    Response.Version = m[2].Groups[3].Value;
                    Response.DownloadURL = m[1].Success ? $"{url.Scheme}://{url.Host}/downloads/{m[1].Groups[2].Value}" : "";
                }
            }

            return null;
        }

        public string GetURL(Uri url)
        {
            return url.PathAndQuery;
        }

        public IEnumerable<string> HandleURLs { get { return new string[] { "www.tukui.org" }; } }
    }
}