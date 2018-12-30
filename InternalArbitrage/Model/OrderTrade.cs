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
    public class OrderTrade
    {        
        private bool succsseExtraPercent;       
        private bool done;        
        private decimal startPrice;
        private decimal bestPrice;
        private decimal newPrice;
        private decimal startAddedPercentageDifference;
        public ExtraPercent extraPercent;
        private decimal fullInterval;
        private decimal endPriceInterval;
        private ExchangeOrderResult result;
        private ExchangeCancelOrder cancel;
        private ExchangeUpdateOrder update;
        private DateTime startTimeThisOrder;
        private decimal initialPercent;
        private decimal currentPercent;
        private decimal currentPrice;
        private decimal updatePercentage;
        private ExchangeAPI api;
        private bool roundingPrice;
        private decimal minAmaunt;

        // Fields in which writing or reading don't create a chain reaction
        public ExchangeOrderRequest request;
        public decimal maxOrMinPrice;               // The maximum or minimum price (depending on buy / sell) that still has a profit
        public decimal originalMaxOrMinPrice;       // The maximum or minimum price resulting from ExchangeOrderBook
        public bool succsseTrade;
        public bool succsseFirstOrder;
        public bool send;
        public decimal startPriceInterval;
        public decimal startPriceIntervalPercentage;
        public decimal fullIntervalPercentage;
        public int numUpdate;
        public bool itsBuy;
        public bool itsCanAdded;
        public bool itsMinimumPrice;
        public bool itsCanRevnu;
        public bool itsCanUpdate;
        private bool itsOrderLeft;
        public bool itsFirstOrder;
        public decimal endPrice;
        public decimal endPriceIntervalPercentage;
        public decimal previousMaxOrMinPrice;
        public DateTime startTimeOrder;
        public DateTime endTimeOrder;
        public TimeSpan timeAllOrder;
        public TimeSpan timeThisOrder;
        public decimal amountStart;
        public decimal amountFilled;
        public decimal AmountFilledDifferentOrderNumber;
        public decimal amountFinish;
        public bool itsLimitOrder;

        public void StartTrade()    
        {
            startTimeOrder = DateTime.Now;
            startPrice = Request.Price;
            startPriceInterval = Math.Abs(startPrice - originalMaxOrMinPrice);
            startPriceIntervalPercentage = startPriceInterval / originalMaxOrMinPrice;
            startAddedPercentageDifference = extraPercent.Percent;
            itsCanAdded = (startPriceInterval > 0 ? true : false);
            itsBuy = request.IsBuy;
            decimal _amountStart = request.Amount;
            amountStart = _amountStart;
        }

        // call by Trade_package.StartTradePackage -> ChangeMaxOrMinPriceBuy
        // call by TradeMagic.Start -> ChangeMaxOrMinPriceSell
        // call by TradeMagic.Start -> ChangeMaxOrMinPriceArbitrage
        public decimal MaxOrMinPrice
        {
            set
            {
                if (itsFirstOrder)  // Only the first order will be limited by (value > originalMaxOrMinPrice Or value < originalMaxOrMinPrice)
                {
                    if (request.IsBuy)
                    {
                        maxOrMinPrice = (value > originalMaxOrMinPrice ? originalMaxOrMinPrice : value);
                    }
                    else
                    {
                        maxOrMinPrice = (value < originalMaxOrMinPrice ? originalMaxOrMinPrice : value);
                    }
                }
                else
                {
                    maxOrMinPrice = value;
                }
                // TODO change FullInterval in orderLeft use case
                //if (request.IsBuy)
                //{
                //    FullInterval = maxOrMinPrice - bestPrice;
                //}
                //else
                //{
                //    FullInterval = bestPrice - maxOrMinPrice;
                //}

            }
        }

        // call by TradeUseCase -> UseCase = 5
        // call by this.SuccsseExtraPercent
        public bool DownExtraPercent()
        {
            return UpdateExtraPercent(false);
        }

        // call by FindingSymbolsTrading.ArbitragePercent -> SymbolsDate.ItsBuy for buy
        // call by TradeMagic.Start -> for sell
        // call by TradeMagic.Start -> for arbitrage
        public bool UpExtraPercent()     
        {
            return UpdateExtraPercent(true);
        }

        public bool UpdateExtraPercent(bool up , bool initial = false)
        {            
            currentPercent = extraPercent.Percent;      // reset the data
            updatePercentage = extraPercent.Percent;    // reset the data
            currentPrice = request.Price;               // reset the data
            newPrice = currentPrice;                    // reset the data

            while (currentPrice == newPrice)
            {
                if (!LimitPercent(up, initial))      // Check to see if there is a possibility to change the percentage
                    return false;

                if(extraPercent.UpdatePercent((up ? extraPercent.Percent +  StaticVariables.eachAddPercentage : extraPercent.Percent - StaticVariables.eachAddPercentage),initial))
                    updatePercentage = extraPercent.Percent;
                else
                    return false;

                if (!PriceUpdate())
                    return false;
            }

            if (request.IsBuy)
                updatePercentage = (originalMaxOrMinPrice - newPrice)/ FullInterval;
            else
                updatePercentage = (newPrice - originalMaxOrMinPrice) / FullInterval;


            if (!up & updatePercentage > currentPercent)
            {
                if (extraPercent.UpdatePercent(extraPercent.Percent - StaticVariables.eachAddPercentage, initial))
                    return UpdateExtraPercent(!up, initial);
                else
                    return false;
            }
            else if (up & updatePercentage < currentPercent)
            {
                if(extraPercent.UpdatePercent(extraPercent.Percent + StaticVariables.eachAddPercentage, initial))
                    return UpdateExtraPercent(up, initial);
                else
                    return false;
            }
            else if(extraPercent.UpdatePercent(updatePercentage,initial))
            {             
                if (initial)
                     return true;
              
                request.Price = newPrice;
                numUpdate++;
                return true;                                                                       
            }
            else
            {
                return false;
            }
        }

        public bool LimitPercent(bool up, bool initial = false)
        {
            if (up)
            {
                if (initial)    
                    return true;

                if (updatePercentage >= 1)
                {
                    itsCanAdded = false;    // Used for next update attempt
                    return false;
                }
                else
                    return true;
            }
            else
            {
                if (updatePercentage <= 0)
                {
                    if (initial)
                        return false;

                    itsCanAdded = false;    // Used for next update attempt
                    return false;
                }
                else
                    return true;
            }           
        }

        public bool PriceUpdate()
        {
            if (request.IsBuy)
            {
                newPrice = originalMaxOrMinPrice - (FullInterval * updatePercentage);

                if (roundingPrice)
                    newPrice = api.GetPriceRounding(request.Symbol, newPrice);

                itsMinimumPrice = (newPrice > maxOrMinPrice ? true : false);   // Used for current update attempt
            }
            else
            {
                newPrice = originalMaxOrMinPrice + (FullInterval * updatePercentage);

                if (roundingPrice)
                    newPrice = api.GetPriceRounding(request.Symbol, newPrice);

                itsMinimumPrice = (newPrice < maxOrMinPrice ? true : false);   // Used for current update attempt
            }


            if (itsMinimumPrice)
            {
                itsCanAdded = false;  // Used for next update attempt
                return false;
            }
            else
                return true;
        }

        public void AdjustmentForInitialPlacement()
        {
            initialPercent = extraPercent.Percent;
            if (initialPercent == 0 || initialPercent == 1)
                return;
           
            updatePercentage = extraPercent.Percent;
            currentPrice = request.Price;
            PriceUpdate();

            if (currentPrice != newPrice)
            {
                if (request.IsBuy)
                    updatePercentage = (originalMaxOrMinPrice - newPrice) / FullInterval;
                else
                    updatePercentage = (newPrice - originalMaxOrMinPrice) / FullInterval;

                /// Until the range of eachAddPercentage (10% difference) we will not change the initial extraPercent
                if ((initialPercent <= updatePercentage) && (updatePercentage < (initialPercent + StaticVariables.eachAddPercentage)))
                {
                    if(extraPercent.UpdatePercent(updatePercentage))
                        request.Price = newPrice;
                    
                    return;
                }
                else if ((initialPercent >= updatePercentage) && (updatePercentage > (initialPercent - StaticVariables.eachAddPercentage)))
                {
                    if (extraPercent.UpdatePercent(updatePercentage))
                        request.Price = newPrice;
                    return;
                }
            }                          
            
            decimal tryUpPercent;
            decimal tryDownPercent;

            extraPercent.Percent = initialPercent;      // For the result to be accurate we will reset the data
            UpdateExtraPercent(true, true);
            tryUpPercent = updatePercentage - initialPercent;

            extraPercent.Percent = initialPercent;
            UpdateExtraPercent(false, true);
            tryDownPercent = initialPercent - updatePercentage;

            extraPercent.Percent = initialPercent;
            if (tryUpPercent < tryDownPercent)
                UpdateExtraPercent(true, true);
            else
                UpdateExtraPercent(false, true);

            if(extraPercent.UpdatePercent(updatePercentage))
                request.Price = newPrice;
        }
      
        public ExchangeOrderRequest Request
        {
            get
            {
                startTimeThisOrder = DateTime.Now;
                return request;
            }
            set
            { request = value; }
        }

        // call by SymbolsDate.ItsBuy
        public decimal BestPrice  
        {
            get { return bestPrice; }
            set
            {
                bestPrice = value;
                if (request.IsBuy)
                {
                    FullInterval = originalMaxOrMinPrice - bestPrice;
                }
                else
                {
                    FullInterval = bestPrice - originalMaxOrMinPrice;
                }
            }
        }

        // call by TradeMagic.Start -> OrderHandling.StartOrderHandling
        public decimal MinAmaunt
        {
            get { return minAmaunt; }
            set { minAmaunt = value; }
        }

        // call by SymbolsDate.ItsBuy 
        public ExtraPercent ExtraPercent  // remember!!! active use case 2
        {
            get { return extraPercent; }
            set
            {
                extraPercent = value;
                AdjustmentForInitialPlacement();
            }
        }

        // call by BestPrice
        public virtual decimal FullInterval
        {
            get { return fullInterval; }
            set
            {
                fullInterval = value;
                fullIntervalPercentage = fullInterval / originalMaxOrMinPrice;
            }
        }

        // call by Result
        public bool Done
        {
            get{ return done; }
            set
            {
                done = value;
                if (done)
                {
                    endPrice = result.AveragePrice;
                    endPriceInterval = Math.Abs(endPrice - originalMaxOrMinPrice);
                    endPriceIntervalPercentage = endPriceInterval / originalMaxOrMinPrice;
                    endTimeOrder = DateTime.Now;
                    timeAllOrder = endTimeOrder.Subtract(startTimeOrder);
                    timeThisOrder = endTimeOrder.Subtract(startTimeThisOrder);
                    itsCanAdded = false;
                    itsCanRevnu = false;
                    itsCanUpdate = false;
                    amountFilled = result.AmountFilled + AmountFilledDifferentOrderNumber;
                   

                    if (amountFilled > 0)      
                    {
                        itsLimitOrder = (result.AveragePrice != originalMaxOrMinPrice);
                        amountFinish = (StaticVariables.CurrencyTradingFeeReduction ? (amountStart * api.FeeTrade(itsLimitOrder)) : amountStart);
                        amountFinish = (amountFinish * 0.99m);

                        if (amountFilled >= minAmaunt)
                            succsseFirstOrder = true;

                        if (amountFilled >= amountFinish)
                            succsseTrade = true;
                    }

                    if (amountFilled >= (amountStart * 0.5m) && amountFilled >= minAmaunt)
                        SuccsseExtraPercent = true;
                    else
                        SuccsseExtraPercent = false;
                }
            }
        }

        // call by Done or ItsOrderLeft
        public bool SuccsseExtraPercent
        {
            get { return succsseExtraPercent; }
            set
            {
                succsseExtraPercent = value;
                extraPercent.SuccessUpdate(succsseExtraPercent);

                if (!succsseExtraPercent)   // If order failed. We'll lower the percentage for the next time
                {
                    decimal tempMaxOrMinPrice = maxOrMinPrice;
                    decimal tempPrice = request.Price;
                    decimal tempUpdatePercentage = updatePercentage;
                    maxOrMinPrice = originalMaxOrMinPrice;  // For the purpose we can lower the percentage without the limit of the max/min current price

                    if (!DownExtraPercent())
                        extraPercent.Percent = 0;   // If we were unable to lower the percentage. Reset Percent

                    maxOrMinPrice = tempMaxOrMinPrice;
                    request.Price = tempPrice;
                    updatePercentage = tempUpdatePercentage;
                }               
            }
        }

        // call by this.Cancel or call by this.Update)
        // TODO write note call by
        public ExchangeOrderResult Result 
        {
            get {return result; }
            set
            {
                result = value;
                send = (result != null ? true : false);                              

                if (result.Result == ExchangeAPIOrderResult.Filled || result.Result == ExchangeAPIOrderResult.Canceled)
                    Done = true;               
            }
        }

        // call by TradeUseCase -> UseCase = 4, return from API by TradeUseCase.CancelOrder
        public ExchangeCancelOrder Cancel  
        {
            get { return cancel; }
            set
            {
                cancel = value;
                if (!(cancel.Details.Result == ExchangeAPIOrderResult.Filled))
                {
                    cancel.Details.Result = ExchangeAPIOrderResult.Canceled;    // Because the result is calibrated in case of an update. We need to manually update the result value
                }
                Result = cancel.Details;
            }
        }

        // call by TradeUseCase -> UseCase = 7, return from API by TradeUseCase.UpdateOrder
        public ExchangeUpdateOrder Update  
        {
            get { return update; }
            set
            {
                update = value;
                if (update.Cancel.Success == ExchangeAPIOrderResult.FilledPartially) //   for this need to write the function of UpdateOrder (ExchangeOrderResult orderCancel, ExchangeOrderRequest orderNew)  
                {
                    AmountFilledDifferentOrderNumber += update.Cancel.Details.AmountFilled;
                }
                Result = update.Result;
                itsCanUpdate = update.OptionToUpdate;

            }
        }

        public bool ItsOrderLeft
        {
            get { return itsOrderLeft; }
            set
            {
                itsOrderLeft = value;

                if (amountFilled >= (amountStart * 0.5m) && amountFilled >= minAmaunt)
                    SuccsseExtraPercent = true;
                else
                    SuccsseExtraPercent = false;
            }
        }
                 
        public OrderTrade(ExchangeOrderRequest _request)
        {

            done = false;
            succsseTrade = false;
            succsseFirstOrder = false;
            itsCanAdded = true;
            itsMinimumPrice = false;
            itsCanRevnu = true;
            itsCanUpdate = true;
            itsOrderLeft = false;
            itsFirstOrder = false;
            amountFilled = 0;
            AmountFilledDifferentOrderNumber = 0;
            numUpdate = -1;

            Request = _request;
            decimal valueMaxOrMinPrice = _request.Price;  //  TO VIEW Since the default value transfer "by reference" we want to get  here "by Value" because the _request.Price value Will change during the run when we update the Request 
            originalMaxOrMinPrice = valueMaxOrMinPrice;
            maxOrMinPrice = valueMaxOrMinPrice;
            newPrice = valueMaxOrMinPrice;

            api = StaticVariables.api;
            roundingPrice = StaticVariables.roundingPrice;
        }

        
        public override string ToString()
        {
            string resultToString = String.Format("{0},{1},{2:P2},{3:P2},{4:P2},{5},{6},{7},{8},{9},{10:P2},{11:P2},{12},{13},{14},{15},{16},,{17},{18}", 
                                         (itsBuy ? "buy" : "sell"),
                                         request.Symbol,
                                         endPriceIntervalPercentage,
                                         startPriceIntervalPercentage,
                                         fullIntervalPercentage,
                                         endPrice,
                                         startPrice,                                         
                                         maxOrMinPrice,     
                                         originalMaxOrMinPrice,
                                         bestPrice,
                                         updatePercentage,
                                         extraPercent.Percent,
                                         timeAllOrder.ToString(),
                                         timeThisOrder.ToString(),
                                         ConvertFunc.ConvertFromDateTime(startTimeThisOrder),
                                         ConvertFunc.ConvertFromDateTime(endTimeOrder),
                                         numUpdate,
                                         request.Print(),
                                         Result.Print()
                                          );
            return resultToString;
        }

    }
}
