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
    public static class WalletFunc
    {
        public static Dictionary<string, decimal> GetWallet()
        {
            Dictionary<string, decimal> wallet;
            try
            {
                wallet = StaticVariables.api.GetAmountsAvailableToTrade();
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                wallet = StaticVariables.api.GetAmountsAvailableToTrade();
            }

            GetWalletAvailableAmount(wallet);
            return wallet;
        }

        public static void GetWalletAvailableAmount(Dictionary<string, decimal> wallet)
        {
            decimal amaount;
            string payment;
            for (int i = 0; i < StaticVariables.PaymentListByWeight.Count; i++)
            {
                amaount = 0;
                payment = StaticVariables.PaymentListByWeight[i];
                wallet.TryGetValue(payment, out amaount);
                StaticVariables.WalletAvailableAmount[payment] = amaount;
            }

            GetWalletAvailable();
        }

        public static void GetWalletAvailable()
        {
          
            decimal amaount;
            string currency;
            string symbole;
            string payment = StaticVariables.paymentWeighted;

            for (int i = 0; i < StaticVariables.PaymentListByWeight.Count; i++)
            {
                currency = StaticVariables.PaymentListByWeight[i];
                payment = StaticVariables.paymentWeighted;

                if (currency.Equals(payment))
                    payment = StaticVariables.PaymentListByWeight[1];

                symbole = currency + "_" + payment;
                SymbolsDate arbitrageSymbolsDate;

                if (StaticVariables.listArbitrageSymbolsDate.TryGetValue(symbole, out arbitrageSymbolsDate))
                {
                    amaount = arbitrageSymbolsDate.MinAmount;
                    StaticVariables.WalletAvailable[currency] = (StaticVariables.WalletAvailableAmount[currency] > amaount );
                }
                else
                {
                    symbole = payment + "_" + currency ;
                   
                    if (StaticVariables.listArbitrageSymbolsDate.TryGetValue(symbole, out arbitrageSymbolsDate))
                    {
                        amaount = arbitrageSymbolsDate.MinAmount;
                        amaount = ConversionPrice(amaount, payment);
                        StaticVariables.WalletAvailable[currency] = (StaticVariables.WalletAvailableAmount[currency] > amaount);
                    }
                    else
                    {
                        PrintException.ExceptionDeliberately("Missing symbol for mainMinAmount");
                    }
                }
                
            }

#if DEBUG
            PrintFunc.PrintDictionary(StaticVariables.WalletAvailableAmount, nameof(StaticVariables.WalletAvailableAmount), StaticVariables.pathDataDebug);
            PrintFunc.PrintDictionary(StaticVariables.WalletAvailable, nameof(StaticVariables.WalletAvailable), StaticVariables.pathDataDebug);
#endif
        }

        public static decimal ConversionPrice(decimal price, string currency, string payment = null, string convertibleCurrency = null)
        {
            payment = (payment == null ? StaticVariables.paymentWeighted : payment);
            if (payment.Equals(currency))
                return price;

            string symbole = currency + "_" + payment;
            decimal result = price * StaticVariables.ConversionCurrencyPayment[symbole];
            convertibleCurrency = (convertibleCurrency == null ? StaticVariables.paymentWeighted : convertibleCurrency);

            if (payment.Equals(convertibleCurrency))
                return result;

            string Finalsymbole = payment + "_" + convertibleCurrency;
            decimal FinalConversionRatio;
            if (!StaticVariables.ConversionCurrencyPayment.TryGetValue(Finalsymbole, out FinalConversionRatio))
            {
                string warningMessage = String.Format("WARNING_Finalsymbole_{0}.txt", Finalsymbole);
                PrintTable.PrintConsole(warningMessage);
                PrintFunc.AddLine(StaticVariables.pathWithDate + warningMessage, warningMessage);
            }

            return result * FinalConversionRatio;
        }
      
        public static void ConversionPayment(int maxCount = 5)
        {
            string symbole;
            decimal price;
            ExchangeOrderBook book;
            string[] currency_payment;
            string currency;
            string payment;
            string reversSymbole;
            foreach (var item in StaticVariables.listArbitrageSymbolsDate)
            {
                symbole = item.Key;
                book = StaticVariables.api.GetOrderBook(symbole, maxCount);
                price = Averge(book.Bids[0].Price, book.Asks[0].Price);
                StaticVariables.ConversionCurrencyPayment[symbole] = price;

                currency_payment = symbole.Split('_');
                currency = currency_payment[0];
                payment = currency_payment[1];
                reversSymbole = payment + "_" + currency;

                StaticVariables.ConversionCurrencyPayment[reversSymbole] =  (1/ price);
            }           
        }

        public static decimal Averge(decimal a, decimal b)
        {
            decimal sum = a + b;
            return (sum/2);
        }

        public static void InitializationStaticLists(List<string> SymbolsList)
        {
            StaticVariables.paymentWeighted = StaticVariables.PaymentListByWeight[0];

            if (StaticVariables.PaymentListByWeight.Contains("usdt"))
                StaticVariables.usdName = "usdt";
            else
                StaticVariables.usdName = StaticVariables.paymentWeighted;

            string symbole;
            string[] currency_payment;
            string currency;
            string payment;
            string reversSymbole;
                        
            for (int j = 0; j < SymbolsList.Count; j++)
            {
                symbole = SymbolsList[j];
                currency_payment = symbole.Split('_');
                currency = currency_payment[0];

                if (StaticVariables.PaymentListByWeight.Contains(currency))
                    StaticVariables.listArbitrageSymbolsDate.Add(symbole, null);                          
            }
                          
            StaticVariables.ConversionCurrencyPayment = new Dictionary<string, decimal>();
            StaticVariables.WalletAvailableAmount = new Dictionary<string, decimal>();
            StaticVariables.WalletAvailable = new Dictionary<string, bool>();
            for (int i = 0; i < StaticVariables.PaymentListByWeight.Count; i++)
            {
                StaticVariables.WalletAvailableAmount.Add(StaticVariables.PaymentListByWeight[i], 0);
                StaticVariables.WalletAvailable.Add(StaticVariables.PaymentListByWeight[i], false);
            }
                      
            foreach (var item in StaticVariables.listArbitrageSymbolsDate)
            {
                try
                {
                    StaticVariables.ConversionCurrencyPayment.Add(item.Key, 0);
                }
                catch (Exception){ }

                currency_payment = item.Key.Split('_');
                currency = currency_payment[0];
                payment = currency_payment[1];
                reversSymbole = payment + "_" + currency;

                try
                {
                    StaticVariables.ConversionCurrencyPayment.Add(reversSymbole, 0);
                }
                catch (Exception) { }
            }
        }

        public static decimal GetMaxAmount(Dictionary<string, ExchangeTicker> allTickers)
        {

            if (StaticVariables.paymentWeighted.Contains(StaticVariables.usdName))
               return StaticVariables.maxTradeInUsdt;

            string payment = StaticVariables.usdName;
           
            string symbole = StaticVariables.paymentWeighted + "_" + payment;
            decimal price = allTickers[symbole].Last;

            decimal maxAmaount = StaticVariables.maxTradeInUsdt / price;

            return maxAmaount;
        }

        public static decimal GetMaxAmountTrade(decimal price, string payment)
        {
            if (payment.Contains(StaticVariables.usdName))
                return  StaticVariables.maxTradeInUsdt / price;

            decimal PriceInpaymentWeighted = ConversionPrice(price,payment);
            return StaticVariables.maxTradeInPaymentWeighted / PriceInpaymentWeighted;
        }

        public static string WalletResult(Dictionary<string, decimal> wallet)
        {
            string result = "";

            foreach (var item in wallet)
            {
                result += String.Format("{0},", item.Value);
            }
            result += ",";

            return result;
        }

        public static string WalletResultCompare(Dictionary<string, decimal> oldWallet, Dictionary<string, decimal> newWallet,
            out decimal sumStart, out decimal sumAfter, out decimal sumRevnu)
        {
#if DEBUG
            string conversionResult = "";
#endif
            string result = "";
            decimal currencyStart = 0;
            decimal currencyAfter = 0;
            decimal currencyDif = 0;            
            sumRevnu = 0;
            sumStart = 0;
            sumAfter = 0;

            foreach (var item in oldWallet)
            {
                currencyStart = ConversionPrice(item.Value, item.Key);
                currencyAfter = ConversionPrice(newWallet[item.Key], item.Key);
                currencyDif = currencyAfter - currencyStart;

                sumStart += currencyStart;
                sumAfter += currencyAfter;
                sumRevnu += currencyDif;

                result += String.Format("{0},", currencyDif);
#if DEBUG              
                conversionResult += String.Format("{0},", (currencyStart / item.Value));        
#endif
            }
            result += ",";

#if DEBUG
            conversionResult += ",";
            result += conversionResult;
#endif
            return result;
        }
    }
}
