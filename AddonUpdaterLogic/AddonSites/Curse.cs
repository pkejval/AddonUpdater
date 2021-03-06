﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdaterLogic.AddonSites
{
    /// <summary>
    /// Definition for wow.curseforge.com
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
                var r = new Regex(string.Format(@"og:title"" content=""(.*?)""(?:.*?)title=""{0}""(?:.*?)data-name=""(.*?)"".*?a href="".*?(\d+)""", type), RegexOptions.Singleline);

                Match m = r.Match(response);
                if (m.Success)
                {
                    Response.AddonName = m.Groups[1].Value;
                    Response.DownloadURL = $"{url.Scheme}://{url.Host}{(url.Host == "www.curseforge.com" ? url.LocalPath.Replace("/files", "") : url.LocalPath)}/download/{m.Groups[3].Value}/file";
                    Response.Version = m.Groups[2].Value;

                    break;
                }
            }

            return null;
        }

        public string GetURL(Uri url)
        {
            return $"{url}/files?sort=releasetype";
        }

        public IEnumerable<string> HandleURLs { get { return new string[] { "wow.curseforge.com", "www.curseforge.com" }; } }
    }
}