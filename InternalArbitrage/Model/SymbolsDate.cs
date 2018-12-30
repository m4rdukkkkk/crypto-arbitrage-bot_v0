/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System;
using System.Reflection;

namespace InternalArbitrage
{
    public class SymbolsDate
    {       
        private string[] currency_payment;
        private decimal minAmount;
        private bool itsBuy;

        public MagicNumber magicNumber;    // Use a reference. For the purpose of ML and the use of DB
        public OrderTrade buyOrderTrade;
        public OrderTrade sellOrderTrade;
        public OrderTrade orderTrade;
        public string symbole;
        public string currency;
        public string payment;
        public decimal lastPrice;
        public decimal valume;
        
        public bool itsAvalible;
        public int currencyRound;


        public string Symbole
        {
            get { return symbole; }
            set {
                symbole = value;
                currency_payment = symbole.Split('_');
                currency = currency_payment[0];
                payment = currency_payment[1];
            }
        }

        public decimal MinAmount
        {
            get { return minAmount; }
            set
            {                
                minAmount = (StaticVariables.api.MinAmount(value,symbole));

#if DEBUG
                decimal minAmount1 = minAmount;
#endif
                if (StaticVariables.CurrencyTradingFeeReduction)
                    minAmount = minAmount * 1.01m;

                if (StaticVariables.api.NeedRoundUp())
                    minAmount = Rounding.RoundUp(minAmount, currencyRound);
                else
                    minAmount = StaticVariables.api.ClampOrderQuantity(symbole, minAmount);


#if DEBUG
                if (StaticVariables.api.MinAmountByCalculation())
                {
                    decimal sum = minAmount * value;
                    string war = "";
                    if (payment == "bnb" & sum < 1)
                        war = "     WARRNING";

                    if (payment == "btc" & sum < 0.001m)
                        war = "     WARRNING";

                    if (payment == "eth" & sum < 0.01m)
                        war = "     WARRNING";

                    if (payment == StaticVariables.usdName & sum < 10)
                        war = "     WARRNING";

                    string tmp = String.Format("symbole - {3},  minAmount - {0},  price - {1},  sum - {2}, minAmount1 - {4},{5}", minAmount, value, sum, symbole, minAmount1, war);
                    PrintFunc.AddLine(StaticVariables.pathDataDebug + "minAmount.txt", tmp);
                }               
#endif
            }
        }

        // call by Trade_package.SetArbitrageSymbolsDate
        // call by FindingSymbolsTrading.ArbitragePercent
        public bool ItsBuy
        {
            get { return itsBuy; }
            set
            {
                try
                {
                    itsBuy = value;
                    if (itsBuy)
                    {
                        orderTrade = buyOrderTrade;
                        orderTrade.BestPrice = sellOrderTrade.originalMaxOrMinPrice;
                        orderTrade.ExtraPercent = magicNumber.Buy;
                        orderTrade.UpExtraPercent();    // only if(itsBuy) An attempt to raise the percentage of the magic number
                    }
                    else
                    {
                        orderTrade = sellOrderTrade;
                        orderTrade.BestPrice = buyOrderTrade.originalMaxOrMinPrice;
                        orderTrade.ExtraPercent = magicNumber.Sell;
                    }
                }
                catch (Exception ex)
                {
                    DateTime localDate = DateTime.Now;
                    string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                    printResult += String.Format("\nsymbole - {0}, itsBuy - {1}", symbole, itsBuy);
                    PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
                }              
            }
        }

        public SymbolsDate(string _symbole, ExchangeTicker ticker, MagicNumber _magicNumber)
        {
            Symbole = _symbole;
            lastPrice = ticker.Last;
            valume = WalletFunc.ConversionPrice((ticker.Volume.PriceAmount * lastPrice), payment);
            
            if (StaticVariables.roundingPrice)
            {
                currencyRound = StaticVariables.api.GetRoundAmount(symbole, lastPrice);
            }

            MinAmount = lastPrice;
            magicNumber = _magicNumber;

            
        }

        public override string ToString()
        {
            string valumeFormat = (StaticVariables.paymentWeighted.Contains("usd") ? String.Format("{0:n0}",valume) : String.Format("{0:n3}", valume));
            string resPrint = String.Format("{0}, {1}", symbole, valumeFormat);
            return resPrint;
        }
    }
}


