using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdater.Class
{
    /// <summary>
    /// Interface for each addon website
    /// </summary>
    internal interface IAddonSite
    {
        string Name { get; }
        AddonSiteResponse Response { get; }

        void ParseResponse(string response, Uri url);

        string GetURL(Uri url);
    }

    public class AddonSiteResponse
    {
        public string Version { get; set; }
        public string DownloadURL { get; set; }
    }

    public class AddonSites
    {
        /// <summary>
        /// Definition for wow.cursedforge.com
        /// </summary>
        public class Curse : IAddonSite
        {
            public string Name { get { return "Curse"; } }
            public AddonSiteResponse Response { get; private set; }

            public void ParseResponse(string response, Uri url)
            {
                Response = new AddonSiteResponse();
                var r = new Regex(@"release-phase.*?\/files\/(\d+).*?data-name\=\""(.*?)\""", RegexOptions.Singleline | RegexOptions.Compiled);

                Match m = r.Match(response);
                if (m.Success)
                {
                    Response.DownloadURL = $"{url.OriginalString}/{m.Groups[1].Value}/download";
                    Response.Version = m.Groups[2].Value;
                }
            }

            public string GetURL(Uri url)
            {
                return $"{url.ToString()}/files";
            }
        }

        /// <summary>
        /// Definition for wowinterface.com
        /// </summary>
        public class WoWInterface : IAddonSite
        {
            public string Name { get { return "WoWInterface"; } }
            public AddonSiteResponse Response { get; private set; }

            public void ParseResponse(string response, Uri url)
            {
                Response = new AddonSiteResponse();

                var r1 = new Regex(@"Version\:\s(.*?)\s", RegexOptions.Singleline | RegexOptions.Compiled);
                Match m1 = r1.Match(response);
                Response.Version = m1.Groups[1].Value;

                // Extract ID from URL and use it to construct download link
                Regex r2 = new Regex(@"info(\d+)", RegexOptions.Compiled);
                Match m2 = r2.Match(url.Segments[2]);

                Response.DownloadURL = $"https://cdn.wowinterface.com/downloads/file{m2.Groups[1].Value}/";
            }

            public string GetURL(Uri url)
            {
                return $"{url}#info";
            }
        }

        /// <summary>
        /// Definition for tukui.org
        /// </summary>
        public class TukUI : IAddonSite
        {
            public string Name { get { return "TukUI"; } }
            public AddonSiteResponse Response { get; private set; }

            public void ParseResponse(string response, Uri url)
            {
                Response = new AddonSiteResponse();

                if (url.LocalPath == "/addons.php")
                {
                    var r = new Regex(@"The latest version of this addon is <b class=""VIP"">(.*?)\<\/b>");
                    Match m = r.Match(response);
                    if (m.Success)
                    {
                        Response.DownloadURL = url.ToString().Replace("id=", "download=");
                        Response.Version = m.Groups[1].Value;
                    }
                }
                else if (url.LocalPath == "/download.php")
                {
                    var r = new Regex(@"(?:(\/downloads\/(.*?)\"")|(Version\s(.*?)\s))", RegexOptions.Singleline | RegexOptions.Compiled);
                    var matches = r.Matches(response);
                    Response.DownloadURL = matches[0].Success ? $"{url.Scheme}://{url.Host}/downloads/{matches[0].Groups[2].Value}" : "";
                    Response.Version = matches[1].Success ? matches[1].Groups[4].Value : "";
                }
            }

            public string GetURL(Uri url)
            {
                return url.PathAndQuery;
            }
        }
    }
}