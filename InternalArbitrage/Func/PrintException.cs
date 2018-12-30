/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;

namespace InternalArbitrage
{
    public static partial class PrintException
    {        
        public static void Start(string FileName,string printResult)
        {
            string path = StaticVariables.pathWithDate;
            string PathFileName = String.Format("{0}Exception_{1}.txt", path, FileName);
            using (StreamWriter sw = File.AppendText(PathFileName))
            {
                sw.WriteLine(printResult);
                sw.WriteLine();
            }
        }

        public static void Start_2(string FileName, Exception ex, string printResult = null)
        {
            string path = StaticVariables.pathWithDate;
            string PathFileName = String.Format("{0}Exception{1}.txt", path, FileName);
            using (StreamWriter sw = File.AppendText(PathFileName))
            {
                sw.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                sw.WriteLine(printResult);
            }
        }

        public static void ExceptionDeliberately(string printResult)
        {          
            decimal result = 0;
            try
            {
                // The goal is to intentionally throw an exception
                decimal temp = 1 / result;
            }
            catch (Exception ex)
            {
                PrintTable.PrintConsole(printResult);
                Start("ExceptionDeliberately", printResult);
                throw ex;
            }
        }
    }
}
