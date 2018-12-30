/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Reflection;

namespace InternalArbitrage
{
    public static class SqlMagicNumber
    {
        public static MagicNumber GetByKey(string key)
        {
            MagicNumber magicNumber = new MagicNumber();
            using (var db = new MagicNumberContext())
            {
                magicNumber = db.Table_Name.FirstOrDefault(x => x.Symbol.Equals(key));
            }
            return magicNumber;
        }


        public static MagicNumber GetByInstance(MagicNumber instanceData)
        {
            using (var db = new MagicNumberContext())
            {
                var query = from b in db.Table_Name
                            orderby b.Symbol
                            select b;

                foreach (var item in query)
                {
                    if (item.Symbol == instanceData.Symbol)
                        return item;
                }
            }
            return null;
        }


        public static List<MagicNumber> GetAll()
        {
            List<MagicNumber> magicNumbers = new List<MagicNumber>();
            using (var db = new MagicNumberContext())
            {
                var query = from b in db.Table_Name
                            orderby b.Symbol
                            select b;

                foreach (var item in query)
                {
                    magicNumbers.Add(item);
                }
            }
            return magicNumbers;
        }


        public static void AddInstance(MagicNumber instanceData)
        {
            using (var db = new MagicNumberContext())
            {
                try
                {
                    db.Table_Name.Add(instanceData);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), instanceData.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
            }
        }


        public static void AddAllInstance(List<MagicNumber> ListInstanceData)
        {
            ListInstanceData = CheckInsertAll(ListInstanceData);
            if (ListInstanceData.Count < 1)
                return;

            using (var db = new MagicNumberContext())
            {
                try
                {
                    db.Table_Name.AddRange(ListInstanceData);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), ListInstanceData.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
            }
        }


        public static void RemoveByInstance(MagicNumber instanceData)
        {
            using (var db = new MagicNumberContext())
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.Table_Name.Attach(instanceData);
                    db.Entry(instanceData).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }
            }
        }


        public static void RemoveAllInstance(List<MagicNumber> ListInstanceData)
        {
            ListInstanceData = CheckInsertAll(ListInstanceData, true);
            if (ListInstanceData.Count < 1)
                return;

            using (var db = new MagicNumberContext())
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    foreach (var item in ListInstanceData)
                    {
                        db.Table_Name.Attach(item);
                        db.Entry(item).State = EntityState.Deleted;
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), ListInstanceData.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }
            }
        }


        public static void RemoveAll()
        {
            using (var db = new MagicNumberContext())
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;

                var query = from b in db.Table_Name
                            orderby b.Symbol
                            select b;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    foreach (var item in query)
                    {
                        db.Table_Name.Attach(item);
                        db.Entry(item).State = EntityState.Deleted;
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), query.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }
            }
        }


        public static void UpdateByInstance(MagicNumber instanceData)
        {
            using (var db = new MagicNumberContext())
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.Table_Name.Attach(instanceData);
                    db.Entry(instanceData).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), instanceData.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }
            }
        }

        public static void UpdateAll(List<MagicNumber> ListInstanceData)
        {
            ListInstanceData = CheckInsertAll(ListInstanceData, true);
            if (ListInstanceData.Count < 1)
                return;

            using (var db = new MagicNumberContext())
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;
                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    foreach (var item in ListInstanceData)
                    {
                        db.Table_Name.Attach(item);
                        db.Entry(item).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}\n\n{2}", localDate.ToString(), ex.ToString(), ListInstanceData.ToString());
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }
            }
        }


        public static List<MagicNumber> CheckInsertAll(List<MagicNumber> ListInstanceData, bool update = false)
        {
            List<MagicNumber> magicNumbers = new List<MagicNumber>();
            using (var db = new MagicNumberContext())
            {
                var query = from b in db.Table_Name
                            orderby b.Symbol
                            select b;

                foreach (var item in query)
                {
                    magicNumbers.Add(item);
                }
            }

            IEnumerable<MagicNumber> resultMagicNumbers = new List<MagicNumber>();
            resultMagicNumbers = from a in ListInstanceData
                                 from b in magicNumbers
                                 where a.Symbol == b.Symbol
                                 select a;

            if (update)
            {
                resultMagicNumbers.Distinct();
                return resultMagicNumbers.ToList();
            }
            else
            {
                foreach (var item in resultMagicNumbers.ToList())
                {
                    ListInstanceData.Remove(item);
                }
                ListInstanceData.Distinct();
                return ListInstanceData;
            }
        }
    }
}