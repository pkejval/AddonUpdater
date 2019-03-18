using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdaterLogic.Tests
{
    [TestClass]
    public class LogicTests
    {
        [TestMethod]
        public void NewAddonInstance()
        {
            Global.AddonSites = Utils.GetAllAddonSites();

            var addon_url = "https://wow.curseforge.com/projects/plater-nameplates";
            var a = new Addon(addon_url);
            Assert.AreEqual(a.Progress, AddonProgress.Starting);
            Assert.AreEqual(a.Status, AddonStatus.New);
            Assert.AreEqual(a.URL, new Uri(addon_url));
            Assert.IsNotNull(a.Guid);
        }

        [TestMethod]
        public void NewAddonInstance_NotSupportedSite()
        {
            Global.AddonSites = Utils.GetAllAddonSites();

            var a = new Addon("http://not_supported_site.com");
            Assert.AreEqual(a.Progress, AddonProgress.NotSupported);
        }

        [TestMethod]
        public void AddonUpdate_Curse()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://wow.curseforge.com/projects/plater-nameplates";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "plater nameplates");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }

        [TestMethod]
        public void AddonUpdate_CurseMeta()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://www.curseforge.com/wow/addons/plater-nameplates";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "plater nameplates");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }

        [TestMethod]
        public void AddonUpdate_WoWInterface()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://wowinterface.com/downloads/info23940-PlaterNameplates.html";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "plater nameplates");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }

        [TestMethod]
        public void AddonUpdate_WoWAce()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://www.wowace.com/projects/adibags";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "adibags");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }

        [TestMethod]
        public void AddonUpdate_TukUI1()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://www.tukui.org/download.php?ui=elvui";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "elvui");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }

        [TestMethod]
        public void AddonUpdate_TukUI2()
        {
            Global.AddonSites = Utils.GetAllAddonSites();
            var addon_url = "https://www.tukui.org/addons.php?id=3";
            var a = new Addon(addon_url);

            a.Update().Wait();

            Assert.IsNotNull(a.Response);
            Assert.AreEqual(a.Response.AddonName.ToLower(), "addonskins");
            Assert.IsInstanceOfType(a.Response.Version, typeof(string));
        }
    }
}