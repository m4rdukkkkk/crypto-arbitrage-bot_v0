/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InternalArbitrage
{
    public static partial class Start
    {
        public static void FindAndTrade(bool trade)
        {                      
            StaticVariables.Wallet = WalletFunc.GetWallet();
#if DEBUG
            PrintDataDebug();
#endif

            DateTime currentTime;
            string timeHouer;
            string pathSummaryFind;
            int numFind = 0;
           
            while (true)
            {
                numFind++;
                currentTime = DateTime.Now;
                List<OrderHandling> packageList =new List<OrderHandling>();

                int i = 0;
                bool tradeSuccses;
                List<MagicNumber> magicNumbersToUpdate = new List<MagicNumber>();
                foreach (var item in StaticVariables.symbolsDateList)
                {
                    if (i % 5 == 0)
                        WalletFunc.ConversionPayment();
                    i++;

                    tradeSuccses = false;
                    do
                    {
                        OrderHandling package;
                        try
                        {
                            package = FindingSymbolsTrading.ArbitragePercent(item.Key, item.Value);
                        }
                        catch (Exception ex)
                        {
                            package = null;
                            DateTime localDate = DateTime.Now;
                            string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                            printResult += String.Format("\ncurrency - {0}", item.Key);
                            PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                        }

                        if (package != null)
                        {
                            packageList.Add(package);
                            PrintTable.Start(StaticVariables.pathFindFile + item.Key + ".csv", package.ToString(), "Trade_package");

                            // TODO Add Func TradeFast

                            List<MagicNumber> magicNumbersTradeMagicToUpdate = new List<MagicNumber>();
                            if (package.percentPotential > StaticVariables.revnuTrade)
                            {
                                if (StaticVariables.rateGateLimit)
                                    StaticVariables.api.RateLimit.OneOpportunity = true;

                                try
                                {
                                    if (package.StartTradePackageMagic())
                                        tradeSuccses = TradeMagic.Start(package);
                                }
                                catch (Exception ex)
                                {
                                    StaticVariables.Wallet = WalletFunc.GetWallet();  // Wallet update. Because part of the trade was carried out. Apparently the amounts of coins have changed

                                    DateTime localDate = DateTime.Now;
                                    string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());                                   
                                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                                    try
                                    {
                                        PrintTable.Start(StaticVariables.pathWithDate + "Exception_TradeMagic_" + item.Key + ".csv", package.Buy.ToString(), "OrderTrade");
                                        PrintTable.Start(StaticVariables.pathWithDate + "Exception_TradeMagic_" + item.Key + ".csv", package.Sell.ToString(), "OrderTrade");
                                        PrintTable.Start(StaticVariables.pathWithDate + "Exception_TradeMagic_" + item.Key + ".csv", package.Arbitrage.ToString(), "OrderTrade");
                                    }
                                    catch (Exception)
                                    {
                                        PrintTable.Start(StaticVariables.pathWithDate + "Exception_TradeMagic_Exception_" + item.Key + ".csv", package.ToString(), "Trade_package");
                                    }                                   
                                }

                                if (StaticVariables.rateGateLimit)
                                    StaticVariables.api.RateLimit.OneOpportunity = false;

                                magicNumbersTradeMagicToUpdate.Add(package.buySymbolsDate.magicNumber);
                                magicNumbersTradeMagicToUpdate.Add(package.sellSymbolsDate.magicNumber);
                                magicNumbersTradeMagicToUpdate.Add(package.arbitrageSymbolsDate.magicNumber);
                                SqlMagicNumber.UpdateAll(magicNumbersTradeMagicToUpdate);
                            }
                            else
                            {
                                magicNumbersToUpdate.Add(package.buySymbolsDate.magicNumber);
                            }
                        }

                    } while (tradeSuccses);
           }

                SqlMagicNumber.UpdateAll(magicNumbersToUpdate);
                WaitingTimeML.Start();      // USE to ML_4
                timeHouer = String.Format("{0}-{1}-{2}", currentTime.Hour, currentTime.Minute, currentTime.Second);
                PrintTable.PrintConsole(timeHouer + "\t" + numFind);
                pathSummaryFind = StaticVariables.pathSummaryFind + "SummaryFind_" + timeHouer + ".csv";
                foreach (var item in packageList)
                {
                    PrintTable.Start(pathSummaryFind, item.ToString(), "Trade_package");
                    if (item.percent > StaticVariables.revnuTrade || item.percentPotential > StaticVariables.revnuTrade)
                    {
                        PrintTable.PrintConsole(item.ToConsole());
                        PrintTable.Start(StaticVariables.pathWithDate + "SummaryFind" + ".csv", item.ToString(), "Trade_package");
                    }
                }

#if DEBUG
                PrintFunc.PrintDictionary(StaticVariables.magicNumberList, nameof(StaticVariables.magicNumberList), StaticVariables.pathDataDebug);
#endif
            }
        }


        public static void PrintDataDebug()
        {           
            PrintFunc.PrintList(StaticVariables.ListPaymentDontTrade, nameof(StaticVariables.ListPaymentDontTrade), StaticVariables.pathDataDebug);
            PrintFunc.PrintList(StaticVariables.PaymentListByWeight, nameof(StaticVariables.PaymentListByWeight), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.Wallet, nameof(StaticVariables.Wallet), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.WalletAvailableAmount, nameof(StaticVariables.WalletAvailableAmount), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.WalletAvailable, nameof(StaticVariables.WalletAvailable), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.ConversionCurrencyPayment, nameof(StaticVariables.ConversionCurrencyPayment), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.listArbitrageSymbolsDate, nameof(StaticVariables.listArbitrageSymbolsDate), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.magicNumberList, nameof(StaticVariables.magicNumberList), StaticVariables.pathDataDebug);

            PrintFunc.PrintDictionaryList(StaticVariables.symbolsDateList, nameof(StaticVariables.symbolsDateList), StaticVariables.pathDataDebug);

            string main1Var = String.Format("paymentWeighted - {0}\n", StaticVariables.paymentWeighted);
            main1Var += String.Format("usdName - {0}\n", StaticVariables.usdName);
            main1Var += String.Format("roundingPrice - {0}\n", StaticVariables.roundingPrice);
            main1Var += String.Format("eachAddPercentage - {0}\n", StaticVariables.eachAddPercentage);
            main1Var += String.Format("orderType - {0}\n", StaticVariables.orderType);
            main1Var += String.Format("FeeTrade - {0:P4}\n", StaticVariables.FeeTrade);
            main1Var += String.Format("maxTradeInUsdt - {0}\n", StaticVariables.maxTradeInUsdt);
            main1Var += String.Format("maxTradeInPaymentWeighted - {0}\n", StaticVariables.maxTradeInPaymentWeighted);
            main1Var += String.Format("revnuTrade - {0:P4}\n", StaticVariables.revnuTrade / 100);
            main1Var += String.Format("maxCount - {0}\n", StaticVariables.maxCount);
            main1Var += String.Format("CurrencyTradingFeeReduction - {0}\n", StaticVariables.CurrencyTradingFeeReduction);
            main1Var += String.Format("rateGateLimit - {0}\n", StaticVariables.rateGateLimit);

            PrintFunc.AddLine(StaticVariables.pathDataDebug + "Main1_var.txt", main1Var);
        }
    }
}