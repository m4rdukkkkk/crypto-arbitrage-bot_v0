/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Security;
using Main;

namespace InternalArbitrage
{
    public static partial class Approval
    {
        public static void Start(ExchangeAPI api)
        {
            string nameComputer = Environment.MachineName;
            string path = LoadSecurePath(NaneMainDir.GetFileDir() + "output/path" + nameComputer + ".bin").ToUnsecureString();            

            string fileName = api.Name + nameComputer + ".bin";                       
            string fileName1 = path + fileName;
            api.LoadAPIKeys(fileName1);

            fileName = "DB_" + nameComputer + ".bin";
            fileName1 = path + fileName;
            SecureString[] dbConnection = LoadSecureString(fileName1);

            MyConfiguration.ConfigurationApp(dbConnection);
        }

        public static SecureString[] LoadSecureString(string encryptedFile)
        {
            SecureString[] strings = CryptoUtility.LoadProtectedStringsFromFile(encryptedFile);
            return strings;
        }

        public static SecureString LoadSecurePath(string encryptedFile)
        {
            SecureString[] strings = CryptoUtility.LoadProtectedStringsFromFile(encryptedFile);
            return strings[0];
        }
    }
}