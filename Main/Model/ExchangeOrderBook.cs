/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Main
{
    /// <summary>
    /// A price entry in an exchange order book
    /// </summary>
    public struct ExchangeOrderPrice
    {
        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public decimal Timestamp { get; set; }

        /// <summary>
        /// Neme of Exchange
        /// </summary>
        public string NemeOfExchange { get; set; }


        public override string ToString()
        {
            long timestamp1 = (long)Timestamp;
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Unspecified);
            dtDateTime = dtDateTime.AddSeconds(timestamp1).ToLocalTime();         

            return "Price: " + Price + ", Amount: " + Amount + ", Timestamp: " + dtDateTime;
        }

        /// <summary>
        /// Write to a binary writer
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write((double)Price);
            writer.Write((double)Amount);
            writer.Write((double)Timestamp);
            writer.Write((string)NemeOfExchange);
        }

        /// <summary>
        /// Constructor from a binary reader
        /// </summary>
        /// <param name="reader">Binary reader to read from</param>
        public ExchangeOrderPrice(BinaryReader reader)
        {
            Price = (decimal)reader.ReadDouble();
            Amount = (decimal)reader.ReadDouble();
            Timestamp =(decimal)reader.ReadDouble();
            NemeOfExchange = reader.ReadString();
        }
    }

    /// <summary>
    /// Represents all the asks (sells) and bids (buys) for an exchange asset
    /// </summary>
    public class ExchangeOrderBook
    {
        /// <summary>
        /// List of asks (sells)
        /// </summary>
        public List<ExchangeOrderPrice> Asks { get; } = new List<ExchangeOrderPrice>();

        /// <summary>
        /// List of bids (buys)
        /// </summary>
        public List<ExchangeOrderPrice> Bids { get; } = new List<ExchangeOrderPrice>();

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            
            string print = "Asks";

            for (int i = 0; i < Asks.Count; i++)
            {
                print += "\t" + Asks[i] + "\n";
            }
            print += "\nBids";
            for (int i = 0; i < Asks.Count; i++)
            {
                print += "\t" + Bids[i] + "\n";
            }

            return print;
        }

        /// <summary>
        /// Write to a binary writer
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(Asks.Count);
            writer.Write(Bids.Count);
            foreach (ExchangeOrderPrice price in Asks)
            {
                price.ToBinary(writer);
            }
            foreach (ExchangeOrderPrice price in Bids)
            {
                price.ToBinary(writer);
            }
        }

        /// <summary>
        /// Read from a binary reader
        /// </summary>
        /// <param name="reader">Binary reader</param>
        public void FromBinary(BinaryReader reader)
        {
            Asks.Clear();
            Bids.Clear();
            int askCount = reader.ReadInt32();
            int bidCount = reader.ReadInt32();
            while (askCount-- > 0)
            {
                Asks.Add(new ExchangeOrderPrice(reader));
            }
            while (bidCount-- > 0)
            {
                Bids.Add(new ExchangeOrderPrice(reader));
            }
        }
    }
}
