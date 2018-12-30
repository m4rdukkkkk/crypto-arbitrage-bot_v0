/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using System.Data;
using Main;

namespace InternalArbitrage
{
    public static partial class DataInitialization
    {
        public static void Start(bool fullSymbol = false)
        {
            List<string> SymbolsList = StaticVariables.api.GetSymbolsNormalize();
            SymbolsList.Sort();
#if DEBUG
            PrintFunc.PrintList(SymbolsList, "SymbolsList_beforeRemove", StaticVariables.pathDataDebug);
            List<string> SymbolsListRemove = new List<string>();
#endif
            List<string> currencyList = new List<string>();
            Dictionary<string, int> paymentList = new Dictionary<string, int>();
            string[] currency_payment;
            string currency;
            string payment;
            for (int g = 0; g < SymbolsList.Count; g++)
            {
                currency_payment = SymbolsList[g].Split('_');
                currency = currency_payment[0];
                payment = currency_payment[1];

                if (Remove(payment))
                {

#if DEBUG
                    SymbolsListRemove.Add(SymbolsList[g]);
#endif

                    SymbolsList.Remove(SymbolsList[g]);  // For the purpose of saving running time in the following loops
                    g--;                                 // Because we removed the value in the current index, then the next loop should use the current index that contains the following value
                }
                else
                {
                    currencyList.Add(currency);
                    if (paymentList.Keys.Contains(payment))
                        paymentList[payment] = paymentList[payment] + 1;
                    else
                        paymentList.Add(payment, 1);
                }
            }

#if DEBUG
            PrintFunc.PrintList(SymbolsList, "SymbolsList_afterRemove", StaticVariables.pathDataDebug);
            PrintFunc.PrintList(SymbolsListRemove, "SymbolsListRemove", StaticVariables.pathDataDebug);
#endif

            StaticVariables.PaymentListByWeight = paymentList.OrderByDescending(x => x.Value).Select(y => y.Key).ToList();
            currencyList = currencyList.Distinct().ToList();

            WalletFunc.InitializationStaticLists(SymbolsList);
            WalletFunc.ConversionPayment();
            Dictionary<string, ExchangeTicker> allTickers = StaticVariables.api.GetTickers();
            StaticVariables.maxTradeInPaymentWeighted = WalletFunc.GetMaxAmount(allTickers);
            //DataTable symboleDB = GetDB(GetExtraPercentFromDB);

            Dictionary<string, List<string>> listCurrenciesAndPayment = new Dictionary<string, List<string>>();
            StaticVariables.symbolsDateList = new Dictionary<string, List<SymbolsDate>>();
            

            // Use a reference. For the purpose of machine learning and the use of databases
            StaticVariables.magicNumberList = DBfunc.GetMagicNumberTable();

#if DEBUG
            PrintFunc.PrintList(SymbolsList, "SymbolsList_afterDistinct", StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(allTickers, "allTickers", StaticVariables.pathDataDebug);
#endif
            for (int i = 0; i < currencyList.Count; i++)
            {
                currency = currencyList[i];
                List<string> paymentCurrencyList = new List<string>();
                List<SymbolsDate> tempSymbolsDateList = new List<SymbolsDate>();
                SymbolsDate tempSymbolsDate;
                ExchangeTicker tempTicker;
                MagicNumber magicNumber;
                string symbole;
                for (int j = 0; j < SymbolsList.Count; j++)
                {
                    symbole = SymbolsList[j];
                    currency_payment = symbole.Split('_');
                    if (currency_payment[0].Equals(currency))
                    {
                        paymentCurrencyList.Add((fullSymbol ? SymbolsList[j] : currency_payment[1]));
                        
                        if (!allTickers.TryGetValue(symbole, out tempTicker))
                        {
                            SymbolsList.Remove(symbole);
                            j--;
                            continue;
                        }

                        magicNumber = DBfunc.GetMagicNumberItem(symbole, currency);
                        tempSymbolsDate = new SymbolsDate(symbole, tempTicker, magicNumber);
                        tempSymbolsDateList.Add(tempSymbolsDate);

                        if (StaticVariables.PaymentListByWeight.Contains(currency))
                            StaticVariables.listArbitrageSymbolsDate[symbole] = tempSymbolsDate;
  
                        SymbolsList.Remove(symbole);  // For the purpose of saving running time in the following loops
                        j--;                                 // Because we removed the value in the current index, then the next loop should use the current index that contains the following value
                    }
                }

                if (paymentCurrencyList.Count > 1)
                {
                    paymentCurrencyList.Sort();
                    listCurrenciesAndPayment.Add(currency, paymentCurrencyList);
                    StaticVariables.symbolsDateList.Add(currency, tempSymbolsDateList);
                }
            }

            DBfunc.AddMagicNumberTable(StaticVariables.magicNumberList);
            return;
        }

        public static bool Remove(string currency)
        {
            if (StaticVariables.ListPaymentDontTrade == null)
                return false;
         
            foreach (string coin in StaticVariables.ListPaymentDontTrade)
            {
                if (coin.Equals(currency))
                    return true;
            }

            return false;
        }
    }
}
