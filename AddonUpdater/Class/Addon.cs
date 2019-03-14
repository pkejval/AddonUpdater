﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

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
        private int ConsoleLine { get; set; }

        public string AddonName { get { return !string.IsNullOrEmpty(Response?.AddonName) ? Response.AddonName : URL.OriginalString; } }

        public Addon(string URL)
        {
            this.URL = new Uri(URL);
        }

        private void Print(string text, ConsoleColor color = ConsoleColor.White)
        {
            Global.ConsoleWrite(ConsoleLine, text, AddonName, color);
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
                Console.WriteLine();
                ConsoleLine = Console.CursorTop - 1;
                if (!Global.AddonSites.ContainsKey(URL.Host)) { Print("NOT SUPPORTED", ConsoleColor.Red); Error = true; return; }

                bool download = true;
                InstalledVersion = Global.InstalledAddons.ContainsKey(URL.OriginalString) ? Global.InstalledAddons[URL.OriginalString] : "";

                Print("Searching", ConsoleColor.DarkYellow);

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
                                    New = addon.New;
                                    Updated = addon.Updated;
                                    Error = addon.Error;
                                    URL = addon.URL;
                                    download = false;
                                }

                                connection_problem = false;
                            }
                        }
                        // If Http exception - wait 5s and try again.... after 24 tries = 2 minutes, fail
                        catch (HttpRequestException) { iter++; if (iter >= 24) { break; } Print("Waiting", ConsoleColor.DarkRed); Thread.Sleep(5000); connection_problem = true; }
                    }

                    if (connection_problem) { Error = true; return; }

                    // download and extract only if website version is different
                    if (download && (string.IsNullOrEmpty(InstalledVersion) || InstalledVersion != Response.Version))
                    {
                        if (string.IsNullOrEmpty(InstalledVersion)) { New = true; }
                        else { Updated = true; }

                        //Console.WriteLine($"Downloading {AddonName} - {(New ? "not installed" : $"new version {Response.Version}")}");
                        Print("Downloading", ConsoleColor.Yellow);

                        if (await Download(client))
                        {
                            Print("Extracting", ConsoleColor.DarkYellow);
                            FastZip zip = new FastZip();
                            zip.ExtractZip(DownloadedFilePath, Global.WoWPath, null);
                            File.Delete(DownloadedFilePath);
                        }

                        if (Error) { Print("ERROR", ConsoleColor.Red); return; }
                    }

                    Print(New ? "INSTALLED" : Updated ? "UPDATED" : Error ? "ERROR" : "NO UPDATE", New || Updated ? ConsoleColor.Green : Error ? ConsoleColor.Red : ConsoleColor.Green);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); Print("ERROR", ConsoleColor.Red); Error = true; return; }
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
            catch { Error = true; return false; }
        }
    }
}