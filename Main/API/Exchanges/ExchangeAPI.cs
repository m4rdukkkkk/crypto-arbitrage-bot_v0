/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com
Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Main
{
    /// <summary>
    /// Base class for all exchange API
    /// </summary>
    public abstract class ExchangeAPI : BaseAPI, IExchangeAPI
    {       
        /// <summary>
        /// Normalize a symbol for use on this exchange
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <returns>Normalized symbol</returns>
        public virtual string NormalizeSymbol(string symbol) { return symbol; }

        /// <summary>
        /// Normalize a symbol to a global standard symbol that is the same with all exchange symbols, i.e. btcusd
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Normalized global symbol</returns>
        public virtual string NormalizeSymbolGlobal(string symbol) { return symbol?.Replace("-", "_").Replace("/", "_").ToLowerInvariant(); }
    
        /// <summary>
        /// Get exchange symbols
        /// </summary>
        /// <returns>Array of symbols</returns>
        public virtual IEnumerable<string> GetSymbols() { throw new NotImplementedException(); }

        /// <summary>
        /// Get symbols for the exchange Normalize 
        /// </summary>
        /// <returns>List of symbols</returns>
        public virtual List<string> GetSymbolsNormalize()
        {
            List<string> symbolesList;
            symbolesList = GetSymbols().ToList<string>();
            for (int i = 0; i < symbolesList.Count; i++)
            {
                symbolesList[i] = NormalizeSymbolGlobal(symbolesList[i]);
            }

            return symbolesList; 
        }

        /// <summary>
        /// Get Uniform format of symbols 
        /// </summary>
        /// <returns>List of Uniform format symbols</returns>
        public Dictionary<string, string> ListSymbolKeyAndValue()
        {
            string pathPairList = NaneMainDir.GetFileDir() + "PairList/";
            string pathKey = pathPairList + "keyList.txt";
            string pathValue = pathPairList + "valueList.txt";

            string[] listKey = File.ReadAllLines(pathKey);
            string[] listValue = File.ReadAllLines(pathValue);

            Dictionary<string, string> list_SymbolKeyAndValue = new Dictionary<string, string> { { "", "" } };
            for (int i = 0; i < listKey.Length; i++)
            {
                if (!(listKey[i] == ""))
                {
                    try
                    {
                        list_SymbolKeyAndValue.Add(listKey[i], listValue[i]);
                    }
                    catch (Exception)
                    {

                    }
                }
                    
            }

            return list_SymbolKeyAndValue;
        }
               
        /// <summary>
        /// Get exchange order book
        /// </summary>
        /// <param name="symbol">Symbol to get order book for</param>
        /// <param name="maxCount">Max count, not all exchanges will honor this parameter</param>
        /// <returns>Exchange order book or null if failure</returns>
        public virtual ExchangeOrderBook GetOrderBook(string symbol, int maxCount = 5) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Get exchange order book
        /// </summary>
        /// <param name="symbol">Symbol to get order book for</param>
        /// <param name="maxCount">Max count, not all exchanges will honor this parameter</param>
        /// <returns>Exchange order book or null if failure</returns>
        public Task<ExchangeOrderBook> GetOrderBookAsync(string symbol, int maxCount = 100) => Task.Factory.StartNew(() => GetOrderBook(symbol, maxCount));

        /// <summary>
        /// Get exchange order book all symbols. Not all exchanges support this. Depending on the exchange, the number of bids and asks will have different counts, typically 50-100.
        /// </summary>
        /// <param name="maxCount">Max count of bids and asks - not all exchanges will honor this parameter</param>
        /// <returns>Symbol and order books pairs</returns>
        public virtual IEnumerable<KeyValuePair<string, ExchangeOrderBook>> GetOrderBooks(int maxCount = 100) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Get exchange order book all symbols. Not all exchanges support this. Depending on the exchange, the number of bids and asks will have different counts, typically 50-100.
        /// </summary>
        /// <param name="maxCount">Max count of bids and asks - not all exchanges will honor this parameter</param>
        /// <returns>Symbol and order books pairs</returns>
        public Task<IEnumerable<KeyValuePair<string, ExchangeOrderBook>>> GetOrderBooksAsync(int maxCount = 100) => Task.Factory.StartNew(() => GetOrderBooks(maxCount));

        /// <summary>
        /// Get total amounts, symbol / amount dictionary
        /// </summary>
        /// <returns>Dictionary of symbols and amounts</returns>
        public virtual Dictionary<string, decimal> GetAmounts() { throw new NotSupportedException(); }

        /// <summary>
        /// ASYNC - Get total amounts, symbol / amount dictionary
        /// </summary>
        /// <returns>Dictionary of symbols and amounts</returns>
        public Task<Dictionary<string, decimal>> GetAmountsAsync() => Task.Factory.StartNew(() => GetAmounts());

        /// <summary>
        /// Get amounts available to trade, symbol / amount dictionary
        /// </summary>
        /// <returns>Symbol / amount dictionary</returns>
        public virtual Dictionary<string, decimal> GetAmountsAvailableToTrade() { throw new NotImplementedException(); }


        /// <summary>
        ///  Get amounts available to Margin
        /// </summary>
        /// <returns>Symbol / amount dictionary</returns>
        public virtual Dictionary<string, decimal> GetAmountsAvailableToMargin() { throw new NotImplementedException(); }
        
        /// <summary>
        /// ASYNC - Get amounts available to trade, symbol / amount dictionary
        /// </summary>
        /// <returns>Symbol / amount dictionary</returns>
        public Task<Dictionary<string, decimal>> GetAmountsAvailableToTradeAsync() => Task.Factory.StartNew<Dictionary<string, decimal>>(() => GetAmountsAvailableToTrade());

        /// <summary>
        /// Place an order
        /// </summary>
        /// <param name="order">The order request</param>
        /// <returns>Result</returns>
        public virtual ExchangeOrderResult PlaceOrder(ExchangeOrderRequest order) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Place an order
        /// </summary>
        /// <param name="order">The order request</param>
        /// <returns>Result</returns>
        public Task<ExchangeOrderResult> PlaceOrderAsync(ExchangeOrderRequest order) => Task.Factory.StartNew(() => PlaceOrder(order));

        /// <summary>
        /// Get the details of all completed orders
        /// </summary>
        /// <param name="symbol">Symbol to get completed orders for or null for all</param>
        /// <param name="afterDate">Only returns orders on or after the specified date/time</param>
        /// <returns>All completed order details for the specified symbol, or all if null symbol</returns>
        public virtual IEnumerable<ExchangeOrderResult> GetCompletedOrderDetails(string symbol = null, DateTime? afterDate = null) { throw new NotImplementedException(); }
     
        /// <summary>
        /// ASYNC - Get the details of all completed orders
        /// </summary>
        /// <param name="symbol">Symbol to get completed orders for or null for all</param>
        /// <returns>All completed order details for the specified symbol, or all if null symbol</returns>
        public Task<IEnumerable<ExchangeOrderResult>> GetCompletedOrderDetailsAsync(string symbol = null) => Task.Factory.StartNew(() => GetCompletedOrderDetails(symbol));

        /// <summary>
        /// Get order details
        /// </summary>
        /// <param name="orderId">Order id to get details for</param>
        /// <returns>Order details</returns>
        public virtual ExchangeOrderResult GetOrderDetails(string orderId,string symbol) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Get order details
        /// </summary>
        /// <param name="orderId">Order id to get details for</param>
        /// <returns>Order details</returns>
        public Task<ExchangeOrderResult> GetOrderDetailsAsync(string orderId, string symbol) => Task.Factory.StartNew(() => GetOrderDetails(orderId,symbol));

        /// <summary>
        /// Get the details of all open orders
        /// </summary>
        /// <param name="symbol">Symbol to get open orders for or null for all</param>
        /// <returns>All open order details</returns>
        public virtual IEnumerable<ExchangeOrderResult> GetOpenOrderDetails(string symbol = null) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Get the details of all open orders
        /// </summary>
        /// <param name="symbol">Symbol to get open orders for or null for all</param>
        /// <returns>All open order details</returns>
        public Task<IEnumerable<ExchangeOrderResult>> GetOpenOrderDetailsAsync(string symbol = null) => Task.Factory.StartNew(() => GetOpenOrderDetails());

        /// <summary>
        /// Get the details of Existing order
        /// </summary>
        /// <param name="order">ExchangeOrderResult of Existing order</param>
        /// <returns>result</returns>
        public virtual ExchangeOrderResult GetOrderDetails(ExchangeOrderResult order) { throw new NotImplementedException(); }

        /// <summary>
        /// Cancel Existing order
        /// </summary>
        /// <param name="order">ExchangeOrderResult to cancel</param>
        /// <returns>Result Cancel</returns>
        public virtual ExchangeCancelOrder CancelOrder(ExchangeOrderResult order) { throw new NotImplementedException(); }

        /// <summary>
        /// Update Existing order
        /// </summary>
        /// <param name="orderCancel">old order</param>
        /// <param name="orderNew">new order</param>
        /// <returns>Result Cancel</returns>
        public virtual ExchangeUpdateOrder UpdateOrder(ExchangeOrderResult orderCancel, ExchangeOrderRequest orderNew) { throw new NotImplementedException(); }


        /// <summary>
        /// Get exchange ticker
        /// </summary>
        /// <param name="symbol">Symbol to get ticker for</param>
        /// <returns>Ticker</returns>
        public virtual ExchangeTicker GetTicker(string symbol) { throw new NotImplementedException(); }

        /// <summary>
        /// ASYNC - Get exchange ticker
        /// </summary>
        /// <param name="symbol">Symbol to get ticker for</param>
        /// <returns>Ticker</returns>
        public Task<ExchangeTicker> GetTickerAsync(string symbol) => Task.Factory.StartNew(() => GetTicker(symbol));

        /// <summary>
        /// Get all tickers, not all exchanges support this
        /// </summary>
        /// <returns>Key value pair of symbol and tickers array</returns>
        public virtual Dictionary<string, ExchangeTicker> GetTickers(bool NormalizeSymbol = true, bool newGetTickers = false) { throw new NotImplementedException(); }

        /// add by shalomm2468@gmail.com
        public virtual decimal ClampOrderPrice(string symbol, decimal outputPrice) { return outputPrice; }

        /// add by shalomm2468@gmail.com
        public virtual decimal ClampOrderQuantity(string symbol, decimal outputQuantity)
        {
            decimal mod = outputQuantity % 0.00000001m;
            outputQuantity -= mod;
            return outputQuantity.Normalize();
        }
        
        /// add by shalomm2468@gmail.com
        public virtual  decimal FeeTrade(bool maker) { return 1; }
    
        /// add by shalomm2468@gmail.com
        public virtual decimal MinAmount(decimal pairPrice,string symbole) { return 0m; }

        /// add by shalomm2468@gmail.com
        public virtual ExchangeMarket GetExchangeMarket(string symbol) { throw new NotImplementedException(); }

        /// add by shalomm2468@gmail.com
        public virtual decimal MinAmountWithRounding(decimal pairPrice,string symbole) { return 0m; }

        /// add by shalomm2468@gmail.com
        public virtual int GetRoundAmount(string symbole, decimal outputQuantity = 0.12345678m) { return  8; }

        /// add by shalomm2468@gmail.com
        public virtual int GetRoundPrice(string symbole) { return 8; }

        /// add by shalomm2468@gmail.com
        public virtual decimal GetPriceRounding(string symbol, decimal offerPrice) { return offerPrice; }

        /// add by shalomm2468@gmail.com
        public virtual bool RoundingPrice() { return false; }

        /// add by shalomm2468@gmail.com
        public virtual bool CurrencyTradingFeeReduction() { return true; }
        
        /// add by shalomm2468@gmail.com
        public virtual bool NeedRoundUp() { return false; }

        /// add by shalomm2468@gmail.com
        public virtual bool MinAmountByCalculation() { return false; }

        public virtual List<string> ListPaymentDontTrade() { return null; }
    }

    /// <summary>
    /// List of exchange names
    /// </summary>
    public static class ExchangeName
    {
        /// <summary>
        /// Binance
        /// </summary>
        public const string Binance = "Binance";

        /// <summary>
        /// Bitfinex
        /// </summary>
        public const string Bitfinex = "Bitfinex";

        /// <summary>
        /// Bithumb
        /// </summary>
        public const string Bithumb = "Bithumb";

        /// <summary>
        /// Bitstamp
        /// </summary>
        public const string Bitstamp = "Bitstamp";

        /// <summary>
        /// Bittrex
        /// </summary>
        public const string Bittrex = "Bittrex";

        /// <summary>
        /// GDAX
        /// </summary>
        public const string GDAX = "GDAX";

        /// <summary>
        /// Gemini
        /// </summary>
        public const string Gemini = "Gemini";

        /// <summary>
        /// Kraken
        /// </summary>
        public const string Kraken = "Kraken";

        /// <summary>
        /// Okex
        /// </summary>
        public const string Okex = "Okex";

        /// <summary>
        /// Poloniex
        /// </summary>
        public const string Poloniex = "Poloniex";

        /// <summary>
        /// Huobi
        /// </summary>
        public const string Huobi = "Huobi";

        /// <summary>
        /// Hitbtc
        /// </summary>
        public const string Hitbtc = "Hitbtc";

    }
}
