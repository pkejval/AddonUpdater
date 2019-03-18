using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AddonUpdaterLogic;
using static AddonUpdaterCli.Utils;
using static AddonUpdaterLogic.Utils;
using System.Linq;

namespace AddonUpdaterCli
{
    internal class Program
    {
        public static Dictionary<Guid, int> lines = new Dictionary<Guid, int>();
        private static bool InteractiveMode = true;

        public static async Task Main(string[] args)
        {
            InteractiveMode = !(args.Count() > 0 && args[0] == "--script");

            Console.WriteLine("*******************");
            Console.WriteLine("*WoW Addon updater*");
            Console.WriteLine("*******************\n");

            try
            {
#if RELEASE
                var cnf_path = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.txt");
#else
                var cnf_path = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "dev_config.txt");
#endif

                var addon_updater = new AddonUpdater(cnf_path);
                addon_updater.AddonProgressUpdated += addon_updater_AddonProgressUpdated;
                await addon_updater.UpdateAll();

                Exit(addon_updater.Summary(), 0);
            }
            catch (Exception ex)
            {
#if DEBUG
                Exit(ex.ToString(), 1);
#else
                Exit(ex.Message, 1);
#endif
            }
        }

        public static void Exit(string msg = "", int code = 0)
        {
            if (!string.IsNullOrEmpty(msg)) { Console.WriteLine(msg + "\n"); }
            if (InteractiveMode)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            Environment.Exit(code);
        }

        private static void addon_updater_AddonProgressUpdated(object sender, object addon)
        {
            var a = (Addon)addon;

            switch (a.Progress)
            {
                case AddonProgress.Starting:
                    Console.WriteLine();
                    lines[a.Guid] = Console.CursorTop - 1;
                    ConsoleWrite(lines[a.Guid], a.Progress.Desc(), a.AddonName, ConsoleColor.White);
                    break;

                case AddonProgress.Done:
                    ConsoleWrite(lines[a.Guid], a.StatusVerbose, a.AddonName, ConsoleColor.Green);
                    break;

                default:
                    ConsoleWrite(lines[a.Guid], a.Progress.Desc(), a.AddonName, GetColor(a));
                    break;
            }
        }
    }
}