using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AddonUpdaterLogic.AddonSites
{
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

        public IEnumerable<string> HandleURLs { get { return new string[] { "www.curseforge.com" }; } }
    }
}