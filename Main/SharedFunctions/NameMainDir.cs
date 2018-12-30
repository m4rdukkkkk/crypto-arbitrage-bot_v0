/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Main
{
    public static partial class NaneMainDir
    {
        
        public static string GetProgramsDir()
        {            
            string rootDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            rootDir += "/../../../";
            return rootDir;
        }

        public static string GetRootDir()
        {
            string rootDir = GetProgramsDir() + (string)Properties.Settings.Default["rootDir"];
            return rootDir;
        }
        
        public static string GetFileDir()
        {
            string rootDir = GetProgramsDir() + (string)Properties.Settings.Default["filesDir"];
            return rootDir;
        }

        public static string GetDateDir(bool textAfterDate = false)
        {
            DateTime localDate = DateTime.Now;
            string dateDir = String.Format("{0}.{1}.{2}/{3}_{4}_{5}", localDate.Day, localDate.Month, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second);
            dateDir += (textAfterDate ? "" : "/");
            return dateDir;
        }

        public static string GetMainDir(string project = null, string exchange = null, string symbol = null, bool dateDir = false, bool createDir = false, string textAfterDate = null)
        {
            string beginDir = GetRootDir();
            project = (project != null ? project + "/" : "");
            exchange = (exchange != null ? exchange + "/" : "");
            symbol = (symbol != null ? symbol + "/" : "");
            bool isTextAfterDate = (textAfterDate != null ? true : false);
            string datePath = (dateDir ? GetDateDir(isTextAfterDate) : "");
            if (textAfterDate != null)
            {
                char firstLetter = textAfterDate[0];
                Regex _regex = new Regex(@"[a-zA-Z0-9]$");
                if (_regex.IsMatch(firstLetter.ToString()))
                {
                    textAfterDate = "-" + textAfterDate;
                }
                textAfterDate += "/";
            }
            else
            {
                textAfterDate = "";
            }

            string rootDir = beginDir + project + exchange + symbol + datePath + textAfterDate;

            if (createDir)
            {
                Directory.CreateDirectory(rootDir);
            }

            return rootDir;
        }

        public static string GetDirAndFile(string dateDir)
        {
            dateDir = dateDir.Remove(dateDir.Length - 1);
            return dateDir;
        }

        public static string GetCurrentDir()
        {
            string result = GetProgramsDir().Substring(0, 3);
            return result;
            //Properties.Settings.Default["SomeProperty"] = "Some Value";
            //Properties.Settings.Default.Save(); // Saves settings in application configuration file
        }
    }
}
