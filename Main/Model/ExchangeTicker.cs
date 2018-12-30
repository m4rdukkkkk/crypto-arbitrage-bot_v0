/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com
Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;

namespace Main
{
    /// <summary>
    /// Details of the current price of an exchange asset
    /// </summary>
    public class ExchangeTicker
    {
        /// <summary>
        /// The bid is the price to sell at
        /// </summary>
        public decimal Bid { get; set; }

        public decimal BidAmount { get; set; }

        /// <summary>
        /// The ask is the price to buy at
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The last trade purchase price
        /// </summary>
        public decimal Last { get; set; }


        public decimal AskAmount { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal AvgPrice { get; set; }       
        public int PriceStepSize { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Volume info
        /// </summary>
        public ExchangeVolume Volume { get; set; }

        /// <summary>
        /// Get a string for this ticker
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return string.Format("Bid: {0}, BidAmount: {1}, Ask: {2}, AskAmount: {3},  Last: {4}, High: {5}, Low: {6}, PriceStepSize: {7}", Bid, BidAmount, Ask, AskAmount, Last, High, Low, PriceStepSize);
        }

        /// <summary>
        /// Write to writer
        /// </summary>
        /// <param name="writer">Writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write((double)Bid);
            writer.Write((double)Ask);
            writer.Write((double)Last);
            Volume.ToBinary(writer);
        }

        /// <summary>
        /// Read from reader
        /// </summary>
        /// <param name="reader">Reader</param>
        public void FromBinary(BinaryReader reader)
        {
            Bid = (decimal)reader.ReadDouble();
            Ask = (decimal)reader.ReadDouble();
            Last = (decimal)reader.ReadDouble();
            Volume = (Volume ?? new ExchangeVolume());
            Volume.FromBinary(reader);
        }
    }

    /// <summary>
    /// Info about exchange volume
    /// </summary>
    public class ExchangeVolume

    {
        /// <summary>
        /// Last volume update timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Price symbol - will equal quantity symbol if exchange doesn't break it out by price unit and quantity unit
        /// </summary>
        public string PriceSymbol { get; set; }

        /// <summary>
        /// Price amount - will equal QuantityAmount if exchange doesn't break it out by price unit and quantity unit
        /// </summary>
        public decimal PriceAmount { get; set; }

        /// <summary>
        /// Quantity symbol (converted into this unit)
        /// </summary>
        public string QuantitySymbol { get; set; }

        /// <summary>
        /// Quantity amount (this many units total)
        /// </summary>
        public decimal QuantityAmount { get; set; }

        /// <summary>
        /// Write to a binary writer
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(Timestamp.ToUniversalTime().Ticks);
            writer.Write(PriceSymbol);
            writer.Write((double)PriceAmount);
            writer.Write(QuantitySymbol);
            writer.Write((double)QuantityAmount);
        }

        /// <summary>
        /// Read from a binary reader
        /// </summary>
        /// <param name="reader">Binary reader</param>
        public void FromBinary(BinaryReader reader)
        {
            Timestamp = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            PriceSymbol = reader.ReadString();
            PriceAmount = (decimal)reader.ReadDouble();
            QuantitySymbol = reader.ReadString();
            QuantityAmount = (decimal)reader.ReadDouble();
        }
    }
}
