/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System;

namespace InternalArbitrage
{
    public static partial class TradeMagic
    {
        public static bool Start(OrderHandling orderHandling)
        {
            bool result = false;
            bool orderIsLeft = false;
            decimal cutAmountFee = 1;

            orderHandling.StartOrderHandling();

            /// buy          
            orderHandling.OrderToCare = orderHandling.Buy;
            if (!HandlingOneOrderTrade(orderHandling,true))
                return false;

            if (orderHandling.OrderToCare.amountFilled > orderHandling.AmountTrade)
            {
                string warningMessage = String.Format("orderHandling.OrderToCare.amountFilled - {0}, orderHandling.AmountTrade - {1},", orderHandling.OrderToCare.amountFilled, orderHandling.AmountTrade);
                PrintTable.PrintConsole(warningMessage);
                PrintFunc.AddLine(StaticVariables.pathWithDate + "WARNING_amountFilled Greater than AmountTrade.txt", warningMessage);
                PrintTable.Start(StaticVariables.pathWithDate + "WARNING_amountFilledOrderHandlingData.csv", orderHandling.PrintResult_2(), "OrderHandling");
            }
            else if (orderHandling.OrderToCare.amountFilled <= orderHandling.AmountTrade)
            {
                if (StaticVariables.CurrencyTradingFeeReduction)
                {
                    cutAmountFee = StaticVariables.api.FeeTrade(orderHandling.OrderToCare.originalMaxOrMinPrice == orderHandling.OrderToCare.request.Price);
                    orderHandling.AmountTrade = orderHandling.OrderToCare.amountFilled * cutAmountFee;
                }
                else
                {
                    orderHandling.AmountTrade = orderHandling.OrderToCare.amountFilled;
                }
                orderHandling.Sell.Request.Amount = orderHandling.AmountTrade;

                if (orderHandling.itsBuyArbitrage)
                    orderHandling.Arbitrage.Request.Amount = orderHandling.AmountTrade * orderHandling.Buy.request.Price;
                else
                    orderHandling.Arbitrage.Request.Amount = orderHandling.AmountTrade * orderHandling.Sell.request.Price;

                if (orderHandling.Arbitrage.request.Amount < orderHandling.arbitrageSymbolsDate.MinAmount)
                    orderHandling.Arbitrage.request.Amount = orderHandling.arbitrageSymbolsDate.MinAmount;
            }


            if (orderHandling.AmountTrade < orderHandling.minAmountTrade)
                return false;

            // sell                        
            orderHandling.percent_2 = orderHandling.revnuCalculation(orderHandling.percent_1);
            orderHandling.ChangeMaxOrMinPriceSell((StaticVariables.revnuTrade / 100));
            orderHandling.OrderToCare = orderHandling.Sell;            
            orderHandling.OrderToCare.UpExtraPercent();          
            if (!HandlingOneOrderTrade(orderHandling, false))
            {
                orderIsLeft = true;
                orderHandling.buySymbolsDate.magicNumber.Buy.WaitingTimeForNextPriceUpdate -= 300;                 // USE to ML_4 
                orderHandling.sellSymbolsDate.magicNumber.Sell.WaitingTimeForNextPriceUpdate -= 300;               // USE to ML_4
            }


            result = true;
#if DEBUG
            orderHandling.debug += String.Format("\n\n{0},cutAmountFee,\n{1},Buy.extraPercent.Percent,\n{2},AmountTrade,\n{3},Sell.request.Amount,", cutAmountFee, orderHandling.Buy.extraPercent.Percent, orderHandling.AmountTrade, orderHandling.OrderToCare.request.Amount);
            PrintFunc.AddLine(StaticVariables.pathDebug + orderHandling.currency + "/sell_"+ orderHandling.currency + ".csv", orderHandling.debug);
#endif

            if (StaticVariables.CurrencyTradingFeeReduction)
            {
                cutAmountFee = StaticVariables.api.FeeTrade(orderHandling.OrderToCare.originalMaxOrMinPrice == orderHandling.OrderToCare.request.Price);
                orderHandling.AmountTrade = orderHandling.AmountTrade * cutAmountFee;
            }

            if (orderHandling.itsBuyArbitrage)
                orderHandling.Arbitrage.Request.Amount = orderHandling.AmountTrade * orderHandling.Buy.request.Price;
            else
                orderHandling.Arbitrage.Request.Amount = orderHandling.AmountTrade * orderHandling.Sell.request.Price;

            if (orderHandling.Arbitrage.request.Amount < orderHandling.arbitrageSymbolsDate.MinAmount)
                orderHandling.Arbitrage.request.Amount = orderHandling.arbitrageSymbolsDate.MinAmount;

            // arbitrage
            orderHandling.percent_3 = orderHandling.revnuCalculation(orderHandling.percent_2);
            orderHandling.ChangeMaxOrMinPriceArbitrage((StaticVariables.revnuTrade / 100));            
            orderHandling.OrderToCare = orderHandling.Arbitrage;
            orderHandling.OrderToCare.UpExtraPercent();
            try
            {
                if (!HandlingOneOrderTrade(orderHandling, false))
                {
                    orderIsLeft = true;

                    if (orderHandling.itsBuyArbitrage)
                        orderHandling.arbitrageSymbolsDate.magicNumber.Buy.WaitingTimeForNextPriceUpdate -= 300;       // USE to ML_4
                    else
                        orderHandling.arbitrageSymbolsDate.magicNumber.Sell.WaitingTimeForNextPriceUpdate -= 300;      // USE to ML_4
                }
                CancellationFunc.Cancellation99(orderHandling);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                string fileName = String.Format("ExceptionArbitrage_{0}_{1}", (orderHandling.OrderToCare.request.IsBuy ? "Buy" : "Sell"), orderHandling.OrderToCare.request.Symbol);
                PrintException.Start(fileName, printResult);
                // TODU Send to a function to check if step b (sell) has been performed. And resubmit arbitrage order. The function should use a database to restore if the boot is shut down and restarted
            }

#if DEBUG
            orderHandling.debug += String.Format("\n\n{0},cutAmountFee,\n{1},Sell.extraPercent.Percent,\n{2},AmountTrade,\n{3},Sell.request.Amount,", cutAmountFee, orderHandling.Sell.extraPercent.Percent, orderHandling.AmountTrade, orderHandling.OrderToCare.request.Amount);
            PrintFunc.AddLine(StaticVariables.pathDebug + orderHandling.currency + "/Arbitrage_" + orderHandling.currency + ".csv", orderHandling.debug);
#endif


            // TODO Add a unique id variable to OrderHandling and send to a function that handles incomplete orders (orderLeft)            
            if (!orderIsLeft) {
                PrintTable.PrintConsole("yes");
                
                orderHandling.buySymbolsDate.magicNumber.Buy.WaitingTimeForNextPriceUpdate += 500;                 // USE to ML_4
                orderHandling.sellSymbolsDate.magicNumber.Sell.WaitingTimeForNextPriceUpdate += 500;               // USE to ML_4
                if (orderHandling.itsBuyArbitrage)                                                                  
                    orderHandling.arbitrageSymbolsDate.magicNumber.Buy.WaitingTimeForNextPriceUpdate += 500;       // USE to ML_4
                else                                                                                                
                    orderHandling.arbitrageSymbolsDate.magicNumber.Sell.WaitingTimeForNextPriceUpdate += 500;      // USE to ML_4

            }

            orderHandling.percent_end = orderHandling.revnuCalculation(orderHandling.percent_3);
            orderHandling.WalletResultEnd();
            PrintTable.Start(StaticVariables.pathWithDate + "WalletResultReal.csv", orderHandling.summaryTradeReal, "WalletResultReal");
            PrintTable.Start(StaticVariables.pathWithDate + "WalletResult.csv", orderHandling.summaryTrade, "WalletResult");

            PrintFunc.AddLine(StaticVariables.pathWithDate +  "main.txt", orderHandling.PrintResult());
            PrintTable.Start(StaticVariables.pathWithDate + "OrderHandlingData.csv", orderHandling.PrintResult_2(), "OrderHandling");
#if DEBUG
            PrintFunc.AddLine(StaticVariables.pathDebug + orderHandling.currency + "/end_" + orderHandling.currency + ".csv", orderHandling.debug);
#endif           
            result = orderHandling.succsseTrade;
            return result;
        }


        public static bool HandlingOneOrderTrade(OrderHandling orderHandling, bool firstOrder)
        {
            orderHandling.OrderToCare.StartTrade();
            orderHandling.OrderToCare.Result = TradeUseCase.Order(orderHandling.OrderToCare.Request);
            string resPrint = String.Format("Handling Order - {0}-{1}, ExtraPercent {2:P0}, Percentage(without ExtraPercent) - {3:P3}, realPercentage - {4:P3}", (orderHandling.OrderToCare.request.IsBuy ? "Buy" :"Sell"), orderHandling.OrderToCare.request.Symbol, orderHandling.OrderToCare.extraPercent.Percent, orderHandling.percent / 100, orderHandling.realPercentage / 100);
            PrintTable.PrintConsole(resPrint);
            int useCase = 0;
            decimal difPrecentge = 0;

            System.Threading.Thread.Sleep(orderHandling.OrderToCare.ExtraPercent.WaitingTimeForNextPriceUpdate);
            orderHandling.OrderToCare.Result = TradeUseCase.OrderDetails(orderHandling.OrderToCare.Result);

            DateTime startTimeSmallAmount = DateTime.Now; 
            while (!orderHandling.OrderToCare.Done)
            {
                if (orderHandling.OrderToCare.itsCanAdded & orderHandling.OrderToCare.itsCanUpdate)
                {
                    startTimeSmallAmount = DateTime.Now;    
                    if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.FilledPartially)
                    {
                        difPrecentge = orderHandling.OrderToCare.Result.AmountFilled / orderHandling.OrderToCare.Result.Amount;
                        if (difPrecentge > 0.99m)
                        {
                            useCase = 4; // Current Order Cancellation 
                        }
                        else
                        {
                            useCase = 3;  // We will also update the price in addition to the quantity update
                        }
                    }
                    else // if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.Pending)
                    {
                        useCase = 1; // Update price
                    }

                    TradeUseCase.Start(useCase, orderHandling);
                }
                else if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.Filled)
                {
                    return true; 
                }
                else if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.FilledPartially)
                {
                    if (firstOrder)     // We will cancel an order only in the case of a first order, because in other orders we will want the order to remain until it is executed
                    {
                        DateTime timeSmallAmount = DateTime.Now;
                        if (!(timeSmallAmount.Subtract(startTimeSmallAmount).TotalMinutes > 1))  // Limit of waiting for order to 1 minute
                        {
                            if ((orderHandling.OrderToCare.Result.AmountFilled + orderHandling.OrderToCare.AmountFilledDifferentOrderNumber) < orderHandling.minAmountTrade)  // try fix bug of buying a small amount of coins, In use case of the quantity is less than the minimum for the trade, we will continue to wait until at least the required minimum is filled
                                continue;
                        }
                        else
                        {
                            useCase = 4; // Current Order Cancellation
                            TradeUseCase.Start(useCase, orderHandling);

                            if (orderHandling.OrderToCare.amountFilled < orderHandling.minAmountTrade)  // try fix bug of buying a small amount of coins, In use case of the quantity is less than the minimum for the trade, we will continue to wait until at least the required minimum is filled
                            {
                                ExchangeOrderRequest revertOrder = new ExchangeOrderRequest();
                                try
                                {                                   
                                    revertOrder.Amount = orderHandling.OrderToCare.amountFilled;
                                    revertOrder.IsBuy = false;
                                    revertOrder.OrderType = StaticVariables.orderType;
                                    revertOrder.Price = orderHandling.OrderToCare.Request.Price * (1 + (StaticVariables.FeeTrade*2));
                                    revertOrder.Symbol = orderHandling.OrderToCare.Request.Symbol;
                                    OrderTrade revertOrderTrade = new OrderTrade(revertOrder);
                                    revertOrderTrade.Result = StaticVariables.api.PlaceOrder(revertOrder);

                                    PrintTable.Start(StaticVariables.pathWithDate + "Small_Amount_Handling.csv", orderHandling.OrderToCare.Result.PrintSymbol(), "OrderResultSymbol");                                   
                                    PrintTable.Start(StaticVariables.pathWithDate + "Small_Amount_Handling.csv", revertOrderTrade.Result.PrintSymbol(), "OrderResultSymbol");
                                }
                                catch (Exception ex)
                                {
                                    StaticVariables.Wallet = WalletFunc.GetWallet();     // Wallet update. Because part of the trade was carried out. Apparently the amounts of coins have changed

                                    string warningMessage = String.Format("{0},\tamountFilled - {1},\tAmount - {2},\tminAmountTrade - {3},", orderHandling.OrderToCare.request.Symbol,  orderHandling.OrderToCare.amountFilled, orderHandling.OrderToCare.request.Amount, orderHandling.minAmountTrade);
                                    PrintTable.PrintConsole(warningMessage);
                                    PrintFunc.AddLine(StaticVariables.pathWithDate + "Small_Amount_Left.txt", warningMessage);
                                    PrintTable.Start(StaticVariables.pathWithDate + "Small_Amount_Left.csv", orderHandling.OrderToCare.request.Print(), "OrderResult");

                                    DateTime localDate = DateTime.Now;
                                    string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                                    PrintException.Start("Small_Amount_Left", printResult);

                                }
                            }

                            return orderHandling.OrderToCare.succsseFirstOrder;
                        }
                    }
                    else
                    {
                        orderHandling.OrderToCare.ItsOrderLeft = true;
                        return false;
                    }                     
                }
                else if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.Pending) 
                {
                    if (firstOrder)     // We will cancel an order only in the case of a first order, because in other orders we will want the order to remain until it is executed
                    {
                        useCase = 4;    // Current Order Cancellation
                        TradeUseCase.Start(useCase, orderHandling);
                        return orderHandling.OrderToCare.succsseFirstOrder;
                    }
                    else
                    {
                        orderHandling.OrderToCare.ItsOrderLeft = true;
                        return false;
                    }                                       
                }


                if (!orderHandling.OrderToCare.Done)
                {
                    System.Threading.Thread.Sleep(orderHandling.OrderToCare.ExtraPercent.WaitingTimeForNextPriceUpdate);
                    orderHandling.OrderToCare.Result = TradeUseCase.OrderDetails(orderHandling.OrderToCare.Result);
                    if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.Canceled)     // In case a cancellation was made by the stock exchange due to an order or quantity error
                    {
                        bool checkCancel = CancellationFunc.ReviewCancellationAndUpdateOrder(orderHandling);

                        if (firstOrder)    
                        {
                            return orderHandling.OrderToCare.succsseFirstOrder;
                        }
                        else
                        {
                            // TODO Check what amount has not traded. Maybe by the wallet. Update amount and send request in the current function
                        }


                    }
                }
            }

            if (firstOrder)
            {
                return orderHandling.OrderToCare.succsseFirstOrder;   //  amountFilled > minAmaunt
            }
            else
            {
                return orderHandling.OrderToCare.succsseTrade;                 // ((amountFilled == amountStart) || (amountFilled >= amountFinish))
            }                       
        }
    }
}
