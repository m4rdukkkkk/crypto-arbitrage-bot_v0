/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

namespace InternalArbitrage
{
    public class ExtraPercent
    {
        private bool doUpdate;
        private DateTime currentUpdateTime;

        public decimal Percent { get; set; }
        public bool SuccessLastUpdate { get; set; }
        public DateTime InitializationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        // ML_1
        public int UpdateDirection { get; set; }                         // USE to ML_1
        public int MagicUpdateDirection { get; set; }                    // USE to ML_1
        public bool UseMagicUpdateDirection { get; set; }                // USE to ML_1        

        // ML_2
        public double MagicTimeHasPassed { get; set; }                   // USE to ML_2     
        public bool UseMagicTimeHasPassed { get; set; }                  // USE to ML_2     

        // Collect information for USE ML_1
        public int UpdatedUpTimesRejected { get; set; }                  // USE to ML_1
        public int NumUpdateUp { get; set; }                             // USE to ML_1
        public int NumUpdateDown { get; set; }                           // USE to ML_1

        // TODO ML_3
        //public int SumGeneralUpdates { get; set; }                     // TODO ML_3 Summary General number of updates + number of update attempts                      
        //public int UpdatedDownTimesUpLimit { get; set; }               // TODO ML_3 
        //public int UpdatedUpTimesUpLimit { get; set; }                 // TODO ML_3 

        // ML_4
        public int WaitingTimeForNextPriceUpdate { get; set; }           // USE to ML_4        
        public int NumSuccess { get; set; }                              // USE to ML_4
        public int NumUnSuccess { get; set; }                            // USE to ML_4
        public bool IsArbitrageSymbol { get; set; }                      // USE to ML_4   

        public bool UpdatePercent(decimal value, bool initial = false)
        {
            doUpdate = true;          

            if (value > Percent)
                doUpdate = UpdateUp(value);
            else if (value < Percent)
                doUpdate = UpdateDown(value);

            if (doUpdate)
                PercentLimit(value);

            return doUpdate;
        }

        public bool UpdateUp(decimal _value, bool initial = false)
        {

            if (SuccessLastUpdate)
            {
                if(!initial)
                    NumUpdateUp++;
                return true;
            }

            currentUpdateTime = DateTime.Now;
            if (currentUpdateTime.Subtract(LastUpdateTime).TotalMinutes > MagicTimeHasPassed)
            {
                if (!initial)
                    NumUpdateUp++;
                UseMagicTimeHasPassed = true;
                return true;
            }

            GeneralUpdates();
            if (MagicUpdateDirection > UpdateDirection)
            {
                if (!initial)
                    NumUpdateUp++;
                UseMagicUpdateDirection = true;
                return true;
            }

            UpdatedUpTimesRejected ++;
            return false;
        }

        public bool UpdateDown(decimal _value, bool initial = false)
        {
            if (initial)
                return true;

            if (UseMagicUpdateDirection)
            {
                MagicUpdateDirection += UpdatedUpTimesRejected;
                UseMagicUpdateDirection = false;
            }

            if (UseMagicTimeHasPassed)
            {
                MagicTimeHasPassed++;
                UseMagicTimeHasPassed = false;
            }

            if (!initial)
            NumUpdateUp++;
            return true;
        }

        public void PercentLimit(decimal _value)
        {
            if (_value > 1)
            {
                Percent = 1;
                //UpdatedUpTimesUpLimit++;      // TODO ML_3 
            }
            else if (_value < 0)
            {
                Percent = 0;
                //UpdatedDownTimesUpLimit++;    // TODO ML_3 
            }
            else
            {
                Percent = _value;
            }

            if (!UseMagicTimeHasPassed)
                LastUpdateTime = DateTime.Now;
        }

        private void GeneralUpdates()
        {
            UpdateDirection = (NumUpdateUp+ UpdatedUpTimesRejected) - NumUpdateDown;
        }

        // call by OrderTrade.SuccsseExtraPercent
        public void SuccessUpdate(bool success) 
        {
                SuccessLastUpdate = success;
                if (success)
                {
                    NumSuccess++;       // USE to ML_4

                    if (UseMagicUpdateDirection)
                    {
                        MagicUpdateDirection--;
                        UseMagicUpdateDirection = false;
                    }

                    if (UseMagicTimeHasPassed)
                    {
                        MagicTimeHasPassed -= 0.5;
                        UseMagicTimeHasPassed = false;
                    }
                }
                else
                {            
                    NumUnSuccess++;     // USE to ML_4

                if (UseMagicUpdateDirection)
                    {
                        MagicUpdateDirection += UpdatedUpTimesRejected;
                        UseMagicUpdateDirection = false;
                    }

                    if (UseMagicTimeHasPassed)
                    {
                        MagicTimeHasPassed++;
                        UseMagicTimeHasPassed = false;
                    }
                }
        }

        public ExtraPercent() { }

        public ExtraPercent(decimal _percent, int _waitingTimeForOrderUpdate,bool isArbitrageSymbol = false)
        {
            Percent = _percent;
            SuccessLastUpdate = true;
            WaitingTimeForNextPriceUpdate = _waitingTimeForOrderUpdate;
            InitializationTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            NumUpdateUp = 0;
            NumUpdateDown = 0;            
            UpdatedUpTimesRejected = 0;           
            UpdateDirection = 0;
            MagicUpdateDirection = 2;
            UseMagicUpdateDirection = false;
            MagicTimeHasPassed = 5;
            UseMagicTimeHasPassed = false;
            IsArbitrageSymbol = isArbitrageSymbol;
            NumSuccess = 0;
            NumUnSuccess = 0;
            //SumGeneralUpdates = 0;            // TODO ML_3 
            //UpdatedDownTimesUpLimit = 0;      // TODO ML_3 
            //UpdatedUpTimesUpLimit = 0;        // TODO ML_3 
        }        
    }
}
