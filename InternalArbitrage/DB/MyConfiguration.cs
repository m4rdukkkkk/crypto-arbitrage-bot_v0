/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System.Configuration;
using System.Security;

namespace InternalArbitrage
{
    public static class MyConfiguration
    {
        public static void ConfigurationApp(SecureString[] dbConnection)
        {
            string host = dbConnection[0].ToUnsecureString();
            string port = "1433";
            string id = dbConnection[1].ToUnsecureString();
            string pwd = dbConnection[2].ToUnsecureString();
            string database = StaticVariables.nameDb;
            string providerName = "System.Data.SqlClient";
            string connectionString = string.Format("Data Source={0},{1}; Initial Catalog={2}; User ID={3}; Password={4};", host, port, database, id, pwd);

            var configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettings confCollection = new ConnectionStringSettings(StaticVariables.nameDb, connectionString, providerName);
            configManager.ConnectionStrings.ConnectionStrings.Add(confCollection);

            configManager.Save(ConfigurationSaveMode.Modified);
        }
    }
}

