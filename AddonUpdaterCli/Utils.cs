using AddonUpdaterLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AddonUpdaterCli
{
    public static class Utils
    {
        private static object WriteLock = new object();

        /// <summary>
        /// Selects ConsoleColor based on AddonProgress property value.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static ConsoleColor GetColor(Addon a)
        {
            switch (a.Progress)
            {
                case AddonProgress.Searching:
                    return ConsoleColor.DarkYellow;

                case AddonProgress.Downloading:
                    return ConsoleColor.Yellow;

                case AddonProgress.Extracting:
                    return ConsoleColor.DarkYellow;

                case AddonProgress.NotSupported:
                case AddonProgress.Error:
                    return ConsoleColor.Red;

                default:
                    return ConsoleColor.White;
            }
        }

        /// <summary>
        /// Writes to specified console line. For progress updating.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="status"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void ConsoleWrite(int line, string status, string text, ConsoleColor color)
        {
            lock (WriteLock)
            {
                var positiontop = Console.CursorTop;
                var positionleft = Console.CursorLeft;
                Console.SetCursorPosition(0, line);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, line);
                Console.Write("[");
                Console.ForegroundColor = color;
                Console.Write(status);
                Console.ResetColor();
                Console.Write("]");
                Console.Write($" - {text}");
                Console.SetCursorPosition(positionleft, positiontop);
            }
        }
    }
}