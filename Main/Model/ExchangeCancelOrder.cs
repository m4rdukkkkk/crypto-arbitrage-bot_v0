/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com
Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

namespace Main
{
    /// <summary>
    /// Order request details
    /// </summary>
    //[System.Serializable]
    public class ExchangeCancelOrder
    {
        private ExchangeOrderResult details;
        private decimal difPrecentge;

        public ExchangeAPIOrderResult Success;

        public ExchangeOrderResult Details
        {
            get
            {
                return details;
            }
            set
            {
                details = value;
                details.Result = (details.AmountFilled == details.Amount ? ExchangeAPIOrderResult.Filled : (details.AmountFilled == 0 ? ExchangeAPIOrderResult.Pending : ExchangeAPIOrderResult.FilledPartially));
                Success = (details.AmountFilled == details.Amount ? ExchangeAPIOrderResult.Filled : (details.AmountFilled == 0 ? ExchangeAPIOrderResult.Canceled : ExchangeAPIOrderResult.FilledPartially));
                if (Success == ExchangeAPIOrderResult.FilledPartially)
                {                   
                    difPrecentge = details.AmountFilled / details.Amount;
                    if (difPrecentge < 0.01m)
                    {
                        Success = ExchangeAPIOrderResult.Canceled;
                        details.Result = ExchangeAPIOrderResult.Canceled;
                    }
                    else if (difPrecentge > 0.99m)
                    {
                        Success = ExchangeAPIOrderResult.Filled;
                        details.Result = ExchangeAPIOrderResult.Filled;
                    }
                }
            }
        }
        /// <summary>
        /// Symbol or pair for the order, i.e. btcusd
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>Order datetime in UTC</summary>
        public DateTime OrderDate { get; set; }

        public bool Result { get; set; }

        public string OrderId { get; set; }


        public ExchangeCancelOrder() {}

        public ExchangeCancelOrder(string symbol)
        {
            Symbol = symbol;           
        }

        public override string ToString()
        {
            OrderDate = DateTime.Now;
            return $"Result - {Result}, Success - {Success}, OrderId - {OrderId}, Symbol - {Symbol}, {OrderDate}";
        }
    }
}