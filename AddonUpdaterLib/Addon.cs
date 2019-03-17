using AddonUpdaterLogic.AddonSites;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AddonUpdaterLogic
{
    public enum AddonStatus
    {
        New,
        Updated,
        Error
    }

    public enum AddonProgress
    {
        Searching,
        Downloading,
        Extracting,
        Done,
        NotSupported
    }

    public class Addon : IEquatable<Addon>
    {
        public Uri URL { get; private set; }
        public AddonStatus Status { get; private set; }
        public string InstalledVersion { get; private set; }
        public AddonSiteResponse Response { get; private set; }
        private string DownloadedFilePath { get; set; }

        public string AddonName { get { return !string.IsNullOrEmpty(Response?.AddonName) ? $"{Response.AddonName} ({InstalledVersion})" : URL.OriginalString; } }

        public Addon(string URL)
        {
            this.URL = new Uri(URL);
        }

        /// <summary>
        /// Does all the work.
        /// Looks for new addon version, downloads it and extracts it to WOW_PATH directory.
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            try
            {
                if (!Global.AddonSites.ContainsKey(URL.Host)) { Status = AddonStatus.Error; return; }

                bool download = true;
                InstalledVersion = Global.InstalledAddons.ContainsKey(URL.OriginalString) ? Global.InstalledAddons[URL.OriginalString] : "";

                //Print("Searching", ConsoleColor.DarkYellow);

                using (var client = new HttpClient())
                {
                    // lookup addon website
                    var site = (IAddonSite)Activator.CreateInstance(Global.AddonSites[URL.Host]);
                    client.BaseAddress = new Uri(URL.Scheme + "://" + URL.Host);

                    var iter = 0;
                    var connection_problem = true;

                    while (connection_problem)
                    {
                        try
                        {
                            using (var request = await client.GetAsync(site.GetURL(URL)))
                            {
                                var response = await request.Content.ReadAsStringAsync();
                                var addon = site.ParseResponse(response, request.RequestMessage.RequestUri);
                                Response = site.Response;

                                // Addon was handled by IAddonSite - just set properties from it
                                if (addon != null)
                                {
                                    InstalledVersion = addon.InstalledVersion;
                                    Status = addon.Status;
                                    URL = addon.URL;
                                    download = false;
                                }

                                connection_problem = false;
                            }
                        }
                        // If Http exception - wait 5s and try again.... after 24 tries = 2 minutes, fail
                        catch (HttpRequestException) { iter++; if (iter >= 24) { break; } Thread.Sleep(5000); connection_problem = true; }
                    }

                    if (connection_problem) { Status = AddonStatus.Error; return; }

                    // download and extract only if website version is different
                    if (download && (string.IsNullOrEmpty(InstalledVersion) || InstalledVersion != Response.Version))
                    {
                        if (string.IsNullOrEmpty(InstalledVersion)) { Status = AddonStatus.New; }
                        else { Status = AddonStatus.Updated; }

                        //Print($"Downloading ({Response?.Version})", ConsoleColor.Yellow);

                        if (await Download(client))
                        {
                            //Print("Extracting", ConsoleColor.DarkYellow);
                            FastZip zip = new FastZip();
                            zip.ExtractZip(DownloadedFilePath, Global.WoWPath, null);
                            File.Delete(DownloadedFilePath);
                        }

                        if (Status == AddonStatus.Error) { return; }
                    }

                    //Print(New ? "INSTALLED" : Updated ? $"UPDATED ({Response?.Version})" : Error ? "ERROR" : "NO UPDATE", New || Updated ? ConsoleColor.Green : Error ? ConsoleColor.Red : ConsoleColor.Green);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); Status = AddonStatus.Error; return; }
        }

        /// <summary>
        /// Downloads file from addon website.
        /// </summary>
        /// <param name="client"></param>
        private async Task<bool> Download(HttpClient client)
        {
            try
            {
                if (Response == null || string.IsNullOrEmpty(Response.DownloadURL)) { throw new Exception("Download URL not valid"); }

                var tmp = Path.GetTempFileName();

                using (var result = await client.GetStreamAsync(Response.DownloadURL))
                {
                    using (FileStream fs = new FileStream(tmp, FileMode.OpenOrCreate))
                    {
                        await result.CopyToAsync(fs);
                        DownloadedFilePath = tmp;
                    }
                }

                return true;
            }
            catch { Status = AddonStatus.Error; return false; }
        }

        public bool Equals(Addon addon)
        {
            return URL == addon.URL;
        }

        public override int GetHashCode()
        {
            return URL.GetHashCode();
        }
    }
}
