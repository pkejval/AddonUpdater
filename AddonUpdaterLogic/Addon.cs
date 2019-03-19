using AddonUpdaterLogic.AddonSites;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AddonUpdaterLogic
{
    public enum AddonStatus
    {
        [Description("NEW")]
        New,

        [Description("UPDATED")]
        Updated,

        [Description("NO UPDATE")]
        UpToDate,

        [Description("ERROR")]
        Error
    }

    public enum AddonProgress
    {
        [Description("Starting")]
        Starting,

        [Description("Searching")]
        Searching,

        [Description("Downloading")]
        Downloading,

        [Description("Extracting")]
        Extracting,

        [Description("DONE")]
        Done,

        [Description("NOT SUPPORTED")]
        NotSupported,

        [Description("WAITING")]
        Waiting,

        [Description("ERROR")]
        Error
    }

    public class Addon : IEquatable<Addon>, INotifyPropertyChanged
    {
        public Guid Guid { get; private set; } = Guid.NewGuid();

        public delegate void AddonUpdaterEvent(object sender);

        public event PropertyChangedEventHandler PropertyChanged;

        public Uri URL { get; private set; }

        public AddonStatus Status { get; set; }

        public AddonProgress Progress
        {
            get { return progress; }
            set { progress = value; OnPropertyChanged("Progress"); }
        }

        private AddonProgress progress { get; set; }

        public string InstalledVersion { get; private set; }
        public AddonSiteResponse Response { get; private set; }
        private string DownloadedFilePath { get; set; }
        private bool ShouldDownload { get; set; } = true;

        public string AddonName { get { return !string.IsNullOrEmpty(Response?.AddonName) ? $"{Response.AddonName} {(string.IsNullOrEmpty(InstalledVersion) ? "" : $"({InstalledVersion})")}" : URL.OriginalString; } }
        public string StatusVerbose { get { return Status != AddonStatus.Error ? (Status == AddonStatus.New ? $"INSTALLED" : Status == AddonStatus.Updated ? $"UPDATED ({Response.Version})" : AddonStatus.UpToDate.Desc()) : AddonProgress.Error.Desc(); } }

        public string ProgressVerbose
        {
            get
            {
                switch (Progress)
                {
                    case AddonProgress.Done:
                        return StatusVerbose;

                    default:
                        return Progress.Desc();
                }
            }
        }

        public Addon(string URL)
        {
            this.URL = new Uri(URL);
            if (!Global.AddonSites.ContainsKey(this.URL.Host)) { Progress = AddonProgress.NotSupported; Status = AddonStatus.Error; return; }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        private void Error()
        {
            Progress = AddonProgress.Error;
            Status = AddonStatus.Error;
        }

        private async Task<bool> LookupSite(HttpClient client)
        {
            // lookup addon website
            Progress = AddonProgress.Searching;

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
                            Progress = addon.Progress;
                            URL = addon.URL;
                            ShouldDownload = false;
                        }

                        connection_problem = false;
                    }
                }
                // If Http exception - wait 5s and try again.... after 24 tries = 2 minutes, fail
                catch (HttpRequestException hre) { Debug.WriteLine(hre); iter++; if (iter >= 24) { break; } Progress = AddonProgress.Waiting; Thread.Sleep(5000); connection_problem = true; }
                catch (Exception ex) { Debug.WriteLine(ex); }
            }

            return !connection_problem;
        }

        private void Extract()
        {
            try
            {
                Progress = AddonProgress.Extracting;
                FastZip zip = new FastZip();
                zip.ExtractZip(DownloadedFilePath, Global.WoWPath, null);
                File.Delete(DownloadedFilePath);
            }
            catch (Exception ex) { Debug.WriteLine(ex); Error(); }
        }

        /// <summary>
        /// Does all the work.
        /// Looks for new addon version, downloads it and extracts it to WOW_PATH directory.
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            var client = new HttpClient();

            try
            {
                Progress = AddonProgress.Starting;

                InstalledVersion = Global.InstalledAddons.ContainsKey(URL.OriginalString) ? Global.InstalledAddons[URL.OriginalString] : "";

                if (!await LookupSite(client)) { Error(); return; }
                if (Response == null || string.IsNullOrEmpty(Response.DownloadURL)) { throw new Exception("Response empty!"); }

                // download and extract only if website version is different
                if (ShouldDownload && (string.IsNullOrEmpty(InstalledVersion) || InstalledVersion != Response.Version))
                {
                    if (string.IsNullOrEmpty(InstalledVersion)) { Status = AddonStatus.New; }
                    else { Status = AddonStatus.Updated; }

                    if (await Download(client)) { Extract(); }
                    else { return; }
                }
                else if (!ShouldDownload && Status == AddonStatus.New) { Status = AddonStatus.New; }
                else if (InstalledVersion == Response.Version) { Status = AddonStatus.UpToDate; }
                else { Status = AddonStatus.Updated; }

                Progress = AddonProgress.Done;
            }
            catch (Exception ex) { Debug.WriteLine(ex); Error(); return; }
            finally { client?.Dispose(); }
        }

        /// <summary>
        /// Downloads file from addon website.
        /// </summary>
        /// <param name="client"></param>
        private async Task<bool> Download(HttpClient client)
        {
            Progress = AddonProgress.Downloading;

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
            catch (Exception ex) { Debug.WriteLine(ex); Error(); return false; }
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