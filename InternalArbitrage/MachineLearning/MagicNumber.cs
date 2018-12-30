/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalArbitrage
{
    [Table("MagicNumber")]
    public class MagicNumber
    {
        [Key]
        public string Symbol { get; set; }
        public ExtraPercent Buy { get; set; }
        public ExtraPercent Sell { get; set; }

        public MagicNumber() { }

        public MagicNumber(string _symbole, decimal _buyExtraPercent, decimal _sellExtraPercent, int _buyWaitingTimeForOrderUpdate, int _sellWaitingTimeForOrderUpdate, bool _isArbitrageSymbol = false)
        {
            Symbol = _symbole;
            Buy = new ExtraPercent(_buyExtraPercent, _buyWaitingTimeForOrderUpdate, _isArbitrageSymbol);
            Sell = new ExtraPercent(_sellExtraPercent, _sellWaitingTimeForOrderUpdate, _isArbitrageSymbol);
        }

        public override string ToString()
        {
            string resPrint = String.Format("{0},Buy.Percent-{1:P0}, Buy.NumSuccess-{2}, Buy.Wait-{3}, Sell.Percent-{4:P0}, Sell.NumSuccess-{5}, Sell.Wait-{6},", Symbol,Buy.Percent, Buy.NumSuccess, Buy.WaitingTimeForNextPriceUpdate,
                                            Sell.Percent, Sell.NumSuccess, Sell.WaitingTimeForNextPriceUpdate);
            return resPrint;
        }
    }
}
