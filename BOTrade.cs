using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NQuotes;

namespace biiuse
{
    enum Trend
    {
        UP, SIDE, DOWN
    }

    class BOTrade : Trade
    {
        public BOTrade(string _strategyLabel, int _magicNumber, TradeType _tradeType, int _lotDigits, int _lengthIn1MBarsOfWaitingPeriod, double _maxBalanceRisk, string _logFileName, int _rangeBufferInMicroPips, int _lookbackDaysForStopLossAdjustment, double _atr, double _currentSessionRange, double _overallRange, int _emailNotificationLevel, MqlApi mql4) : base(_strategyLabel, false, _lotDigits, _logFileName, _rangeBufferInMicroPips, _emailNotificationLevel, mql4)
        {
            this.tradeType = _tradeType;
            this.magicNumber = _magicNumber;
            this.maxBalanceRisk = _maxBalanceRisk;
            this.lengthIn1MBarsOfWaitingPeriod = _lengthIn1MBarsOfWaitingPeriod;
            this.lookbackDaysForStopLossAdjustment = _lookbackDaysForStopLossAdjustment;
            this.atr = _atr;
            this.currentSessionRange = _currentSessionRange;
            this.overallRange = _overallRange;

            this._1MTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M1, "FXEdgeTrendAverage", 20, 0, 0);
            this._5MTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M5, "FXEdgeTrendAverage", 20, 0, 0);
            this._15MTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M15, "FXEdgeTrendAverage", 20, 0, 0);
            this._30MTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M30, "FXEdgeTrendAverage", 20, 0, 0);
            this._1HTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_H1, "FXEdgeTrendAverage", 20, 0, 0);
            this._4HTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_H4, "FXEdgeTrendAverage", 20, 0, 0);
            this._1DTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_D1, "FXEdgeTrendAverage", 20, 0, 0);
            this._1WTrend = mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_W1, "FXEdgeTrendAverage", 20, 0, 0);

            this.highestHighSinceOpen = 0;
            this.lowestLowSinceOpen = 9999;
        }

        public override void update()
        {
            base.update();

            if ((!isInFinalState()) && isNewBar()) {

                //get daily bar since trend started
                int shift = mql4.iBarShift(null, MqlApi.PERIOD_D1, this.tradeOpenedDate, false);

                double tradeLow = 9999;
                double tradeHigh = 0;
                for (int i = 1; i <= shift; ++i)
                {
                    double sessionLow = mql4.iLow(null, MqlApi.PERIOD_D1, i);
                    double sessionHigh = mql4.iHigh(null, MqlApi.PERIOD_D1, i);
                    if (sessionLow < tradeLow) tradeLow = sessionLow;
                    if (sessionHigh > tradeHigh) tradeHigh = sessionHigh;
                }

                if (tradeHigh > highestHighSinceOpen) highestHighSinceOpen = tradeHigh;
                if (tradeLow < lowestLowSinceOpen) lowestLowSinceOpen = tradeLow;
            }

        }

        public int getMagicNumber()
        {
            return magicNumber;
        }

        public double getATR()
        {
            return atr;
        }


        public double getHighestHighSinceOpen()
        {
            return this.highestHighSinceOpen;
        }

        public double getLowestLowSinceOpen()
        {
            return this.lowestLowSinceOpen;
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

        private Trend doubleToTrend(double _trendInDouble)
        {
            if (_trendInDouble == -1) return Trend.DOWN;
            if (_trendInDouble == 1) return Trend.UP;
            else return Trend.SIDE;
        }

        public override void writeLogToCSV()
        {
            mql4.ResetLastError();
            int openFlags;
            openFlags = MqlApi.FILE_WRITE | MqlApi.FILE_READ | MqlApi.FILE_TXT;
            int filehandle = mql4.FileOpen(this.logFileName, openFlags);
            mql4.FileSeek(filehandle, 0, MqlApi.SEEK_END); //go to the end of the file

            string output;
            //if first entry, write column headers
            if (mql4.FileTell(filehandle) == 0)
            {
                output = "STRATEGY, TRADE_ID, ORDER_TICKET, TRADE_TYPE, SYMBOL, TRADE_OPENED_DATE, ORDER_PLACED_DATE, STARTING_BALANCE, PLANNED_ENTRY, ORDER_FILLED_DATE, ACTUAL_ENTRY, SPREAD_ORDER_OPEN, INITIAL_STOP_LOSS, REVISED_STOP_LOSS, INITIAL_TAKE_PROFIT, REVISED TAKE_PROFIT, CANCEL_PRICE, ACTUAL_CLOSE, SPREAD_ORDER_CLOSE, POSITION_SIZE, MIN_UNREALIZED_PL, MAX_UNREALIZED_PL, REALIZED PL, COMMISSION, SWAP, ENDING_BALANCE, TRADE_CLOSED_DATE, SESSION_RANGE, ATR, OVERALL_RANGE, 1M_TREND, 5M_TREND, 15M_TREND, 30M_TREND, 1H_TREND, 4H_TREND, 1D_TREND, 1W_TREND";
                mql4.FileWriteString(filehandle, output, output.Length);
            }
            output = this.strategyLabel + ", " + this.id + ", " + this.Order.OrderTicket + ", " + this.tradeType + "," + mql4.Symbol() + ", " + ExcelUtil.datetimeToExcelDate(this.tradeOpenedDate) + ", " + ExcelUtil.datetimeToExcelDate(this.orderPlacedDate) + ", " + this.startingBalance + ", " + this.plannedEntry + ", " + ExcelUtil.datetimeToExcelDate(this.orderFilledDate) + ", " + this.actualEntry + ", " + this.spreadOrderOpen + ", " + this.originalStopLoss + ", " + this.stopLoss + ", " + this.initialProfitTarget + ", " + this.takeProfit + ", " + this.cancelPrice + ", " + this.actualClose + ", " + this.spreadOrderClose + ", " + this.positionSize + ", " + this.minUnrealizedPL + ", " + this.maxUnrealizedPL + ", " + this.realizedPL + ", " + this.Order.getOrderCommission() + ", " + this.Order.getOrderSwap() + ", " + this.endingBalance + ", " + ExcelUtil.datetimeToExcelDate(this.tradeClosedDate) + ", " + (this.currentSessionRange * OrderManager.getPipConversionFactor(mql4)) + ", " + this.atr * OrderManager.getPipConversionFactor(mql4) + ", " + this.overallRange * OrderManager.getPipConversionFactor(mql4) + ", " + this._1MTrend + ", " + this._5MTrend + ", " + this._15MTrend + ", " + this._30MTrend + ", " + this._1HTrend + ", " + this._4HTrend + ", " + this._1DTrend + ", " + this._1WTrend;
            mql4.FileWriteString(filehandle, "\n", 1);
            mql4.FileWriteString(filehandle, output, output.Length);
            mql4.FileClose(filehandle);
        }

        private bool isNewBar()
        {
            DateTime curbar = mql4.Time[0];
            if (lastbar != curbar)
            {
                lastbar = curbar;
                return (true);
            }
            else
            {
                return (false);
            }
        }

        private int magicNumber;
        private int lengthIn1MBarsOfWaitingPeriod;
        private double maxBalanceRisk;
        private int lookbackDaysForStopLossAdjustment;
        private double atr;
        private double currentSessionRange;
        private double overallRange;
        private double _1MTrend;
        private double _5MTrend;
        private double _15MTrend;
        private double _30MTrend;
        private double _1HTrend;
        private double _4HTrend;
        private double _1DTrend;
        private double _1WTrend;
        private double highestHighSinceOpen;
        private double lowestLowSinceOpen;

        private System.DateTime lastbar = new System.DateTime();
    }
}
