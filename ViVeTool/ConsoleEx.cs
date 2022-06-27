/*
    ViVe - Windows feature configuration library
    Copyright (C) 2019-2022  @thebookisclosed

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace Albacore.ViVeTool
{
    public static class ConsoleEx
    {
        public static void WriteErrorLine(string text, params object[] parameters)
        {
            var formatted = string.Format(text, parameters);
            WriteErrorLine(formatted);
        }

        public static void WriteErrorLine(string text)
        {
            var defaultFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultFg;
        }

        public static void WriteWarnLine(string text, params object[] parameters)
        {
            var formatted = string.Format(text, parameters);
            WriteWarnLine(formatted);
        }

        public static void WriteWarnLine(string text)
        {
            var defaultFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultFg;
        }

        public static bool UserQuestion(string question)
        {
            var defaultFg = Console.ForegroundColor;
            bool? returnValue = null;
            while (!returnValue.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(question + " [Y/N] ");
                Console.ForegroundColor = defaultFg;
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y)
                    returnValue = true;
                else if (key.Key == ConsoleKey.N)
                    returnValue = false;
                Console.WriteLine();
            }
            return returnValue.Value;
        }
    }
}
