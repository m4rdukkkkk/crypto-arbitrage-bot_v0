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
    public static partial class CancellationFunc
    {
        public static void Cancellation99(OrderHandling orderHandling)
        {
            if (orderHandling.Done)
                return;         
            
            orderHandling.OrderToCare = orderHandling.Buy;
            CheckAndCancel99(orderHandling);

            orderHandling.OrderToCare = orderHandling.Sell;
            CheckAndCancel99(orderHandling);

            orderHandling.OrderToCare = orderHandling.Arbitrage;
            CheckAndCancel99(orderHandling);

            orderHandling.OrderToCare = orderHandling.Buy;  // Only to be the result of OrderHandling.Done
        }

        public static void CheckAndCancel99(OrderHandling orderHandling)
        {
            if (!orderHandling.OrderToCare.Done)
                orderHandling.OrderToCare.Result = TradeUseCase.OrderDetails(orderHandling.OrderToCare.Result);

            int useCase = 0;
            decimal difPrecentge = 0;

            if (orderHandling.OrderToCare.Result.Result == ExchangeAPIOrderResult.FilledPartially)
            {             
                difPrecentge = orderHandling.OrderToCare.Result.AmountFilled / orderHandling.OrderToCare.Result.Amount;
                if (difPrecentge > 0.99m)
                {
                    useCase = 4; // Current Order Cancellation
                    TradeUseCase.Start(useCase, orderHandling);
                }
            }
        }


        ///Review the cancellation results and update the order accordingly
        public static bool ReviewCancellationAndUpdateOrder(OrderHandling orderHandling)
        {
            bool result = true;
            try
            {
                if (orderHandling.OrderToCare.Cancel.Success == ExchangeAPIOrderResult.Canceled)  // Ostensibly this situation does not belong only in the case of cancellation due to non-execution of the order at all (and not in the case of cancellation due to incomplete filling) because this only happens in details.AmountFilled == 0
                {
                    result = true;
                }
                else if (orderHandling.OrderToCare.Cancel.Success == ExchangeAPIOrderResult.Filled)
                {
                    result = false;
                }
                else if (orderHandling.OrderToCare.Cancel.Success == ExchangeAPIOrderResult.FilledPartially)
                {
                    result = false; //  We will continue on the other side for traded quantity
                }
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}", localDate.ToString(), ex.ToString());
                printResult += String.Format("\nCan not get details about cancellation. Apparently he did not have to check why");
                printResult += String.Format("\nsymbole - {0}, itsBuy - {1}", orderHandling.OrderToCare.request.Symbol, orderHandling.OrderToCare.request.IsBuy);
                PrintException.Start(MethodBase.GetCurrentMethod().Name, printResult);
            }
           
            orderHandling.OrderToCare.Result = orderHandling.OrderToCare.Cancel.Details; // For a goal that will receive its own value and update the value of the variable Done & SuccsseExtraPercent & amountFilled += result.AmountFilled;

            return result;
        }
    }
}
