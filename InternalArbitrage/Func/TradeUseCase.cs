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
    public static partial class TradeUseCase
    {
        public static bool Start(int useCase, OrderHandling orderHandling)
        {
            bool result = true;
            decimal amountToFilled;
            switch (useCase)
            {
                case 0:
                    break;
                case 1: // Update price and send
                    if (Start(5, orderHandling))
                        Start(7, orderHandling);
                    else
                        return false;
                    break;
                case 2: // Update quantity and send
                    if (Start(6, orderHandling))
                        Start(7, orderHandling);
                    else
                        return false;
                    break;
                case 3: // Update quantity and price and send
                    if (Start(5, orderHandling))
                    {
                        if (Start(6, orderHandling))
                            Start(7, orderHandling);
                        else
                            return false;
                    }
                    else
                        return false;
                        
                    break;
                case 4:  // Send Cancel Order                  
                    orderHandling.OrderToCare.Cancel = CancelOrder(orderHandling.OrderToCare.Result);
                    break;
                case 5: // Price update action                  
                    if (!orderHandling.OrderToCare.DownExtraPercent())
                        result = false;                 
                    break;
                case 6: // Quantity update operation                    
                    amountToFilled = orderHandling.OrderToCare.Result.Amount - orderHandling.OrderToCare.Result.AmountFilled;
                    amountToFilled = StaticVariables.api.ClampOrderQuantity(orderHandling.OrderToCare.Request.Symbol, amountToFilled);
                    if (amountToFilled < orderHandling.OrderToCare.MinAmaunt)
                    {
                        orderHandling.OrderToCare.itsCanUpdate = false;
                        return false;
                    }
                                          
                    orderHandling.OrderToCare.Request.Amount = amountToFilled;
                    break;
                case 7: // Submit an update request to the Exchange
                    if (!orderHandling.OrderToCare.itsCanUpdate)
                    {
                        return false;
                    }
                    else
                    {
                        orderHandling.OrderToCare.Update = UpdateOrder(orderHandling.OrderToCare.Result, orderHandling.OrderToCare.Request);
                    }
                    break;
                default:
                    break;
            }

            return result;
        }


        public static ExchangeOrderResult Order(ExchangeOrderRequest order)
        {
            ExchangeOrderResult orderMarket = new ExchangeOrderResult();
            try
            {
                orderMarket = StaticVariables.api.PlaceOrder(order);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}\n{2}", localDate.ToString(), order.ToString(), ex.ToString());
                string fileName = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, StaticVariables.api.Name, (order.IsBuy ? "Buy" : "Sell"));
                PrintException.Start_2(fileName, ex, printResult);
                throw ex;
            }
            return orderMarket;
        }

        public static ExchangeOrderResult OrderDetails(ExchangeOrderResult order)
        {
            ExchangeOrderResult orderMarket = new ExchangeOrderResult();
            try
            {
                orderMarket = StaticVariables.api.GetOrderDetails(order);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}\n{2}", localDate.ToString(), order.ToString(), ex.ToString());
                string fileName = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, StaticVariables.api.Name, (order.IsBuy ? "Buy" : "Sell"));
                PrintException.Start_2(fileName, ex, printResult);
                throw ex;
            }
            return orderMarket;
        }

        public static ExchangeCancelOrder CancelOrder(ExchangeOrderResult order)
        {
            ExchangeCancelOrder orderCancel = new ExchangeCancelOrder();
            try
            {
                orderCancel = StaticVariables.api.CancelOrder(order);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\n{1}\n{2}", localDate.ToString(), order.ToString(), ex.ToString());
                string fileName = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, StaticVariables.api.Name, (order.IsBuy ? "Buy" : "Sell"));
                PrintException.Start_2(fileName, ex, printResult);
                throw ex;
            }
            return orderCancel;
        }

        public static ExchangeUpdateOrder UpdateOrder(ExchangeOrderResult orderCancel, ExchangeOrderRequest orderNew)
        {
            ExchangeUpdateOrder orderUpdate = new ExchangeUpdateOrder();
            try
            {
                orderUpdate = StaticVariables.api.UpdateOrder(orderCancel, orderNew);
            }
            catch (Exception ex)
            {
                DateTime localDate = DateTime.Now;
                string printResult = String.Format("{0}\norderCancel - {1}\norderNew - {2}\n{3}", localDate.ToString(), orderCancel.ToString(), orderNew.ToString(), ex.ToString());
                string fileName = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, StaticVariables.api.Name, (orderNew.IsBuy ? "Buy" : "Sell"));
                PrintException.Start_2(fileName, ex, printResult);
                throw ex;
            }
            return orderUpdate;
        }
    }
}
