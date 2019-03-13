using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdater.Class
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

    public class AddonSites
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
                var r = new Regex(@"overflow-tip\""\>(.*?)\<.*release-phase.*?\/files\/(\d+).*?data-name\=\""(.*?)\""", RegexOptions.Singleline | RegexOptions.Compiled);

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
        }

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
        }

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
        }

        /// <summary>
        /// Definition for www.curseforge.com
        /// </summary>
        public class MetaCurseForgePage : IAddonSite
        {
            private IAddonSite Addon { get; set; }
            public AddonSiteResponse Response { get; private set; }

            public Addon ParseResponse(string response, Uri url)
            {
                var r = new Regex(@"a href=\""(.*?)\"">Visit Project Page", RegexOptions.Compiled);
                var m = r.Match(response);

                if (m.Success)
                {
                    // parsed "ordinary" (wow.curseforge.com || www.wowace.com) link, call addon method again
                    var addon = new Addon(m.Groups[1].Value);
                    addon.Update().Wait();
                    Response = addon.Response;

                    return addon;
                }

                return null;
            }

            public string GetURL(Uri uri)
            {
                return uri.OriginalString;
            }
        }
    }
}