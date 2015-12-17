using System;
using NQuotes;

namespace biiuse
{
    internal class BuyOrderFilledTrendTrade : TradeState
    {
        private BOTrade context; //hides conext in Trade
        private DateTime startOfCurDailyBar;
        public BuyOrderFilledTrendTrade(BOTrade aContext, MqlApi mql4) : base(mql4)
        {
            this.context = aContext;
            context.setOrderFilledDate(mql4.TimeCurrent());
            context.Order.OrderType = OrderType.BUY;
            startOfCurDailyBar = mql4.iTime(null, MqlApi.PERIOD_D1, 0);
        }

        public override void update()
        {
            if (!context.Order.getOrderCloseTime().Equals(new DateTime()))
            {
                double pips = mql4.MathAbs(mql4.OrderClosePrice() - context.getActualEntry()) * OrderManager.getPipConversionFactor(mql4);
                string logMessage = "Loss of " + mql4.DoubleToString(pips, 1) + " micro pips.";
                //context.addLogEntry("Stop loss triggered @" + mql4.DoubleToString(mql4.OrderClosePrice(), mql4.Digits) + " " + logMessage, true);
                //context.addLogEntry("P/L of: $" + mql4.DoubleToString(mql4.OrderProfit(), 2) + "; Commission: $" + mql4.DoubleToString(mql4.OrderCommission(), 2) + "; Swap: $" + mql4.DoubleToString(mql4.OrderSwap(), 2) + "; New Account balance: $" + mql4.DoubleToString(mql4.AccountBalance(), 2), true);

                context.addLogEntry(1, "Stop loss triggered @" + mql4.DoubleToString(mql4.OrderClosePrice(), mql4.Digits),
                                          "Stop loss triggered @" + mql4.DoubleToString(mql4.OrderClosePrice(), mql4.Digits), "\n",
                                          logMessage, "\n",
                                          "P/L of: $" + mql4.DoubleToString(mql4.OrderProfit(), 2), "\n",
                                          "Commission: $" + mql4.DoubleToString(mql4.OrderCommission(), 2), "\n",
                                          "Swap: $" + mql4.DoubleToString(mql4.OrderSwap(), 2), "\n",
                                          "New Account balance: $" + mql4.DoubleToString(mql4.AccountBalance(), 2)
                                    );


                context.setRealizedPL(context.Order.getOrderProfit());
                context.setActualClose(context.Order.getOrderClosePrice());

                context.setState(new TradeClosed(context, mql4));
                return;
            }


            //check if new session started

            if ((mql4.iTime(null, MqlApi.PERIOD_D1, 0) - this.startOfCurDailyBar) > TimeSpan.FromHours(1)) {

                this.startOfCurDailyBar = mql4.iTime(null, MqlApi.PERIOD_D1, 0);

                double prevprevDayHigh = mql4.iHigh(null, MqlApi.PERIOD_D1, 2);
                double prevprevDayLow = mql4.iLow(null, MqlApi.PERIOD_D1, 2);
                double closePrevDay = mql4.iClose(null, MqlApi.PERIOD_D1, 1);

                //double prevDayLL = mql4.iLow(mql4.Symbol(), MqlApi.PERIOD_D1, context.getLookBackDaysForStopLossAdjustment());
                double buffer = context.getRangeBufferInMicroPips() / OrderManager.getPipConversionFactor(mql4); ///Check for 3 digit pais
                //double oneMicroPip = 1 / OrderManager.getPipConversionFactor(mql4);
                //if ((prevDayLL - buffer > (context.getStopLoss() + oneMicroPip)) && (mql4.Bid > (prevDayLL - buffer)) && (Math.Abs((prevDayLL - buffer) - mql4.Bid) > context.getATR()))
                if ((closePrevDay > prevprevDayHigh) && (prevprevDayLow - buffer > context.getStopLoss()))

                {
                    //adjust stop loss to prevDayLL
                    context.addLogEntry(1, "Adjust stop loss to previous days's low (minus buffer)",
                                           "Previous day's low is: " + mql4.DoubleToString(prevprevDayLow, mql4.Digits), "\n",
                                           "New stop loss (low-buffer): ", mql4.NormalizeDouble(prevprevDayLow - buffer, mql4.Digits)
                                                   );

                    ErrorType result = context.Order.modifyOrder(context.Order.getOrderOpenPrice(), mql4.NormalizeDouble(prevprevDayLow - buffer, mql4.Digits), 0);


                    if (result == ErrorType.NO_ERROR)
                    {
                        context.setStopLoss(mql4.NormalizeDouble(prevprevDayLow - buffer, mql4.Digits));
                        context.addLogEntry("Stop loss succssfully adjusted", true);
                    }

                    if ((result == ErrorType.RETRIABLE_ERROR) && (context.Order.OrderTicket == -1))
                    {
                        context.addLogEntry("Order modification failed. Error code: " + mql4.IntegerToString(mql4.GetLastError()) + ". Will re-try at next tick", true);
                        return;
                    }

                    if ((result == ErrorType.NON_RETRIABLE_ERROR) && (context.Order.OrderTicket == -1))
                    {
                        context.addLogEntry("Non-recoverable error occurred. Errorcode: " + mql4.IntegerToString(mql4.GetLastError()) + ". Trade will be canceled", true);
                        context.setState(new TradeClosed(context, mql4));
                        return;
                    }
                }

            }

            


            
        }


    }
}