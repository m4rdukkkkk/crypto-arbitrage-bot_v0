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
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Main
{
    public class ExchangeBinanceAPI : ExchangeAPI
    {
        public override string BaseUrl { get; set; } = "https://www.binance.com/api/v1";
        public string BaseUrlPrivate { get; set; } = "https://www.binance.com/api/v3";
        public string BaseUrlWithdraw { get; set; } = "https://www.binance.com/wapi/v3";
        public override string Name => ExchangeName.Binance;
        List<string> symbols;
        public static Dictionary<string, string> OneSymbolList = new Dictionary<string, string>();
        private IEnumerable<ExchangeMarket> exchangeMarkets;
        Dictionary<string, string> NormalizeSymbolList;
        List<string> normalSymbolesList;
        Dictionary<string, ExchangeTicker> allTickers = null;
        public override List<string> ListPaymentDontTrade() { return new List<string> { "xrp", "usdc", "pax", "tusd" }; }
       
        public override bool MinAmountByCalculation() { return true; }
        public override bool NeedRoundUp() { return true; }
        public override bool CurrencyTradingFeeReduction() { return false; }
        public override bool RoundingPrice() { return true; }
        public override RateGate RateLimit { get; set; } = new RateGate(1200);

        public Dictionary<string, ExchangeTicker> AllTickers
        {
            get {
                if (allTickers == null)
                    allTickers = GetTickersInternal();

                return allTickers;
            }
            set
            { allTickers = value;}
        }

        public override string NormalizeSymbol(string symbol)
        {           
            string symbole = null;
            if (!(OneSymbolList.TryGetValue(symbol, out symbole)))
            {
                if (!symbol.Contains("_"))      // In case they have already made it to normal
                    return symbol;

                symbol = symbol.Replace("_", string.Empty);
                return symbol.ToUpperInvariant();
            }
            return symbole.ToUpperInvariant(); 
        }

        public override List<string> GetSymbolsNormalize()
        {
            if (normalSymbolesList != null)
                return normalSymbolesList;

            NormalizeSymbolList = ListSymbolKeyAndValue();  
            List<string> symbolesList = GetSymbols().ToList();
            normalSymbolesList = new List<string>();

            string temp;
            foreach (var item in NormalizeSymbolList)
            {
                temp = item.Value.Replace("_", "").ToUpperInvariant();
                if (symbolesList.Contains(temp))
                {
                    normalSymbolesList.Add(item.Value);
                    symbolesList.Remove(temp);
                }
            }


            for (int i = 0; i < symbolesList.Count; i++)
            {
                string tempNew = symbolesList[i].Replace("USDT", "USD").ToLowerInvariant();
                try
                {
                    temp = NormalizeSymbolList[tempNew];
                    normalSymbolesList.Add(temp);
                    OneSymbolList.Add(temp,symbolesList[i]);
                }
                catch (Exception)
                {
                    Console.WriteLine("You need to add the next pair to the  keyList & valueList from binance  - ###  {0}  ###", symbolesList[i].ToLowerInvariant());
                }
            }
          
            return normalSymbolesList;
        }

        private void CheckError(JToken result)
        {
            if (result != null && !(result is JArray) && result["status"] != null && result["code"] != null)
            {
                // TODO parsing error code
                //if (result["code"].ToString() == "-2001") // For returning a message instead of an exception
                //{
                //    // TODO parsing error code 
                //}
                //else
                //{
                //    throw new APIException(result["code"].Value<string>() + ": " + (result["msg"] != null ? result["msg"].Value<string>() : "Unknown Error"));
                //}

                throw new APIException(result["code"].Value<string>() + ": " + (result["msg"] != null ? result["msg"].Value<string>() : "Unknown Error"));
            }
        }
        private bool ParseOrderBook(ExchangeTicker ticker, string symbole, out ExchangeOrderBook book)
        {
            bool result;
            book = new ExchangeOrderBook();
            book.Bids.Add(new ExchangeOrderPrice { Price = ticker.Bid, Amount = ticker.BidAmount, });
            book.Asks.Add(new ExchangeOrderPrice { Price = ticker.Ask, Amount = ticker.AskAmount, });

            decimal minAmount = MinAmountWithRounding(ticker.Bid, symbole);
            result = (ticker.BidAmount > minAmount);

            result = (!result ? false : (ticker.AskAmount > minAmount));

            return result;
        }

        private ExchangeOrderBook ParseOrderBook(JToken token)
        {
            ExchangeOrderBook book = new ExchangeOrderBook();
            foreach (JArray array in token["bids"])
            {
                book.Bids.Add(new ExchangeOrderPrice { Price = (decimal)array[0], Amount = (decimal)array[1] });
            }
            foreach (JArray array in token["asks"])
            {
                book.Asks.Add(new ExchangeOrderPrice { Price = (decimal)array[0], Amount = (decimal)array[1] });
            }
            return book;
        }

        private new Dictionary<string, object> GetNoncePayload()
        {
            RateLimit.WaitToProceed();
            return new Dictionary<string, object>
            {
                { "nonce", ((long)DateTime.UtcNow.UnixTimestampFromDateTimeMilliseconds()).ToString() },
                { "recvWindow", 60000 }
            };
        }

        protected override void ProcessRequest(HttpWebRequest request, Dictionary<string, object> payload)
        {
            if (CanMakeAuthenticatedRequest(payload))
            {
                request.Headers["X-MBX-APIKEY"] = PublicApiKey.ToUnsecureString();
            }
        }

        protected override Uri ProcessRequestUrl(UriBuilder url, Dictionary<string, object> payload)
        {
            if (CanMakeAuthenticatedRequest(payload))
            {
                var query = HttpUtility.ParseQueryString(url.Query);
                string newQuery = "timestamp=" + payload["nonce"].ToString() + (query.Count == 0 ? string.Empty : "&" + query.ToString()) +
                    (payload.Count > 1 ? "&" + GetFormForPayload(payload, false) : string.Empty);
                string signature = CryptoUtility.SHA256Sign(newQuery, CryptoUtility.SecureStringToBytes(PrivateApiKey));
                newQuery += "&signature=" + signature;
                url.Query = newQuery;
                return url.Uri;
            }
            return base.ProcessRequestUrl(url, payload);
        }
       
        public override IEnumerable<string> GetSymbols()
        {
            if (symbols != null)
                return symbols;

            symbols = new List<string>();
            JToken obj = MakeJsonRequest<JToken>("/ticker/allPrices");
            CheckError(obj);
            foreach (JToken token in obj)
            {
                string symbol = (string)token["symbol"];
                if (GetExchangeMarket(symbol).IsActive)
                {
                    symbols.Add(symbol);
                }
                else
                {
                    Console.WriteLine("GetSymbols - {0} - Not Active", symbol); // if symbol not in keyValueList
                }
            }
            return symbols;
        }

        public override ExchangeOrderBook GetOrderBook(string symbol, int maxCount = 100)
        {
            string symbole = NormalizeSymbol(symbol);
            ExchangeTicker ticker = GetTickers(true, true)[symbol];
            ExchangeOrderBook book;
            if (ParseOrderBook(ticker, symbole, out book))
                return book;

            JToken obj = MakeJsonRequest<JToken>("/depth?symbol=" + symbole + "&limit=" + maxCount);
            CheckError(obj);
            return ParseOrderBook(obj);
        }

        public override Dictionary<string, decimal> GetAmountsAvailableToTrade()
        {
            JToken token = MakeJsonRequest<JToken>("/account", BaseUrlPrivate, GetNoncePayload());
            CheckError(token);
            Dictionary<string, decimal> balances = new Dictionary<string, decimal>();
            foreach (JToken balance in token["balances"])
            {
                if((decimal)balance["free"] > 0)
                balances[(string)balance["asset"].ToStringLowerInvariant()] = (decimal)balance["free"];
            }
            return balances;
        }
       
        public override decimal FeeTrade(bool maker)
        { 
            if(maker)
            {
                return 0.99925m;
            }
            else
            {
                return 0.99925m;
            }
        }
   
        public override decimal MinAmount(decimal pairPrice,string symbole)
        {
            ExchangeMarket market = GetExchangeMarket(symbole);
            decimal minNotional = market.MinAmountDiv;

            return minNotional / pairPrice;         
        }

        public override decimal MinAmountWithRounding(decimal pairPrice,string symbole)
        {
            ExchangeMarket market = GetExchangeMarket(symbole);
            decimal minNotional = market.MinAmountDiv;
            decimal result = minNotional / pairPrice;

            result = ClampOrderQuantity(symbole, result);

            decimal sum = result * pairPrice;

            while ( sum < minNotional)
            {
                result += (decimal)market.QuantityStepSize;
                result = ClampOrderQuantity(symbole, result);
                sum = result * pairPrice;
            }

            return result;
        }

        public override int GetRoundPrice(string symbole)
        {            
            ExchangeMarket market = GetExchangeMarket(symbole);            
            decimal outputPrice = ClampOrderPrice(symbole, 9.99999999m);     
            int result = BitConverter.GetBytes(decimal.GetBits(outputPrice)[3])[2]; 
            return result;                 
        }

        public override decimal GetPriceRounding(string symbol, decimal offerPrice)
        {
            symbol = NormalizeSymbol(symbol);
            decimal newPrice = ClampOrderPrice(symbol, offerPrice);
            return newPrice;
        }

        public override ExchangeOrderResult PlaceOrder(ExchangeOrderRequest order)
        {
            string symbol = NormalizeSymbol(order.Symbol);
            Dictionary<string, object> payload = GetNoncePayload();
            payload["symbol"] = symbol;
            payload["side"] = order.IsBuy ? "BUY" : "SELL";
            payload["type"] = order.OrderType.ToStringUpperInvariant();

            // Binance has strict rules on which prices and quantities are allowed. They have to match the rules defined in the market definition.
            decimal outputQuantity = ClampOrderQuantity(symbol, order.Amount);
            decimal outputPrice = ClampOrderPrice(symbol, order.Price);           
            decimal outputStopPrice;

            switch (order.OrderType)
            {
                case OrderType.Limit:
                    payload["timeInForce"] = "GTC";
                    payload["quantity"] = outputQuantity;
                    payload["price"] = outputPrice;
                    break;
                case OrderType.Market:
                    payload["quantity"] = outputQuantity;
                    break;
                case OrderType.STOP_LOSS:
                    payload["quantity"] = outputQuantity;
                    outputStopPrice = ClampOrderPrice(symbol, order.StopPrice);
                    payload["stopPrice"] = outputStopPrice;
                    break;
                case OrderType.STOP_LOSS_LIMIT:
                    payload["timeInForce"] = "GTC";
                    payload["quantity"] = outputQuantity;
                    payload["price"] = outputPrice;
                    outputStopPrice = ClampOrderPrice(symbol, order.StopPrice);
                    payload["stopPrice"] = outputStopPrice;
                    break;
                case OrderType.TAKE_PROFIT:
                    payload["quantity"] = outputQuantity;
                    outputStopPrice = ClampOrderPrice(symbol, order.StopPrice);
                    payload["stopPrice"] = outputStopPrice;
                    break;
                case OrderType.TAKE_PROFIT_LIMIT:
                    payload["timeInForce"] = "GTC";
                    payload["quantity"] = outputQuantity;
                    payload["price"] = outputPrice;
                    outputStopPrice = ClampOrderPrice(symbol, order.StopPrice);
                    payload["stopPrice"] = outputStopPrice;
                    break;
                case OrderType.LIMIT_MAKER:
                    payload["quantity"] = outputQuantity;
                    payload["price"] = outputPrice;
                    break;
            }
            
            JToken token = MakeJsonRequest<JToken>("/order", BaseUrlPrivate, payload, "POST");
            CheckError(token);
            return ParseOrder(token);
        }

  
        private ExchangeOrderResult ParseOrder(JToken token)
        {
            /*
              "symbol": "IOTABTC",
              "orderId": 1,
              "clientOrderId": "abABsrARGZfl5wwdkYrsx1",
              "transactTime": 1510629334993,
              "price": "1.00000000",
              "origQty": "1.00000000",
              "executedQty": "0.00000000",
              "status": "NEW",
              "timeInForce": "GTC",
              "type": "LIMIT",
              "side": "SELL"
            */
            ExchangeOrderResult result = new ExchangeOrderResult
            {
                Amount = (decimal)token["origQty"],
                AmountFilled = (decimal)token["executedQty"],
                AveragePrice = (decimal)token["price"],
                IsBuy = (string)token["side"] == "BUY",
                OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["time"] == null ? (long)token["transactTime"] : (long)token["time"]),
                OrderId = (string)token["orderId"],
                Symbol = (string)token["symbol"]
            };
            switch ((string)token["status"])
            {
                case "NEW":
                    result.Result = ExchangeAPIOrderResult.Pending;
                    break;
                case "PARTIALLY_FILLED":
                    result.Result = ExchangeAPIOrderResult.FilledPartially;
                    break;
                case "FILLED":
                    result.Result = ExchangeAPIOrderResult.Filled;
                    break;
                case "CANCELED":
                    result.Result = ExchangeAPIOrderResult.Canceled;
                    break;
                //case "PENDING_CANCEL":
                //case "EXPIRED":
                case "REJECTED":
                    result.Result = ExchangeAPIOrderResult.Canceled;
                    break;
                default:
                    result.Result = ExchangeAPIOrderResult.Error;
                    break;
            }
            return result;
        }
        
        public override ExchangeOrderResult GetOrderDetails(string orderId, string symbol)
        {
            Dictionary<string, object> payload = GetNoncePayload();
            symbol = NormalizeSymbol(symbol);
            payload["symbol"] = symbol;
            payload["orderId"] = orderId;
            JToken token = MakeJsonRequest<JToken>("/order", BaseUrlPrivate, payload);
            CheckError(token);
            return ParseOrder(token);
        }

        public override ExchangeOrderResult GetOrderDetails(ExchangeOrderResult order)
        {
            Dictionary<string, object> payload = GetNoncePayload();            
            payload["symbol"] = order.Symbol;
            payload["orderId"] = order.OrderId;
            JToken token = MakeJsonRequest<JToken>("/order", BaseUrlPrivate, payload);
            CheckError(token);
            return ParseOrder(token);
        }

        public override IEnumerable<ExchangeOrderResult> GetOpenOrderDetails(string symbol = null)
        {      
            Dictionary<string, object> payload = GetNoncePayload();
            payload["symbol"] = NormalizeSymbol(symbol);
            JToken token = MakeJsonRequest<JToken>("/openOrders", BaseUrlPrivate, payload);
            CheckError(token);
            foreach (JToken order in token)
            {
                yield return ParseOrder(order);
            }
        }
  
        public override ExchangeCancelOrder CancelOrder(ExchangeOrderResult order)
        {

            ExchangeCancelOrder res = new ExchangeCancelOrder(order.Symbol);

            Dictionary<string, object> payload = GetNoncePayload();            
            payload["symbol"] = order.Symbol;
            payload["orderId"] = order.OrderId;

            try
            {
                JToken token = MakeJsonRequest<JToken>("/order", BaseUrlPrivate, payload, "DELETE");
                res.Result = true;
            }
            catch (Exception)
            {
                res.Result = false;
            }

            // For receiving a result even if the order was made or canceled               
            payload = GetNoncePayload();
            payload["symbol"] = order.Symbol;
            payload["orderId"] = order.OrderId;                               
            JToken result = MakeJsonRequest<JToken>("/order", BaseUrlPrivate, payload);
            
            ExchangeOrderResult details = ParseOrder(result);
            res.Details = details;            
            res.OrderId = details.OrderId;
  
            return res;
        }

        public override ExchangeUpdateOrder UpdateOrder(ExchangeOrderResult orderCancel, ExchangeOrderRequest orderNew)
        {
            ExchangeUpdateOrder updateOrder = new ExchangeUpdateOrder();

            decimal minAmount = MinAmountWithRounding(orderNew.Price,orderNew.Symbol);
            if (orderNew.Amount < minAmount)
            {                
                updateOrder.Cancel = new ExchangeCancelOrder();
                updateOrder.Cancel.Details = orderCancel;
                updateOrder.Result = orderCancel;
                updateOrder.ItsUpdate = false;
                updateOrder.OptionToUpdate = false;
                return updateOrder;
            }
            
            updateOrder.Cancel = CancelOrder(orderCancel);
            if (updateOrder.Cancel.Result == true & !(updateOrder.Cancel.Success == ExchangeAPIOrderResult.Filled))
            {
                if (updateOrder.Cancel.Success == ExchangeAPIOrderResult.FilledPartially) // Case of partial cancellation made partial invitation update
                {
                    decimal newAmount = updateOrder.Cancel.Details.Amount - updateOrder.Cancel.Details.AmountFilled;
                    
                    if (newAmount >= minAmount)
                    {
                        orderNew.Amount = newAmount;
                        updateOrder.Result = PlaceOrder(orderNew);
                        updateOrder.ItsUpdate = true;
                    }
                    else
                    {
                        updateOrder.ItsUpdate = false;
                        updateOrder.OptionToUpdate = false;
                        updateOrder.Result = GetOrderDetails(orderCancel);
                    }
                }
                else
                {
                    updateOrder.Result = PlaceOrder(orderNew);
                    updateOrder.ItsUpdate = true;
                }               
            }
            else
            {
                updateOrder.ItsUpdate = false;
                updateOrder.Result = updateOrder.Cancel.Details;
            }

            return updateOrder;
        }

        public override decimal ClampOrderPrice(string symbol, decimal outputPrice)
        {
            ExchangeMarket market = GetExchangeMarket(symbol);
            return market == null ? outputPrice : CryptoUtility.ClampDecimal(market.MinPrice, market.MaxPrice, market.PriceStepSize, outputPrice);
        }
      
        public override decimal ClampOrderQuantity(string symbol, decimal outputQuantity)
        {
            symbol = NormalizeSymbol(symbol);
            ExchangeMarket market = GetExchangeMarket(symbol);
            return market == null ? outputQuantity : CryptoUtility.ClampDecimal(market.MinTradeSize, market.MaxTradeSize, market.QuantityStepSize, outputQuantity);
        }

        public override int GetRoundAmount(string symbole, decimal outputQuantity)
        {
            ExchangeMarket market = GetExchangeMarket(symbole);
            outputQuantity = ClampOrderQuantity(symbole, outputQuantity);
            int result = BitConverter.GetBytes(decimal.GetBits(outputQuantity)[3])[2];
            return result;
        }

        public override ExchangeMarket GetExchangeMarket(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            PopulateExchangeMarkets();
            return exchangeMarkets.FirstOrDefault(x => x.MarketName == symbol);
        }

        private void PopulateExchangeMarkets()
        {
            // Get the exchange markets if we haven't gotten them yet.
            if (exchangeMarkets == null)
            {
                lock (this)
                {
                    if (exchangeMarkets == null)
                    {
                        exchangeMarkets = GetSymbolsMetadata();
                    }
                }
            }
        }

        protected IEnumerable<ExchangeMarket> GetSymbolsMetadata()
        {
            /*
            {
            "symbol": "QTUMETH",
            "status": "TRADING",
            "baseAsset": "QTUM",
            "baseAssetPrecision": 8,
            "quoteAsset": "ETH",
            "quotePrecision": 8,
            "orderTypes": [
                "LIMIT",
                "LIMIT_MAKER",
                "MARKET",
                "STOP_LOSS_LIMIT",
                "TAKE_PROFIT_LIMIT"
            ],
            "icebergAllowed": true,
            "filters": [
                {
                    "filterType": "PRICE_FILTER",
                    "minPrice": "0.00000100",
                    "maxPrice": "100000.00000000",
                    "tickSize": "0.00000100"
                },
                {
                    "filterType": "LOT_SIZE",
                    "minQty": "0.01000000",
                    "maxQty": "90000000.00000000",
                    "stepSize": "0.01000000"
                },
                {
                    "filterType": "MIN_NOTIONAL",
                    "minNotional": "0.01000000"
                }
            ]
            },
            */

            var markets = new List<ExchangeMarket>();
            JToken obj = MakeJsonRequest<JToken>("/exchangeInfo");
            CheckError(obj);
            JToken allSymbols = obj["symbols"];
            foreach (JToken symbol in allSymbols)
            {
                var market = new ExchangeMarket
                {
                    MarketName = symbol["symbol"].ToStringUpperInvariant(),
                    IsActive = ParseMarketStatus(symbol["status"].ToStringUpperInvariant()),
                    BaseCurrency = symbol["quoteAsset"].ToStringUpperInvariant(),
                    MarketCurrency = symbol["baseAsset"].ToStringUpperInvariant()
                };

                // "LOT_SIZE"
                JToken filters = symbol["filters"];
                JToken lotSizeFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "LOT_SIZE"));
                if (lotSizeFilter != null)
                {
                    market.MaxTradeSize = lotSizeFilter["maxQty"].ConvertInvariant<decimal>();
                    market.MinTradeSize = lotSizeFilter["minQty"].ConvertInvariant<decimal>();
                    market.QuantityStepSize = lotSizeFilter["stepSize"].ConvertInvariant<decimal>();
                }

                // PRICE_FILTER
                JToken priceFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "PRICE_FILTER"));
                if (priceFilter != null)
                {
                    market.MaxPrice = priceFilter["maxPrice"].ConvertInvariant<decimal>();
                    market.MinPrice = priceFilter["minPrice"].ConvertInvariant<decimal>();
                    if (market.MaxPrice == 0 || market.MinPrice == 0)
                    {
                        decimal price = AllTickers[market.MarketName].Last;
                        market.MaxPrice = price * 10;
                        market.MinPrice = price * 0.1m;
                    }
                    market.PriceStepSize = priceFilter["tickSize"].ConvertInvariant<decimal>();
                }

                // MIN_NOTIONAL
                JToken notionalFilter = filters?.FirstOrDefault(x => string.Equals(x["filterType"].ToStringUpperInvariant(), "MIN_NOTIONAL"));
                if (notionalFilter != null)
                {
                    market.MinAmountDiv = notionalFilter["minNotional"].ConvertInvariant<decimal>();                   
                }
                markets.Add(market);
            }

            return markets;
        }

        private bool ParseMarketStatus(string status)
        {
            bool isActive = false;
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "TRADING":
                    case "PRE_TRADING":
                    case "POST_TRADING":
                        isActive = true;
                        break;
                        /* case "END_OF_DAY":
                            case "HALT":
                            case "AUCTION_MATCH":
                            case "BREAK": */
                }
            }

            return isActive;
        }

        public override ExchangeTicker GetTicker(string symbol)
        {
            string symbole = NormalizeSymbol(symbol);
            JToken obj = MakeJsonRequest<JToken>("/ticker/24hr?symbol=" + symbole);
            CheckError(obj);
            return ParseTicker(symbol, obj);
        }

        public override Dictionary<string, ExchangeTicker> GetTickers(bool NormalizeSymbol = true , bool newGetTickers = false)
        {
            Dictionary<string, ExchangeTicker> tickers = new Dictionary<string, ExchangeTicker>();

            if (newGetTickers)
            {
                if (ReadCache("Tickers", out Dictionary<string, ExchangeTicker> cachedTickers))
                {
                    return cachedTickers;
                }
                else
                {
                    tickers = GetTickersInternal(!NormalizeSymbol);
                }
            }                
            else
            {
                tickers = AllTickers;
            }

            if (NormalizeSymbol)
            {
                Dictionary<string, ExchangeTicker> newTickers = new Dictionary<string, ExchangeTicker>();
                string symbol;
                string newSymbol;
                string temp;
                foreach (var item in tickers)
                {
                    symbol = item.Key;
                    if (NormalizeSymbol)
                    {
                        temp = symbol.Replace("_", "").ToLowerInvariant();
                        temp = (temp.Contains("usdt") ? temp.Replace("usdt", "usd") : temp);                        
                        try
                        {
                            NormalizeSymbolList.TryGetValue(temp, out newSymbol);
                            newTickers.Add(newSymbol, item.Value);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                newSymbol = OneSymbolList.FirstOrDefault(x => x.Value == symbol).Key;
                                newTickers.Add(newSymbol, item.Value);
                            }
                            catch (Exception)
                            {
                                if (item.Value.IsActive & (item.Value.Volume.PriceSymbol.Length > 5))
                                    Console.WriteLine("GetTickers - NormalizeSymbolList missing - {0} ", symbol); // if symbol not in keyValueList
                            }
                        }                         
                    }
                }
                WriteCache("Tickers", TimeSpan.FromSeconds(30.0), newTickers);

                return newTickers;
            }
            else
                return tickers;
        }

        public Dictionary<string, ExchangeTicker> GetTickersInternal(bool all = true)
        {         
            Dictionary<string, ExchangeTicker> tickers = new Dictionary<string, ExchangeTicker>();
            string symbol;
            JToken obj = MakeJsonRequest<JToken>("/ticker/24hr");
            foreach (JToken child in obj)
            {
                symbol = child["symbol"].ToStringInvariant();
                tickers.Add(symbol, ParseTicker(symbol, child, all));             
            }           
            return tickers;
        }

        private ExchangeTicker ParseTicker(string symbol, JToken token, bool all = true)
        {
            // {"priceChange":"-0.00192300","priceChangePercent":"-4.735","weightedAvgPrice":"0.03980955","prevClosePrice":"0.04056700","lastPrice":"0.03869000","lastQty":"0.69300000","bidPrice":"0.03858500","bidQty":"38.35000000","askPrice":"0.03869000","askQty":"31.90700000","openPrice":"0.04061300","highPrice":"0.04081900","lowPrice":"0.03842000","volume":"128015.84300000","quoteVolume":"5096.25362239","openTime":1512403353766,"closeTime":1512489753766,"firstId":4793094,"lastId":4921546,"count":128453}
            return new ExchangeTicker
            {
                Ask = (decimal)token["askPrice"],
                AskAmount = (decimal)token["askQty"],
                Bid = (decimal)token["bidPrice"],
                BidAmount = (decimal)token["bidQty"],
                Last = (decimal)token["lastPrice"],
                AvgPrice = (decimal)token["weightedAvgPrice"],
                High = (decimal)token["highPrice"],
                Low = (decimal)token["lowPrice"],
                Volume = new ExchangeVolume
                {
                    PriceAmount = (decimal)token["volume"],
                    PriceSymbol = symbol,
                    QuantityAmount = (decimal)token["quoteVolume"],
                    QuantitySymbol = symbol,
                    Timestamp = CryptoUtility.UnixTimeStampToDateTimeMilliseconds((long)token["closeTime"])
                }
            };
        }
    }
}

