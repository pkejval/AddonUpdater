using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AddonUpdater.Class
{
    public class Addon
    {
        public Uri URL { get; private set; }
        public bool New { get; private set; }
        public bool Updated { get; private set; }
        public bool Error { get; private set; }
        public string InstalledVersion { get; private set; }
        public AddonSiteResponse Response { get; private set; }
        private string DownloadedFilePath { get; set; }

        public Addon(string URL)
        {
            this.URL = new Uri(URL);
        }

        public async Task Update()
        {
            try
            {
                bool download = true;
                InstalledVersion = Global.InstalledAddons.ContainsKey(URL.OriginalString) ? Global.InstalledAddons[URL.OriginalString] : "";

                Dictionary<string, IAddonSite> sites = new Dictionary<string, IAddonSite>()
                {
                    { "wow.curseforge.com", new AddonSites.Curse() },
                    { "www.wowace.com", new AddonSites.Curse() }, // wowace is the same logic as curseforge with another skin/domain
                    { "www.curseforge.com", new AddonSites.MetaCurseForgePage() }, // just lookup link for wow.curseforge.com or www.wowace.com
                    { "wowinterface.com", new AddonSites.WoWInterface() },
                    { "www.tukui.org", new AddonSites.TukUI() }
                };

                if (!sites.ContainsKey(URL.Host)) { Console.WriteLine($"Unsupported addon site: {URL.Host}"); Error = true; return; }

                using (var client = new HttpClient())
                {
                    // lookup addon website
                    var site = sites[URL.Host];
                    client.BaseAddress = new Uri(URL.Scheme + "://" + URL.Host);

                    using (var request = await client.GetAsync(site.GetURL(URL)))
                    {
                        var response = await request.Content.ReadAsStringAsync();
                        var addon = site.ParseResponse(response, request.RequestMessage.RequestUri);
                        Response = site.Response;

                        // Addon was handled by IAddonSite - just set properties from it
                        if (addon != null)
                        {
                            InstalledVersion = addon.InstalledVersion;
                            New = addon.New;
                            Updated = addon.Updated;
                            Error = addon.Error;
                            download = false;
                        }
                    }

                    // download and extract only if website version is different
                    if (download && (string.IsNullOrEmpty(InstalledVersion) || InstalledVersion != Response.Version))
                    {
                        if (string.IsNullOrEmpty(InstalledVersion)) { New = true; }
                        else { Updated = true; }

                        Console.WriteLine($"Downloading {URL.OriginalString} - {(New ? "not installed" : $"new version {Response.Version}")}");

                        Download(client);

                        if (Error) { Console.WriteLine($"{URL.OriginalString} - ERROR"); return; }

                        ZipFile.ExtractToDirectory(DownloadedFilePath, Global.WoWPath, true);
                        File.Delete(DownloadedFilePath);
                        Global.InstalledAddons[URL.OriginalString] = Response.Version;
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); Error = true; return; }
        }

        /// <summary>
        /// Downloads file from addon website
        /// </summary>
        /// <param name="client"></param>
        private void Download(HttpClient client)
        {
            try
            {
                var tmp = Path.GetTempFileName();

                using (var c = new WebClient())
                {
                    c.DownloadFile(Response.DownloadURL, tmp);
                    DownloadedFilePath = tmp;
                }
            }
            catch { Error = true; }
        }
    }
}