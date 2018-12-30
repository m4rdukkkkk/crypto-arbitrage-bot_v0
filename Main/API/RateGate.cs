/*
MIT LICENSE

Copyright (c) 2018 Shalom Malovicki - shalomm2468@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Threading;

namespace Main
{    
    public class RateGate
    {
        /// <summary>
        /// Number of sessions per second
        /// </summary>
        public double numberSessionsMinute;
        // Number of consecutive calls to the API allowed
        public double numCallUnbrokenAPI;
        public double numberSessionsWas;

        public double second = 1000;

        public double secondWait;

        public double totalWait;

        double timePast;
        double reservedTime;
        double timeReduction;
        double numberCallsMinute;

        int totalWaitReduce;

        /// <summary>
        /// Entering critical mode
        /// </summary>
        public bool NotCriticaMode;             // normal mode
        public bool OneOpportunity;             // In a situation where we identified an opportunity and there should be no delay
        public bool sequenceOpportunities;      // Possibility to enter the opportunity mode and therefore if possible in terms of restriction is better dnot be delayed

        DateTime timeNow;
        DateTime timeLast;

        DateTime timeStartNumberCallsMinute;
        DateTime timeEndNumberCallsMinute;

        public RateGate(int NumberSessionsMinute,int NumCallUnbrokenAPI = 0)
        {
            numberSessionsMinute = NumberSessionsMinute;
            if (NumCallUnbrokenAPI == 0)
                numCallUnbrokenAPI = NumberSessionsMinute;
            else
                numCallUnbrokenAPI = NumCallUnbrokenAPI;

            numberSessionsWas = 0;
            second = 1000;
            
            secondWait = (60 / numberSessionsMinute);
            secondWait = secondWait * second;

            NotCriticaMode = true;
            OneOpportunity = false;
            sequenceOpportunities = false;

            timeNow = DateTime.Now;
            timeLast = DateTime.Now;
            timeStartNumberCallsMinute = DateTime.Now;
            timeEndNumberCallsMinute = DateTime.Now;

            timePast = 0;
            reservedTime = 0;
            timeReduction = 0;
            numberCallsMinute = 0;          
        }

        public void WaitToProceed()
        {
            if (numberSessionsMinute == -1)  // Unlimited option by using -1
                return;

            if (numberCallsMinute == 0)      // Reset times if set to 0
                timeStartNumberCallsMinute = timeEndNumberCallsMinute = DateTime.Now;

            NotCriticaMode = (!(OneOpportunity || sequenceOpportunities));  // A result of true if both are false

            if (NotCriticaMode) 
            {
                if (numberSessionsWas == 0)     //  The first call does not wait because the wait will begin only before the next call
                {
                    numberSessionsWas = 1;
                    timeLast = DateTime.Now;            // Reset the last call time after the wait (which is actually the time of calling execution) 
                    timePast = 0;                       // Resetting the time just now for safety
                    reservedTime = 0;                   // Reset the time that has just accumulated to be on the safe side
                    numberCallsMinute = 0;              // Reset call counter. Right now for safety
                    return;
                }              
                else
                {
                    timeNow = DateTime.Now; 
                    timePast = timeNow.Subtract(timeLast).Milliseconds; // Check how long it took in milliseconds. From the last call
                    timeReduction = reservedTime + timePast;            

                    totalWait = secondWait * numberSessionsWas ; 

                    if (timeReduction < totalWait)             
                    {
                        totalWaitReduce = Convert.ToInt32(totalWait - timeReduction); 
                        Thread.Sleep(totalWaitReduce);

                        numberCallsMinute = 0;              
                        timeLast = DateTime.Now;            
                        timePast = 0;                       
                        reservedTime = 0;                   
                        numberSessionsWas = 1;                                               
                        return;
                    }
                    else if (numberCallsMinute <= numberSessionsMinute) 
                    {
                        numberCallsMinute++;                        
                        reservedTime += (timePast - secondWait);    

                        timePast = 0;
                        timeLast = DateTime.Now;        
                        numberSessionsWas = 1;
                        return;
                    }
                    else  
                    {
                        timeEndNumberCallsMinute = DateTime.Now; 
                        if (timeEndNumberCallsMinute.Subtract(timeStartNumberCallsMinute).Milliseconds < 60000)  
                        {
                            Thread.Sleep(timeEndNumberCallsMinute.Subtract(timeStartNumberCallsMinute).Milliseconds);

                            numberCallsMinute = 0; 

                            timeLast = DateTime.Now;    
                            timePast = 0;               
                            reservedTime = 0;           
                            numberSessionsWas = 0;                  
                            return;
                        }
                        else if (timePast < secondWait) 
                        {
                            numberCallsMinute = 0;     

                            totalWaitReduce = Convert.ToInt32(totalWait - timePast);
                            Thread.Sleep(totalWaitReduce);
                            timeLast = DateTime.Now;
                            timePast = 0;
                            reservedTime = 0;
                            numberSessionsWas = 1;
                            return;
                        }
                        else    
                        {
                            numberCallsMinute = 0;  

                            timeLast = DateTime.Now;
                            timePast = 0;
                            reservedTime = 0;
                            numberSessionsWas = 1;
                            return;
                        }

                    }                    
                }
            }
            else if (OneOpportunity)
            {
                numberCallsMinute++; 

                if (numberSessionsWas >= numCallUnbrokenAPI) 
                {
                    DateTime localDate = DateTime.Now;
                    using (StreamWriter sw = File.AppendText(NaneMainDir.GetMainDir() + "ExceptionOneOpportunity.txt"))
                    {
                        sw.WriteLine("To see how we got into such a situation \n" + localDate);
                    }

                    timeNow = DateTime.Now;
                    timePast = timeNow.Subtract(timeLast).Milliseconds;
                    totalWait = secondWait; 
                    totalWaitReduce = Convert.ToInt32(totalWait - timePast);

                    if (totalWaitReduce > 0) 
                    {
                        Thread.Sleep(totalWaitReduce);
                        timeLast = DateTime.Now; 
                        return;
                    }
                    else 
                    {
                        timeLast = DateTime.Now;    
                        numberSessionsWas++;
                        return;
                    }


                }
                else
                {
                    numberSessionsWas++;
                }
            }
            else if (sequenceOpportunities)
            {
                numberCallsMinute++; 

                if ((numberSessionsWas + 2) >= numCallUnbrokenAPI) 
                {
                    numberSessionsWas++;    // Add to reduce current call as well
                    timeNow = DateTime.Now;
                    timePast = timeNow.Subtract(timeLast).Milliseconds;
                    totalWait = numberSessionsWas * secondWait;
                    totalWaitReduce = Convert.ToInt32(totalWait - timePast);
                 
                    Thread.Sleep(totalWaitReduce);
                    timeLast = DateTime.Now;  

                    timePast = 0;               
                    reservedTime = 0;           
                    numberSessionsWas = 0;                      
                    numberCallsMinute = 0;       
                    return;

                }
                else
                {
                    numberSessionsWas++;
                }
            }
// TODO enable
//#if DEBUG
//            using (StreamWriter sw = File.AppendText(NaneMainDir.GetMainDir() + "DebugRateGateLog.txt"))
//            {
//                sw.WriteLine("timeNow               - {0}", timeNow);
//                sw.WriteLine("timeLast              - {0}", timeLast);
//                sw.WriteLine("timePast              - {0}", timePast);
//                sw.WriteLine("totalWait             - {0}", totalWait);
//                sw.WriteLine("totalWaitReduce       - {0}", totalWaitReduce);
//                sw.WriteLine("numberSessionsMinute  - {0}", numberSessionsMinute);
//                sw.WriteLine("numberSessionsWas     - {0}", numberSessionsWas);
//                sw.WriteLine("secondWait            - {0}", secondWait);
//                sw.WriteLine("NotCriticaMode        - {0}", NotCriticaMode);
//                sw.WriteLine("OneOpportunity        - {0}", OneOpportunity);
//                sw.WriteLine("sequenceOpportunities - {0}", sequenceOpportunities);
//                sw.WriteLine("\n\n");
//            }
//#endif
        }
    }
}


