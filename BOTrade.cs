using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NQuotes;

namespace biiuse
{
    class BOTrade : Trade
    {
        public BOTrade(string _strategyLabel, int _lotDigits, string _logFileName, int _rangeBufferInMicroPips, int _emailNotificationLevel, MqlApi mql4) : base(_strategyLabel, false, _lotDigits, _logFileName, _rangeBufferInMicroPips, _emailNotificationLevel, mql4)
        {
            
        }
    }
}
