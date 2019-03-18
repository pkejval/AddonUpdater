using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AddonUpdaterLogic.Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void GetClassesImplementingIAddonSite()
        {
            var types = Utils.GetAllAddonSites();
            Assert.IsNotNull(types);
            Assert.AreNotEqual(types.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ParseEmptyConfigFile()
        {
            Utils.ParseConfigFile(Path.GetTempFileName());
        }

        [TestMethod]
        public void ParseConfigFile()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, Global.ExampleConfig);
            var cnf = Utils.ParseConfigFile(tmp);

            Assert.IsNotNull(cnf);
            Assert.AreNotEqual(cnf.Item1.Count(), 0);
            Assert.AreEqual(cnf.Item1.Count(), 2);
            Assert.AreEqual(cnf.Item1.Contains("https://wow.curseforge.com/projects/plater-nameplates") && cnf.Item1.Contains("https://www.tukui.org/download.php?ui=elvui"), true);
            Assert.AreEqual(cnf.Item2.Replace('/', '\\'), @"C:\Program Files (x86)\Battle.NET\World of Warcraft\_retail_\Interface\Addons");
            Assert.AreEqual(cnf.Item3.Replace('/', '\\'), @"C:\Program Files (x86)\Battle.NET\World of Warcraft\_retail_\Interface\Addons\AddonUpdater.json");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ParseConfigWithoutPath()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "WOW_PATH=");
            var cnf = Utils.ParseConfigFile(tmp);
        }

        [TestMethod]
        public void EnumToDescription()
        {
            Assert.IsInstanceOfType(AddonStatus.Error.Desc(), typeof(string));
        }
    }
}