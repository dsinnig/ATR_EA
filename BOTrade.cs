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
            this._1MTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M1, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._5MTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M5, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._15MTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M15, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._30MTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_M30, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._1HTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_H1, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._4HTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_H4, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._1DTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_D1, "FXEdgeTrend_noDraw", 0, 0, 0));
            this._1WTrend = doubleToTrend(mql4.iCustom(mql4.Symbol(), MqlApi.PERIOD_W1, "FXEdgeTrend_noDraw", 0, 0, 0));
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


        private int magicNumber;
        private int lengthIn1MBarsOfWaitingPeriod;
        private double maxBalanceRisk;
        private int lookbackDaysForStopLossAdjustment;
        private double atr;
        private double currentSessionRange;
        private double overallRange;
        private Trend _1MTrend;
        private Trend _5MTrend;
        private Trend _15MTrend;
        private Trend _30MTrend;
        private Trend _1HTrend;
        private Trend _4HTrend;
        private Trend _1DTrend;
        private Trend _1WTrend;
    }
}
