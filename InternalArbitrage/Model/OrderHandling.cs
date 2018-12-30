/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;

namespace InternalArbitrage
{
    public class OrderHandling : Trade_package
    {
        private bool done;
        private OrderTrade buy;
        private OrderTrade sell;
        private OrderTrade arbitrage;
        private OrderTrade orderToCare;     

        public decimal percent_2;       
        public decimal percent_3;        
        public decimal percent_end;
        public decimal basePercentage;
        public bool succsseTrade;
        public bool itsCanAdded;
        public bool itsCanRevnu;
        public bool itsCanUpdate;
        public decimal endPercentage;
        public decimal endPercentageInterval;
        public decimal intervalRevnu;
        public decimal intervalRevnuPercentageTake;
        public decimal intervalRevnuTake;
        public decimal intervalRevnuLeft;
        public decimal fullIntervalPercentage;
        public DateTime[] TimeSummary;
        public int numUpdate;
        public bool itsTradeMagic;
        public bool itsTradeFaster;
        public bool itsOrderHandlingLeft;
        public Dictionary<string, decimal> oldWalletAvailableAmount;
        public Dictionary<string, decimal> newWalletAvailableAmount;
        public decimal sumRevnu;
        public decimal usdRevnu;
        public decimal btcRevnu;
        public decimal percentTrade;
        public decimal usdTrade;
        public decimal btcTrade;
        public decimal usdRevnuCalculation;
        public string summaryTrade;
        public string summaryTradeReal;
        public string walletResultReal;
        public int numFee = 0;

        public void StartOrderHandling()
        {
            buy = buySymbolsDate.orderTrade;
            sell = sellSymbolsDate.orderTrade; 
            arbitrage = arbitrageSymbolsDate.orderTrade;

            buy.MinAmaunt = minAmountTrade;
            sell.MinAmaunt = minAmountTrade;
            percent_1 = realPercentage;
        }


        public bool Done
        {
            get
            {
                return done;
            }
            set
            {
                done = value;
            }
        }

       
        public OrderTrade Buy
        {
            get
            {
                return buy;
            }
            set
            {
                buy = value;               
            }
        }

        public OrderTrade Sell
        {
            get
            {
                return sell;
            }
            set
            {
                sell = value;
            }
        }

        public OrderTrade Arbitrage
        {
            get
            {
                return arbitrage;
            }
            set
            {
                arbitrage = value;
            }
        }

        public void WalletResultEnd()
        {
            succsseTrade = (buy.succsseTrade & sell.succsseTrade & arbitrage.succsseTrade);

            oldWalletAvailableAmount = new Dictionary<string, decimal>();
            foreach (var item in StaticVariables.WalletAvailableAmount)
            {
                oldWalletAvailableAmount.Add(item.Key,item.Value);
            }
            StaticVariables.Wallet = WalletFunc.GetWallet();
            newWalletAvailableAmount = new Dictionary<string, decimal>();
            newWalletAvailableAmount = StaticVariables.WalletAvailableAmount;
            walletResult += WalletFunc.WalletResult(newWalletAvailableAmount);   // -after

            summaryTradeReal = String.Format("{0},,", succsseTrade);
            walletResultReal = walletResult + WalletFunc.WalletResultCompare(oldWalletAvailableAmount, newWalletAvailableAmount, out decimal sumStart, out decimal sumAfter, out decimal sumRevnu);   // -dif

            btcRevnu = sumRevnu;
            usdRevnu = WalletFunc.ConversionPrice(btcRevnu, StaticVariables.paymentWeighted, StaticVariables.usdName, StaticVariables.usdName);        
            btcTrade = WalletFunc.ConversionPrice(Expense,buySymbolsDate.payment);
            usdTrade = Math.Round(WalletFunc.ConversionPrice(btcTrade, StaticVariables.paymentWeighted, StaticVariables.usdName, StaticVariables.usdName),3);     
            percentTrade = btcRevnu / btcTrade;


#if DEBUG
            bool DifferentCalculation = percentTrade == percent_end;
            summaryTradeReal += String.Format("{0},{1},,", DifferentCalculation, percent_end);
#endif

            summaryTradeReal += String.Format("{0},{1},{2},{3},{4},{5},,{6},{7},,{8}", buySymbol, usdRevnu, btcRevnu, percentTrade, usdTrade, btcTrade, sumStart, sumAfter, walletResultReal);

            if (!buy.succsseTrade)
            {
                newWalletAvailableAmount[buySymbolsDate.payment] -= (buy.Result.Amount - buy.Result.AmountFilled) * buy.request.Price;
                numFee++;
            }


            if (!sell.succsseTrade)
            {
                newWalletAvailableAmount[sellSymbolsDate.payment] += (sell.Result.Amount - sell.Result.AmountFilled) * sell.request.Price;
                numFee++;
            }

            if (!arbitrage.succsseTrade)
            {
                if (itsBuyArbitrage)
                    newWalletAvailableAmount[arbitrageSymbolsDate.currency] += (arbitrage.Result.Amount - arbitrage.Result.AmountFilled) * arbitrage.request.Price;
                else
                    newWalletAvailableAmount[arbitrageSymbolsDate.payment] += (arbitrage.Result.Amount - arbitrage.Result.AmountFilled) * arbitrage.request.Price;

                numFee++;
            }

            walletResult += WalletFunc.WalletResult(newWalletAvailableAmount);   // -addOrderLeft

            walletResult += WalletFunc.WalletResultCompare(oldWalletAvailableAmount, newWalletAvailableAmount, out sumStart, out sumAfter, out sumRevnu);   // -dif

            btcRevnu = sumRevnu;
            usdRevnu = WalletFunc.ConversionPrice(btcRevnu, StaticVariables.paymentWeighted, StaticVariables.usdName,StaticVariables.usdName);       
            btcTrade = WalletFunc.ConversionPrice(Expense, buySymbolsDate.payment);
            usdTrade = Math.Round(WalletFunc.ConversionPrice(btcTrade, StaticVariables.paymentWeighted, StaticVariables.usdName, StaticVariables.usdName),3);     
            percentTrade = (btcRevnu / btcTrade) + (StaticVariables.FeeTrade * numFee);
            usdRevnuCalculation = Math.Round(usdTrade * ((percent_end/100) - (StaticVariables.FeeTrade*3)),3);


#if DEBUG
            DifferentCalculation = percentTrade == percent_end;
            summaryTrade = String.Format("{0},{1},{2},,", DifferentCalculation, percent_end, percentTrade);
#endif

            summaryTrade += String.Format("{0},{1},{2},{3},{4},{5},,{6},{7},,{8}", buySymbol, usdRevnu, btcRevnu, percentTrade, usdTrade, btcTrade, sumStart, sumAfter, walletResult);
        }

        public OrderTrade OrderToCare
        {
            get { return orderToCare; }
            set
            {
                orderToCare = value;
                succsseTrade = (buy.succsseTrade & sell.succsseTrade & arbitrage.succsseTrade);
                Done = (buy.Done & sell.Done & arbitrage.Done);
                itsCanAdded = (buy.itsCanAdded || sell.itsCanAdded || arbitrage.itsCanAdded);
                itsCanRevnu = (buy.itsCanRevnu || sell.itsCanRevnu || arbitrage.itsCanRevnu);
                itsCanUpdate = (buy.itsCanUpdate || sell.itsCanUpdate || arbitrage.itsCanUpdate);
                numUpdate = buy.numUpdate + sell.numUpdate + +arbitrage.numUpdate;               
            }
        }

        public bool ItsTradeFaster
        {
            get
            { return itsTradeFaster; }
            set
            {
                itsTradeFaster = value;
                itsTradeMagic = !itsTradeFaster;
            }
        }
    
        public OrderHandling(decimal _precent, string _currency, SymbolsDate _buySymbolsDate, SymbolsDate _sellSymbolsDate) 
            : base(_precent, _currency, _buySymbolsDate, _sellSymbolsDate)
        {
            done = false;
            succsseTrade = false;
            itsCanAdded = true;
            itsCanUpdate = true;
            intervalRevnuPercentageTake = 0.1m;
            TimeSummary = new DateTime[10];
            TimeSummary[4] = DateTime.Now;
            itsOrderHandlingLeft = false;
            itsTradeMagic = true;
            ItsTradeFaster = false;
        }

        public string PrintResult()
        {
            string res = String.Format("\nTime: {4},\toriginal Percentage: {8:P3},\tEnd Percentage: {2:P3} " +                                     
                                       "\nusdTrade - {18},\tusdRevnuCalculation - {21}  usdRevnu - {19:N3}," +
                                       "\nAmount Trade: {0:N3},\tbtcTrade - {17},\tbtcRevnu - {20}," +
                                       "\nprice To Buy = {1},\tprice To Sell = {5},\trevnue: {3:P3}," +
                                       "\npercent - {8:P3}   percentPotential - {9:P3}   percent_real - {10:P3}   percent_beforeBuy - {11:P3}   " +
                                       "\npercent_beforeSell - {12:P3}   percent_beforeArbitrage - {13:P3}  percentTrade - {22:P3}    percent_End - {14:P3}" +
                                       "\nbuy-{15} & sell-{16},\ttrade case-{6},\tSuccess- {7},",
                                       AmountTrade,
                                       WalletFunc.ConversionPrice(Buy.request.Price,buySymbolsDate.payment),
                                       percent_end / 100,
                                       StaticVariables.revnuTrade / 100,
                                       buy.startTimeOrder,
                                       WalletFunc.ConversionPrice(Sell.request.Price, sellSymbolsDate.payment), 
                                       (itsTradeFaster ? "Faster" : "Magic"),
                                       (succsseTrade ? "YES" : "NO"),
                                       percent / 100,
                                       percentPotential / 100,
                                       percent_0/ 100,
                                       percent_1/ 100,
                                       percent_2/ 100,
                                       percent_3 / 100,
                                       percent_end / 100,
                                       buySymbol,
                                       sellSymbol,
                                       btcTrade,
                                       usdTrade,
                                       usdRevnu,
                                       btcRevnu,
                                       usdRevnuCalculation,
                                       percentTrade
                                       );

            if (!succsseTrade)
            {
                res += String.Format("\nbuy - {0:P2},    sell - {1:P2},   arbitrage - {2:P2},",
                    (buy.amountFilled / AmountTrade) ,
                    (sell.amountFilled / AmountTrade) ,
                    (arbitrage.amountFilled / arbitrage.Result.Amount) );
            }
            
            return res;
        }

        public string PrintResult_2()
        {
            string resultToString = String.Format("{0},{1},{2},{3},",
                                         Math.Round(percent_end, 3), 
                                         buy.ToString(),
                                         sell.ToString(),
                                         arbitrage.ToString(),
                                         StaticVariables.revnu
                                          );
            return resultToString;
        }
    }
}



