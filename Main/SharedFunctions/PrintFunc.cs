/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Main
{
    public static partial class PrintFunc
    {
        ///Returns true if the file already exists
        public static bool CreateFile(string path,string firstLine)
        {
            if (File.Exists(path))
                return true;

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(firstLine);               
            }
            return false;
        }


        public static void AddLine(string path, string line)
        {
            using (StreamWriter sw = File.AppendText(path))
            {             
                    sw.WriteLine(line);
            }
        }


        public static void AddMultiLines(string path, List<string> lines)
        {            
            using (StreamWriter sw = File.AppendText(path))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    sw.WriteLine(lines[i]);
                }               
            }           
        }

        public static string PrintList<T>(List<T> list)
        {
            string resPrint = "";
            for (int i = 0; i < list.Count; i++)
            {
                resPrint += String.Format("{0}\n",list[i].ToString());
            }
            return resPrint;
        }

        public static string PrintDictionary<T>(Dictionary<string, T> list)
        {
            string resPrint = "";
            foreach (var item in list)
            {
                resPrint += String.Format("{0},\t{1}\n",item.Key.ToString(), item.Value.ToString());              
            }
            return resPrint;
        }

        public static void PrintList<T>(List<T> list, string nameVar, string path )
        {
            DateTime printTime = DateTime.Now;
            string resPrint = String.Format("Time,{0},", printTime); 
            AddLine(path + nameVar + ".txt", resPrint);
            for (int i = 0; i < list.Count; i++)
            {
                resPrint = String.Format("{0}, {1}", i + 1, list[i].ToString());
                AddLine(path + nameVar + ".txt", resPrint);
            }
        }

        // TO VIEW Generic
        public static void PrintDictionary<T>(Dictionary<string, T> list, string nameVar, string path)  
        {
            DateTime printTime = DateTime.Now;
            string resPrint = String.Format("Time,{0},", printTime);
            AddLine(path + nameVar + ".txt", resPrint);
            int i = 1;
            foreach (var item in list)
            {
                resPrint = String.Format("{0},{1},  {2}", i, item.Key.ToString(), item.Value.ToString());
                AddLine(path + nameVar + ".txt", resPrint);
                i++;
            }
        }

        public static void PrintDictionaryList(Dictionary<object, List<object>> list, string nameVar, string path)
        {
            DateTime printTime = DateTime.Now;
            string resPrint = String.Format("Time,{0},", printTime);
            AddLine(path + nameVar + ".txt", resPrint);
            int i = 1;
            foreach (var item in list)
            {
                resPrint = String.Format("{0},{1}, ", i, item.Key.ToString());
                for (int j = 0; j < item.Value.Count; j++)
                {
                    resPrint += String.Format("{0}, ", item.Value[j].ToString());
                }
                resPrint += String.Format("{0}, ", item.Value.Count);

                AddLine(path + nameVar + ".txt", resPrint);
                i++;
            }
        }

        public static void PrintDictionaryList<T>(Dictionary<string, List<T>> list, string nameVar, string path)
        {
            DateTime printTime = DateTime.Now;
            string resPrint = String.Format("Time,{0},", printTime);
            AddLine(path + nameVar + ".txt", resPrint);
            int i = 1;
            foreach (var item in list)
            {
                resPrint = String.Format("{0},{1}, ", i, item.Key.ToString());
                for (int j = 0; j < item.Value.Count; j++)
                {
                    resPrint += String.Format("{0}, ", item.Value[j].ToString());
                }
                resPrint += String.Format("{0}, ", item.Value.Count);

                AddLine(path + nameVar + ".txt", resPrint);
                i++;
            }
        }       
    }
}
