/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InternalArbitrage
{
    public static class FindingSymbolsTrading
    {

#if DEBUG
        public static string FindDebug;
#endif

        public static OrderHandling ArbitragePercent(string currency,List<SymbolsDate> list)
        {

#if DEBUG
            DateTime timeOrder = DateTime.Now;
            FindDebug = String.Format("{0}\n", timeOrder);
#endif


            OrderHandling package = null;
            int sumListToCheck = ListToCheck(list);
            if (sumListToCheck < 2)
                return package;

            Dictionary<string, decimal> BuyPriceList = new Dictionary<string, decimal>();
            Dictionary<string, decimal> SellPriceList = new Dictionary<string, decimal>();

            ExchangeOrderBook book;
            string symbole;
            decimal priceBuy;
            decimal priceSell;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].itsAvalible)
                    continue;

                symbole = list[i].symbole;
                book = StaticVariables.api.GetOrderBook(symbole, StaticVariables.maxCount);
                priceBuy = GetPrice(book, list[i], true);
                if (priceBuy != 0)
                    BuyPriceList.Add(symbole, priceBuy);

                priceSell = GetPrice(book, list[i], false);
                if (priceSell != 0)
                    SellPriceList.Add(symbole, priceSell);
            }

            string BuyKey = "";
            string SellKey = "";
            decimal BuyPrice = 0;
            decimal SellPrice = 0;

            while (BuyKey == SellKey)
            {
                // Get the lowest price. So sorting Descending
                BuyKey = BuyPriceList.OrderByDescending(x => x.Value).Select(y => y.Key).ToList().Last();
                BuyPrice = BuyPriceList[BuyKey];
                
                // Getting the highest price. That's why sorting is normal
                SellKey = SellPriceList.OrderBy(x => x.Value).Select(y => y.Key).ToList().Last();
                SellPrice = SellPriceList[SellKey];

#if DEBUG
                FindDebug += String.Format("BuyPriceList\n{0}\n", PrintFunc.PrintDictionary(BuyPriceList));
                FindDebug += String.Format("SellPriceList\n{0}\n", PrintFunc.PrintDictionary(SellPriceList));
                FindDebug += String.Format("BuyKey - {0}\tBuyPrice - {1}\n", BuyKey, BuyPrice);
                FindDebug += String.Format("SellKey - {0}\tSellPrice - {1}\n", SellKey, SellPrice);
#endif

                if (BuyKey != SellKey)
                    break;

                // Handling in case of buying and selling from the same currency
                int indexBuyPriceList = BuyPriceList.Count;
                int indexSellPriceList = SellPriceList.Count;
                if (indexBuyPriceList < 2 || indexSellPriceList < 2)
                    return package;

                // indexBuyPriceList-2 -> This is the index of the next proposal
                decimal BuyNextOffer = BuyPriceList.ElementAt(indexBuyPriceList-2).Value;
                decimal BuyNextOfferDifrent = BuyNextOffer - BuyPrice;
                decimal SellNextOffer = SellPriceList.ElementAt(indexSellPriceList - 2).Value;
                decimal SellNextOfferDifrent = SellPrice - SellNextOffer;

                if (BuyNextOfferDifrent > SellNextOfferDifrent)
                    SellPriceList.Remove(SellKey);
                else
                    BuyPriceList.Remove(BuyKey);

#if DEBUG
                FindDebug += String.Format("BuyNextOffer - {0}\tBuyNextOfferDifrent - {1}\n", BuyNextOffer, BuyNextOfferDifrent);
                FindDebug += String.Format("SellNextOffer - {0}\tSellNextOfferDifrent - {1}\n", SellNextOffer, SellNextOfferDifrent);
                FindDebug += String.Format("(BuyNextOfferDifrent > SellNextOfferDifrent) - {0}\n", (BuyNextOfferDifrent > SellNextOfferDifrent));               
#endif
            }


            decimal precent = 0;
            try
            {
                precent = ((SellPrice - BuyPrice) / BuyPrice) * 100;
            }
            catch (Exception)
            {
                // Division by zero
                return package;
            }
           

            SymbolsDate buy = (from item in list where item.Symbole == BuyKey select item).FirstOrDefault();            
            buy.ItsBuy = true;           

            SymbolsDate sell = (from item in list where item.Symbole == SellKey select item).FirstOrDefault();
            sell.ItsBuy = false;

            // After activating the magic number by buying.ItsBuy / buy.ItsBuy we will check the prices and the percentage of potential profit
            decimal buyPricePotential = WalletFunc.ConversionPrice(buy.orderTrade.request.Price, buy.payment);
            decimal sellPricePotential = WalletFunc.ConversionPrice(sell.orderTrade.request.Price,sell.payment); 
            decimal percentPotential = 0;

            try
            {
                percentPotential = ((sellPricePotential - buyPricePotential) / buyPricePotential) * 100;
            }
            catch (Exception)
            {
                // Division by zero
                return package;
            }

#if DEBUG
            FindDebug += String.Format("precent - {0:P3}\n", precent / 100);
            FindDebug += String.Format("after implemation ExtraPercent\textraPercent.Percent - {0:P2}\n", buy.orderTrade.extraPercent.Percent);
            FindDebug += String.Format("buy.Price - {0:N8}\tbuyPricePotential (ConversionPrice) - {1:N8}\n", buy.orderTrade.request.Price, buyPricePotential);
            FindDebug += String.Format("after implemation ExtraPercent\textraPercent.Percent - {0:P2}\n", sell.orderTrade.extraPercent.Percent);
            FindDebug += String.Format("sell.Price - {0:N8}\tsellPricePotential (ConversionPrice) - {1:N8}\n", sell.orderTrade.request.Price, sellPricePotential);
            FindDebug += String.Format("percentPotential - {0:P3}\n\n\n", percentPotential / 100);
            PrintFunc.AddLine(StaticVariables.pathFindDebug + "Find_" + currency + ".txt", FindDebug);
#endif

            package = new OrderHandling(precent, currency, buy, sell);
            package.buyPrice = WalletFunc.ConversionPrice( buy.orderTrade.maxOrMinPrice , buy.payment); 
            package.sellPrice = WalletFunc.ConversionPrice(sell.orderTrade.maxOrMinPrice, sell.payment);  
            package.buyPricePotential = buyPricePotential;
            package.sellPricePotential = sellPricePotential;
            package.percentPotential = percentPotential;

            return package;
        }
        
        public static decimal GetPrice(ExchangeOrderBook book, SymbolsDate item, bool buy)
        {            
            decimal price = 0;
            decimal amaunt = 0;
            int j = 0;
            do
            {
                if (buy)
                {
                    if (book.Asks.Count <= j)
                        return 0;

                    price = book.Asks[j].Price;
                    amaunt = book.Asks[j].Amount;
                }
                else
                {
                    if (book.Bids.Count <= j)
                        return 0;

                    price = book.Bids[j].Price;
                    amaunt = book.Bids[j].Amount;
                }

                j++;

                if (j == StaticVariables.maxCount) //  In case the first 5 orders in the bookOrder are below the minimum required quantity
                {
                    StaticVariables.maxCount = 10;
                    return 0;
                }

                if (!buy)
                    item.MinAmount = price;

            } while (amaunt < item.MinAmount); 
         
            ExchangeOrderRequest request = new ExchangeOrderRequest();
            request.Amount = amaunt;
            request.IsBuy = buy;
            request.OrderType = StaticVariables.orderType;
            request.Price = price;
            request.Symbol = item.symbole;

            OrderTrade orderTrade = new OrderTrade(request);
            if (buy)
            {
                item.buyOrderTrade = orderTrade;
            }
            else
            {
                item.sellOrderTrade = orderTrade;
            }

            price = WalletFunc.ConversionPrice(price,item.payment);

#if DEBUG
            FindDebug += String.Format("{0}\tSymbol - {1}\n", (request.IsBuy ? "Buy" : "Sell"), request.Symbol);
            FindDebug += String.Format("while (amaunt < item.MinAmount) count - {0}\n", j);
            FindDebug += String.Format("MinAmount - {0}\n", item.MinAmount);
            FindDebug += String.Format("request - {0}\n", request.Print());
            FindDebug += String.Format("price - {0}\n\n", price);
#endif

            return price;
        }

        public static int ListToCheck(List<SymbolsDate> list)
        {
            int result = 0;
            foreach (var item in list)
            {
                if (StaticVariables.WalletAvailable[item.payment])
                {
                    item.itsAvalible = true;
                    result++;
                }                   
                else
                    item.itsAvalible = false;
            }

#if DEBUG
            FindDebug += String.Format("ListToCheck_result -{0}\tlist.Count - {1}\n\n", result, list.Count);
#endif
            return result;
        }
    }
}