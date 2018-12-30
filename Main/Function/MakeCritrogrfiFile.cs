/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;

namespace Main
{
    public static class MakeCritrogrfiFile
    {
        public static void GetFiles(string input, string output)
        {                  
            string[] filesAll = Directory.GetFiles(input);
            for (int i = 0; i < filesAll.Length; i++)
            {
                MakeFile(filesAll[i], output);
            }
        }

        public static void MakeFile(string name, string path)
        {
            string oldName = name;
            string[] temp = File.ReadAllLines(name);
            name = Path.GetFileNameWithoutExtension(name);
            name = name + Environment.MachineName + ".bin";
            name = path + name;
            CryptoUtility.SaveUnprotectedStringsToFile(name, temp);
            File.Delete(oldName);
        }
    }
}
//string[] temp = new string[3];
//temp[0] = "API key";
//temp[1] = "API secret";
//temp[2] = "Passphrase" ;