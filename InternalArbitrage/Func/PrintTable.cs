/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System;
using System.IO;

namespace InternalArbitrage
{
    public static partial class PrintTable
    {
        public static void PrintConsole(string resPrint)
        {
            Console.WriteLine(resPrint);
            PrintFunc.AddLine(StaticVariables.pathWithDate + "Console.txt", resPrint);
        }

        public static void Start(string PathFileName, string printResult, string TableType)
        {           
            StartTable(PathFileName, TableType);
            PrintFunc.AddLine(PathFileName, printResult);
        }

        public static void StartTable(string PathFileName, string TableType)
        {
            if (!(File.Exists(PathFileName)))
            {
                string choosingTable = ChoosingTable(TableType);           
                PrintFunc.AddLine(PathFileName, choosingTable);
            }
        }


        public static string ChoosingTable(string TableType)
        {
            string res = "";
            switch (TableType)
            {
                case "Trade_package":
                    res = "percent,percentPotential,currency,buySymbol,buyPrice,buyPricePotential,sellSymbol,sellPrice,sellPricePotential,buy_ExtraPercent,sell_ExtraPercent,endTimeOrder,";
                    break;
                case "OrderTrade":
                    res = OrderTrade();
                break;
                case "OrderResultSymbol":
                    res = "Symbol,OrderId,AmountFilled,Amount,Result,AveragePrice,Time,Result manually changed,";
                    break;
                case "OrderResult":
                    res = "OrderId,AmountFilled,Amount,Result,AveragePrice,Time,Result manually changed,";
                    break;
                case "OrderHandling":
                    res = "percent_end,";
                    for (int i = 0; i < 3; i++)
                    {
                        res += OrderTrade();
                    }
                    res += "revnu,";
                    break;
                case "WalletResult":
                    res = WalletResult();
                    break;
                case "WalletResultReal":
                    res = WalletResultReal();
                    break;
            }

            return res;
        }

        public static string OrderTrade()
        {
            return "itsBuy,Symbol,endPriceIntervalPercentage,startPriceIntervalPercentage,fullIntervalPercentage,endPrice,startPrice,maxOrMinPrice,originalMaxOrMinPrice,bestPrice,updatePercentage,extraPercent,timeAllOrder,timeThisOrder,startTimeThisOrder,endTimeOrder,numUpdate,," +
                "Amount,Price,,OrderId,AmountFilled,Amount,Result,AveragePrice,Time,Result manually changed,|   |,";
        }

        public static string WalletResult()
        {
            string res = "";
#if DEBUG
            res += String.Format("DifferentCalculation,percent_end,,");
#endif
            res += String.Format("buySymbol,usdRevnu,btcRevnu,percentTrade,usdTrade,btcTrade,,sumStart, sumAfter,,");

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-start,", item.Key);
            }
            res += "|    |,";

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-after,", item.Key);
            }
            res += "|    |,";

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-addOrderLeft,", item.Key);
            }
            res += "|    |,";

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-dif,", item.Key);
            }
            res += "|    |,";

#if DEBUG
            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-Conversion,", item.Key);
            }
#endif
            return res;
        }


        public static string WalletResultReal()
        {
            string res = "succsseTrade,,";
#if DEBUG
            res += String.Format("DifferentCalculation,percent_end,percentTrade,,");
#endif
            res += String.Format("buySymbol,usdRevnu,btcRevnu,percentTrade,usdTrade,btcTrade,,sumStart, sumAfter,,");

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-start,", item.Key);
            }
            res += "|    |,";

            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-after,", item.Key);
            }
            res += "|    |,";
           
            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-dif,", item.Key);
            }
            res += "|    |,";

#if DEBUG
            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                res += String.Format("{0}-Conversion,", item.Key);
            }
#endif
            return res;
        }
    }
}
