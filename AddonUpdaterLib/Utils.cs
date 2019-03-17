using AddonUpdaterLogic.AddonSites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AddonUpdaterLogic
{
    static class Utils
    {
        public static Dictionary<string, Type> GetAllAddonSites()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            var assembly = Assembly.GetEntryAssembly();
            var assemblies = assembly.GetReferencedAssemblies();

            foreach (var assemblyName in assemblies)
            {
                assembly = Assembly.Load(assemblyName);

                foreach (var ti in assembly.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(IAddonSite)))
                    {
                        var instance = (IAddonSite)assembly.CreateInstance(ti.FullName);
                        foreach (var url in instance.HandleURLs)
                        {
                            types.Add(url, instance.GetType());
                        }
                    }
                }
            }

            return types;
        }
    }
}
