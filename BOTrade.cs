using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NQuotes;

namespace biiuse
{
    class BOTrade : Trade
    {
        public BOTrade(string _strategyLabel, int _magicNumber, TradeType _tradeType, int _lotDigits, int _lengthIn1MBarsOfWaitingPeriod, double _maxBalanceRisk, string _logFileName, int _rangeBufferInMicroPips, int _lookbackDaysForStopLossAdjustment, double _atr, int _emailNotificationLevel, MqlApi mql4) : base(_strategyLabel, false, _lotDigits, _logFileName, _rangeBufferInMicroPips, _emailNotificationLevel, mql4)
        {
            this.tradeType = _tradeType;
            this.magicNumber = _magicNumber;
            this.maxBalanceRisk = _maxBalanceRisk;
            this.lengthIn1MBarsOfWaitingPeriod = _lengthIn1MBarsOfWaitingPeriod;
            this.lookbackDaysForStopLossAdjustment = _lookbackDaysForStopLossAdjustment;
            this.atr = _atr;
        }

        public int getMagicNumber()
        {
            return magicNumber;
        }

        public double getATR()
        {
            return atr;
        }

        public int getLengthIn1MBarsOfWaitingPeriod()
        {
            return lengthIn1MBarsOfWaitingPeriod;
        }
        public double getMaxBalanceRisk()
        {
            return this.maxBalanceRisk;
        }

        public int getLookBackDaysForStopLossAdjustment()
        {
            return lookbackDaysForStopLossAdjustment;
        }

        private int magicNumber;
        private int lengthIn1MBarsOfWaitingPeriod;
        private double maxBalanceRisk;
        private int lookbackDaysForStopLossAdjustment;
        private double atr;
    }
}
