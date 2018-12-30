/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;

namespace InternalArbitrage
{
    public static class DBfunc
    {
        public static decimal startBuyExtraPercent = 0.3m;
        public static decimal startSellExtraPercent = 0m;       

        public static Dictionary<string, MagicNumber> GetMagicNumberTable()
        {
            Dictionary<string, MagicNumber> magicNumberList = new Dictionary<string, MagicNumber>();
            List<MagicNumber> magicNumbers = new List<MagicNumber>();
            magicNumbers = SqlMagicNumber.GetAll();
            foreach (var item in magicNumbers)
            {
                magicNumberList.Add(item.Symbol, item);
            }

            if (magicNumberList.Count > 0)
            {
                WaitingTimeML.Start();      // USE to ML_4
            }

            return magicNumberList;
        }

        public static void AddMagicNumberTable(Dictionary<string, MagicNumber> magicNumberList)
        {          
            List<MagicNumber> magicNumbers = new List<MagicNumber>();           
            foreach (var item in magicNumberList)
            {
                magicNumbers.Add(item.Value);
            }
            SqlMagicNumber.AddAllInstance(magicNumbers);
        }
       
        public static MagicNumber GetMagicNumberItem(string symbole, string currency)
        {
            MagicNumber magicNumber;
            if (!StaticVariables.magicNumberList.TryGetValue(symbole, out magicNumber))
            {
                if (StaticVariables.PaymentListByWeight.Contains(currency))
                {
                    magicNumber = new MagicNumber(symbole, 0.5m, 0.5m,5000,5000,true);  // In the case of the arbitrage currencies initialized with 50%
                }
                else
                {
                    magicNumber = new MagicNumber(symbole, startBuyExtraPercent, startSellExtraPercent, StaticVariables.startBuyMagicWaiting, StaticVariables.startSellMagicWaiting);
                }
                StaticVariables.magicNumberList.Add(symbole, magicNumber);
            }
            return magicNumber;
        }
    }
}