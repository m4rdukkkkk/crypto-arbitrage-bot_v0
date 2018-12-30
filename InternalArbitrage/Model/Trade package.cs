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
    public class Trade_package
    {            
        private string[] currency_paymentTemp;
        private decimal expense;
        private decimal incomeSell;
        private decimal income;

        public ExchangeAPI api;
        public SymbolsDate arbitrageSymbolsDate;
        public SymbolsDate buySymbolsDate;
        public SymbolsDate sellSymbolsDate;
        public bool itsBuyArbitrage;        
        public decimal minAmountFromExchange;
        public decimal minAmountFromWallet;
        public decimal minAmountTrade;
        public decimal maxAmountTrade;
        private decimal amountTrade;
        public decimal originalAmountTrade;
        public decimal percent;
        public decimal percentPotential;  // Depending on ML MagicNumber;
        public string currency;
        public string buySymbol;
        public string sellSymbol;
        public string arbitrageSymbol;
        public decimal buyPrice;
        public decimal sellPrice;
        public decimal buyPricePotential;
        public decimal sellPricePotential;
        public string currencyArbitrage;
        public string paymentArbitrage;
        public string currencyForWallet;
        
        public bool result;       
        public decimal beforeRound;
        public decimal currencyLeftFromRound;
        public decimal valueCurrencyLeftFromRound;
        public decimal realPercentage;
        public decimal diffRealPercentage;
        public decimal fee;
        public decimal costFee;
        public string walletResult; 
        public decimal percent_0;
        public decimal percent_1;
        public string amountsPrint;


#if DEBUG
        public string debug;
#endif

        public decimal AmountTrade
        {
            get { return amountTrade; }
            set
            {
                amountTrade = api.ClampOrderQuantity(buySymbol, value);
            }
        }

        public decimal Expense
        {
            get { return expense; }
            set
            {
                expense = value;
            }
        }

        public decimal IncomeSell
        {
            get { return incomeSell; }
            set
            {
                incomeSell = value;
            }
        }

        public decimal Income
        {
            get { return income; }
            set
            {
                income = value;
            }
        }


        // call by Start.FindAndTrade
        public bool StartTradePackageMagic()
        {
            StartTradePackage();

            buySymbolsDate.orderTrade.itsFirstOrder = true;
            revnuCalculation(percent);
            percent_0 = realPercentage;
            percent_1 = realPercentage;
            ChangeMaxOrMinPriceBuy((StaticVariables.revnuTrade/100));

#if DEBUG
            PrintFunc.AddLine(StaticVariables.pathDebug + currency + "/buy_" + currency + ".csv", debug);
#endif

            return result;
        }

        // TODO Add Func TradeFast 
        //// call by Start.FindAndTrade
        //public bool StartTradePackageFaster()
        //{
        //    StartTradePackage();
        //    revnuCalculation(percent);
        //    return result;
        //}

        // call by this.StartTradePackageMagic or this.StartTradePackageFaster
        public void StartTradePackage()
        {            
            FindArbitrageSymbol();
            Amounts();
            SetArbitrageSymbolsDate();
            walletResult = WalletFunc.WalletResult(StaticVariables.WalletAvailableAmount);    // -start
        }

        // call by this.StartTradePackage
        public void FindArbitrageSymbol()
        {
            currency_paymentTemp = buySymbol.Split('_');
            currencyArbitrage = currency_paymentTemp[1];    // The payment used to buy the currency
            currencyForWallet = currency_paymentTemp[1];    // Remains constant regardless of whether it is buying or selling

            currency_paymentTemp = sellSymbol.Split('_');
            paymentArbitrage = currency_paymentTemp[1];    // The payment we received for the currency sale

            arbitrageSymbol = currencyArbitrage + "_" + paymentArbitrage;

            if (!StaticVariables.listArbitrageSymbolsDate.TryGetValue(arbitrageSymbol, out arbitrageSymbolsDate))
            {
                arbitrageSymbol = paymentArbitrage + "_" + currencyArbitrage;              
                itsBuyArbitrage = false;
                if (!StaticVariables.listArbitrageSymbolsDate.TryGetValue(arbitrageSymbol, out arbitrageSymbolsDate))
                    throw new Exception("No arbitrage currency was found\n" + arbitrageSymbol);

                currency_paymentTemp = arbitrageSymbol.Split('_');
                currencyArbitrage = currency_paymentTemp[0];    
                paymentArbitrage = currency_paymentTemp[1];    
            }
        }

        // call by this.StartTradePackage
        public void Amounts()
        {
            minAmountFromExchange = Math.Min(buySymbolsDate.orderTrade.request.Amount, sellSymbolsDate.orderTrade.request.Amount);          
            minAmountFromWallet = StaticVariables.WalletAvailableAmount[currencyForWallet] / buySymbolsDate.orderTrade.maxOrMinPrice;
            originalAmountTrade = Math.Min(minAmountFromExchange, minAmountFromWallet);

            decimal originalAmountTrade1 = Math.Max(originalAmountTrade, minAmountTrade);
            decimal originalAmountTrade2 = Math.Min(originalAmountTrade1, maxAmountTrade);

            AmountTrade = originalAmountTrade2;             

#if DEBUG
            DateTime timeOrder = DateTime.Now;
            amountsPrint = String.Format("{0}\n", timeOrder);
            amountsPrint += String.Format("buy.Amount - {0},\tsell.Amount - {1}\n",buySymbolsDate.orderTrade.request.Amount, sellSymbolsDate.orderTrade.request.Amount);
            amountsPrint += String.Format("minAmountFromExchange = Min(buy.Amount, sell.Amount) - {0}\n", minAmountFromExchange);
            amountsPrint += String.Format("minAmountFromWallet - {0}\n", minAmountFromWallet);
            amountsPrint += String.Format("originalAmountTrade = Min(minAmountFromExchange, minAmountFromWallet) - {0}\n",originalAmountTrade);

            amountsPrint += String.Format("minAmountTrade - {0},\tmaxAmountTrade - {1},\n", minAmountTrade, maxAmountTrade);

            amountsPrint += String.Format("originalAmountTrade1 = Max(originalAmountTrade, minAmountTrade) - {0},\n", originalAmountTrade1);
            amountsPrint += String.Format("originalAmountTrade2 = Min(originalAmountTrade1, maxAmountTrade) - {0},\n", originalAmountTrade2);
            amountsPrint += String.Format("AmountTrade = originalAmountTrade2 - {0},\n\n", AmountTrade);

            Directory.CreateDirectory(StaticVariables.pathDebug + currency);
            PrintFunc.AddLine(StaticVariables.pathDebug + currency + "/Amounts_" + currency + ".txt", amountsPrint);
#endif

            buySymbolsDate.orderTrade.request.Amount = AmountTrade;
            sellSymbolsDate.orderTrade.request.Amount = AmountTrade;
        }

        // call by StartTradePackage()
        public void SetArbitrageSymbolsDate()
        {
            ExchangeOrderBook book = api.GetOrderBook(arbitrageSymbol, StaticVariables.maxCount);
            FindingSymbolsTrading.GetPrice(book, arbitrageSymbolsDate, true);
            FindingSymbolsTrading.GetPrice(book, arbitrageSymbolsDate, false);
            arbitrageSymbolsDate.ItsBuy = itsBuyArbitrage;
            arbitrageSymbolsDate.orderTrade.request.api = StaticVariables.api;
            arbitrageSymbolsDate.orderTrade.request.ShouldRoundAmount = true;

            if (itsBuyArbitrage)
            {                  
                arbitrageSymbolsDate.orderTrade.request.Amount = AmountTrade * buySymbolsDate.orderTrade.request.Price;               
            }
            else
            {
                arbitrageSymbolsDate.orderTrade.request.Amount = AmountTrade * sellSymbolsDate.orderTrade.request.Price;
            }

            if (arbitrageSymbolsDate.orderTrade.request.Amount < arbitrageSymbolsDate.MinAmount)
                arbitrageSymbolsDate.orderTrade.request.Amount = arbitrageSymbolsDate.MinAmount;
        }

        public decimal revnuCalculation(decimal percentCompare)
        {
            
            Expense = AmountTrade * buySymbolsDate.orderTrade.request.Price;
            IncomeSell = AmountTrade * sellSymbolsDate.orderTrade.request.Price;
            Income = arbitrageSymbolsDate.orderTrade.request.Amount * arbitrageSymbolsDate.orderTrade.request.Price; 
            
            if (itsBuyArbitrage)
            {
                beforeRound = AmountTrade * buySymbolsDate.orderTrade.request.Price;
                currencyLeftFromRound = beforeRound - arbitrageSymbolsDate.orderTrade.request.Amount;
                valueCurrencyLeftFromRound = currencyLeftFromRound * arbitrageSymbolsDate.orderTrade.request.Price;

                realPercentage = ((IncomeSell - Income - valueCurrencyLeftFromRound) / Income) * 100;
            }
            else
            {
                beforeRound = AmountTrade * sellSymbolsDate.orderTrade.request.Price;
                currencyLeftFromRound = beforeRound - arbitrageSymbolsDate.orderTrade.request.Amount;
                valueCurrencyLeftFromRound = currencyLeftFromRound * arbitrageSymbolsDate.orderTrade.request.Price;

                realPercentage = ((Income - Expense + valueCurrencyLeftFromRound) / Expense) * 100;
            }
            
            diffRealPercentage = ((realPercentage - percentCompare) / percentCompare) * 100;

#if DEBUG
            DateTime revnuCalculationTime = DateTime.Now;
            debug = String.Format("{13},buySymbol,\n{14},sellSymbol,\n{15},arbitrageSymbol,\n{12},revnuCalculationTime,\n{0},buy.Price,\n{1},sell.Price,\n{2},arbitrage.Price,\n{3},AmountTrade,\n{11},arbitrageAmount," +
                "\n{4},Expense,\n{5},IncomeSell,\n{6},Income," +
                "\n{7},beforeRound,\n{8},currencyLeftFromRound,\n{9},valueCurrencyLeftFromRound,\n{10},realPercentage,",
                buySymbolsDate.orderTrade.request.Price, sellSymbolsDate.orderTrade.request.Price, arbitrageSymbolsDate.orderTrade.request.Price,
                AmountTrade, Expense, IncomeSell, Income,
                beforeRound, currencyLeftFromRound, valueCurrencyLeftFromRound, realPercentage, arbitrageSymbolsDate.orderTrade.request.Amount,
                revnuCalculationTime, buySymbol, sellSymbol, arbitrageSymbol);
#endif
            if (realPercentage > StaticVariables.revnuTrade)
                result = true;
            else
                result = false;

            return realPercentage;
        }

        // call by Trade_package.StartTradePackage
        public void ChangeMaxOrMinPriceBuy(decimal PersonalFee)
        {
            costFee = (1 - PersonalFee);
            if (itsBuyArbitrage)
            {
                buySymbolsDate.orderTrade.MaxOrMinPrice = (((IncomeSell / AmountTrade) / arbitrageSymbolsDate.orderTrade.request.Price)) * costFee;
            }
            else
            {
                buySymbolsDate.orderTrade.MaxOrMinPrice = ((Income + valueCurrencyLeftFromRound) / AmountTrade) * costFee;
            }
#if DEBUG
            debug += String.Format("\n,buy\n{0},PersonalFee,\n{1},costFee,\n{2},buy.originalMaxOrMinPrice,\n{3},buy.MaxOrMinPrice,\n",
                PersonalFee, costFee, buySymbolsDate.orderTrade.originalMaxOrMinPrice, buySymbolsDate.orderTrade.maxOrMinPrice);
#endif
        }

        // call by TradeMagic.Start
        public void ChangeMaxOrMinPriceSell(decimal PersonalFee)
        {
            costFee = (1 + PersonalFee);
            if (itsBuyArbitrage)
            {
                sellSymbolsDate.orderTrade.MaxOrMinPrice = ((Income + valueCurrencyLeftFromRound) / AmountTrade) * costFee;
            }
            else
            {
                sellSymbolsDate.orderTrade.MaxOrMinPrice = ((Expense / arbitrageSymbolsDate.orderTrade.request.Price) / AmountTrade) * costFee;
            }
#if DEBUG
            debug += String.Format("\n,sell\n{0},PersonalFee,\n{1},costFee,\n{2},sell.originalMaxOrMinPrice,\n{3},sell.MaxOrMinPrice,\n",
               PersonalFee, costFee, sellSymbolsDate.orderTrade.originalMaxOrMinPrice, sellSymbolsDate.orderTrade.maxOrMinPrice);
#endif
        }

        // call by TradeMagic.Start
        public void ChangeMaxOrMinPriceArbitrage(decimal PersonalFee)
        {            
            if (itsBuyArbitrage)
            {
                costFee = (1 - PersonalFee);
                arbitrageSymbolsDate.orderTrade.MaxOrMinPrice = (IncomeSell / Expense) * costFee;
            }
            else
            {
                costFee = (1 + PersonalFee);
                arbitrageSymbolsDate.orderTrade.MaxOrMinPrice = (Expense / IncomeSell) * costFee;
            }
#if DEBUG
            debug += String.Format("\n,arbitrage\n{0},PersonalFee,\n{1},costFee,\n{2},arbitrage.originalMaxOrMinPrice,\n{3},arbitrage.MaxOrMinPrice,",
               PersonalFee, costFee, arbitrageSymbolsDate.orderTrade.originalMaxOrMinPrice, arbitrageSymbolsDate.orderTrade.maxOrMinPrice);

#endif
        }

       
        public Trade_package(decimal _precent, string _currency, SymbolsDate _buySymbolsDate, SymbolsDate _sellSymbolsDate)
        {
            api = StaticVariables.api;
            itsBuyArbitrage = true;

            percent = _precent;
            buySymbolsDate = _buySymbolsDate;
            sellSymbolsDate = _sellSymbolsDate;
            currency = _currency;

            buySymbol = buySymbolsDate.symbole;
            sellSymbol = sellSymbolsDate.symbole;

            minAmountTrade = Math.Max(buySymbolsDate.MinAmount, sellSymbolsDate.MinAmount);
            maxAmountTrade = WalletFunc.GetMaxAmountTrade(buySymbolsDate.orderTrade.request.Price, buySymbolsDate.payment);
            maxAmountTrade= StaticVariables.api.ClampOrderQuantity(buySymbol, maxAmountTrade);
            if (maxAmountTrade < minAmountTrade)
            {
                string warningMessage = String.Format("currency - {0}, buySymbol - {1}, buy.MinAmount - {2}, sellSymbols - {3}, sell.MinAmount- {4}, minAmountTrade - {5}, maxAmountTrade - {6}", currency,buySymbol, buySymbolsDate.MinAmount, sellSymbol, sellSymbolsDate.MinAmount, minAmountTrade, maxAmountTrade);
                PrintTable.PrintConsole(warningMessage);
                PrintFunc.AddLine(StaticVariables.pathWithDate + "WARNING_maxAmount.txt", warningMessage);

                maxAmountTrade = minAmountTrade;
            }
                
        }

        public override string ToString()
        {
            DateTime endTimeOrder = DateTime.Now;
            string resultToString = String.Format("{0}%,{1}%,{2},{3},{4},{5},{6},{7},{8},{9}%,{10}%,{11},", 
                                         percent,
                                         percentPotential,
                                         currency,
                                         "buy-"+ buySymbol,
                                         buyPrice,
                                          buyPricePotential,
                                         "sell-" + sellSymbol,
                                         sellPrice,                                       
                                         sellPricePotential,
                                         buySymbolsDate.orderTrade.ExtraPercent.Percent*100,
                                         sellSymbolsDate.orderTrade.ExtraPercent.Percent*100,
                                         endTimeOrder.ToString()
                                          );
            return resultToString;
        }

        public string ToConsole()
        {
            string resultToString = String.Format("{0},Percent - {1}%,percentPotential - {2}%",  
                                         currency,
                                         Math.Round(percent,3),
                                         Math.Round(percentPotential,3)
                                         );

            return resultToString;
        }

    }
}
