/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;

namespace InternalArbitrage
{
    // all class is for USE to ML_4
    public static class WaitingTimeML
    {
        public static void Start()
        {
            List<MagicNumber> buyWaitingTime = GetBuyWaitingTimeList();
            List<MagicNumber> sellWaitingTime = GetSellWaitingTimeList();
           
            int buyNewMagicWaiting = SetBuyWaitingTime(buyWaitingTime);
            int sellNewMagicWaiting = SetSellWaitingTime(sellWaitingTime);

            bool buyResult = (buyNewMagicWaiting ==  StaticVariables.startBuyMagicWaiting);
            bool sellResult = (sellNewMagicWaiting == StaticVariables.startSellMagicWaiting);
            if (buyResult & sellResult)
                return;

            List<MagicNumber> magicNumbersToUpdate = new List<MagicNumber>();
            foreach (var item in StaticVariables.magicNumberList)
            {
                if (item.Value.Buy.IsArbitrageSymbol == false)      // We will only update symbols that are not used for arbitrage
                {
                    if (item.Value.Buy.NumSuccess == 0)      // We will only update symbols that have not been valued by trading in the past
                    {
                        item.Value.Buy.WaitingTimeForNextPriceUpdate = buyNewMagicWaiting;
                        magicNumbersToUpdate.Add(item.Value);
                    }

                    if (item.Value.Sell.NumSuccess == 0 & item.Value.Sell.NumUnSuccess == 0)    // We will only update symbols that have not been valued by trading in the past
                    {
                        item.Value.Sell.WaitingTimeForNextPriceUpdate = sellNewMagicWaiting;
                        magicNumbersToUpdate.Add(item.Value);
                    }
                }                
            }

            if (magicNumbersToUpdate.Count > 0)
            {
                magicNumbersToUpdate = magicNumbersToUpdate.Distinct().ToList();
                SqlMagicNumber.UpdateAll(magicNumbersToUpdate);
            }

            StaticVariables.startBuyMagicWaiting = buyNewMagicWaiting;
            StaticVariables.startSellMagicWaiting = sellNewMagicWaiting;
        }
      
        public static List<MagicNumber> GetBuyWaitingTimeList()
        {
            List<MagicNumber> magicNumbers = new List<MagicNumber>();
            using (var db = new MagicNumberContext())
            {
                var query = from b in db.Table_Name
                            where b.Buy.NumSuccess > 0
                            where b.Buy.IsArbitrageSymbol == false
                            select b;

                foreach (var item in query)
                {
                    magicNumbers.Add(item);
                }
            }
            return magicNumbers;
        }

        public static List<MagicNumber> GetSellWaitingTimeList()
        {
            List<MagicNumber> magicNumbers = new List<MagicNumber>();
            using (var db = new MagicNumberContext())
            {
                var query = from b in db.Table_Name
                            where (b.Sell.NumSuccess > 0 || b.Sell.NumUnSuccess > 0)
                            where b.Sell.IsArbitrageSymbol == false
                            select b;

                foreach (var item in query)
                {
                    magicNumbers.Add(item);
                }
            }
            return magicNumbers;
        }

        public static int SetBuyWaitingTime(List<MagicNumber> magicNumberList)
        {
            if (magicNumberList.Count == 0)
                return StaticVariables.startBuyMagicWaiting;

            int numResult = 0;
            int sumResult = 0;
            int result = 0;
            foreach (var item in magicNumberList)
            {
                numResult += item.Buy.NumSuccess;
                sumResult += (item.Buy.WaitingTimeForNextPriceUpdate * item.Buy.NumSuccess);
            }

            result = sumResult / numResult;     // Returns Average waiting time recommended
            return result;
        }

        public static int SetSellWaitingTime(List<MagicNumber> magicNumberList)
        {
            if (magicNumberList.Count == 0)
                return StaticVariables.startSellMagicWaiting;

            int numResult = 0;
            int sumResult = 0;
            int result = 0;
            foreach (var item in magicNumberList)
            {
                numResult += item.Sell.NumSuccess;
                sumResult += (item.Sell.WaitingTimeForNextPriceUpdate * item.Sell.NumSuccess);
                numResult += item.Sell.NumUnSuccess;
                sumResult += (item.Sell.WaitingTimeForNextPriceUpdate * item.Sell.NumUnSuccess);
            }

            result = sumResult / numResult;     // Returns Average waiting time recommended           
            return result;
        }
    }
}