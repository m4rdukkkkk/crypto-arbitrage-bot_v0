/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Main
{
    public class ExchangeUpdateOrder
    {
        public bool OptionToUpdate; // For a case that can not be updated such that the amount below the min '
        private bool itsUpdate;
        private ExchangeCancelOrder cancel;
        private ExchangeOrderResult result;
      
        public bool ItsUpdate
        {
            get { return itsUpdate; }
            set { itsUpdate = value; }
        }

        public ExchangeCancelOrder Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }

        public ExchangeOrderResult Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
                itsUpdate = (result.Result == ExchangeAPIOrderResult.Filled ||
                             result.Result == ExchangeAPIOrderResult.FilledPartially ||
                             result.Result == ExchangeAPIOrderResult.Pending
                             ? true : false);
            }
        }

        public ExchangeUpdateOrder()
        {
            OptionToUpdate = true;
        }

        public override string ToString()
        {
            return cancel.ToString() + result;
        }


    }
}
