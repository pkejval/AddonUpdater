using AddonUpdaterLogic.AddonSites;
using System;
using System.Collections.Generic;
using System.Text;

namespace AddonUpdaterLogic
{
    public class AddonUpdater
    {
        private List<Addon> Addons = new List<Addon>();

        public AddonUpdater()
        {
            var a = Utils.GetAllAddonSites();
            //foreach (var t in Utils.GetAllClassesOf<IAddonSite>())
            //{
            //    Global.AddonSites.Add("", t);
            //}
        }

        public void AddUrl(string url)
        {
            Addons.Add(new Addon(url));
        }
    }
}
