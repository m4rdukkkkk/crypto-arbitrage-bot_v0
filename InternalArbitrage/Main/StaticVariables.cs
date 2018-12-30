/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Main;
using System.Reflection;

namespace InternalArbitrage
{
    public static partial class StaticVariables
    {        
        public static List<string> PaymentListByWeight;
        public static Dictionary<string, decimal> Wallet;
        public static Dictionary<string, decimal> WalletAvailableAmount;
        public static Dictionary<string, bool> WalletAvailable;        
        public static Dictionary<string, decimal> ConversionCurrencyPayment;       
        public static Dictionary<string, SymbolsDate> listArbitrageSymbolsDate = new Dictionary<string, SymbolsDate>();
        public static Dictionary<string, MagicNumber> magicNumberList;
        public static Dictionary<string, List<SymbolsDate>> symbolsDateList;
        public static string paymentWeighted;
        public static string usdName;

        public static ExchangeAPI api = Program.api;    // Assume a value in the api variable before placing values in the following variables
        public static List<string> ListPaymentDontTrade = api.ListPaymentDontTrade();
        public static string nameDb = api.Name + "_AWS";
        public static bool roundingPrice = api.RoundingPrice();
        public static bool CurrencyTradingFeeReduction = api.CurrencyTradingFeeReduction();
        public static bool rateGateLimit = (api.RateLimit.numberSessionsMinute < 600 ? true : false);
        public static decimal eachAddPercentage = 0.1m;
        public static OrderType orderType = OrderType.Limit;
        public static decimal FeeTrade = (1 -api.FeeTrade(orderType == OrderType.Market ? false : true));
        public static decimal maxTradeInUsdt = 100;
        public static decimal maxTradeInPaymentWeighted;
        public static decimal revnu = 0.2m;
        public static decimal revnuTrade = revnu + (FeeTrade * 3 * 100);
        public static int maxCount = 5;
        public static int startBuyMagicWaiting = 10000;
        public static int startSellMagicWaiting = 8000;

        public static string pathInternalArbitrage = NaneMainDir.GetMainDir("InternalArbitrage", api.Name);
        public static string pathFindFile = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, true, revnu.ToString() + "/FindFile");
        public static string pathSummaryFind = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, true, revnu.ToString() + "/SummaryFind");
#if DEBUG
        public static string pathDebug = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, true, revnu.ToString() + "/Debug");
        public static string pathDataDebug = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, true, revnu.ToString() + "/DataDebug");
        public static string pathFindDebug = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, true, revnu.ToString() + "/DataDebug/Find");
#endif
        public static string pathWithDate = NaneMainDir.GetMainDir("InternalArbitrage", api.Name, null, true, false, revnu.ToString());
        
        
        public static void StartProgram()
        {
            Console.WriteLine("Start");
            Approval.Start(api);
            DataInitialization.Start();
                     
            try
            {
                Start.FindAndTrade(true);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                Start.FindAndTrade( true);
            }            
        }             
    }
}

