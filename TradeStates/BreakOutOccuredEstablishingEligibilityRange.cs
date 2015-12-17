using System;
using NQuotes;

namespace biiuse
{
    internal class BreakOutOccuredEstablishingEligibilityRange : TradeState
    {
        private BOTrade context; //hides conext in Trade
        double rangeHigh;
        double rangeLow;
        int barCounter;
        double buffer;
        double prevSessionHigh;
        double prevSessionLow;
        private System.DateTime lastbar = new System.DateTime();


        public BreakOutOccuredEstablishingEligibilityRange(BOTrade _context, double _prevSessionHigh, double _prevSessionLow, MqlApi mql4) : base(mql4)
        {
            this.context = _context;
            this.prevSessionHigh = _prevSessionHigh;
            this.prevSessionLow = _prevSessionLow;
            this.rangeHigh = mql4.High[0];
            this.rangeLow = mql4.Low[0];
            this.barCounter = 0;
            this.buffer = _context.getRangeBufferInMicroPips() / OrderManager.getPipConversionFactor(mql4); ///Works for 5 Digts pairs. Verify that calculation is valid for 3 Digits pairs
        }


        public override void update()
        {
            double factor = OrderManager.getPipConversionFactor(mql4);

            //update range lows and range highs
            if (mql4.Low[0] < rangeLow)
            {
                rangeLow = mql4.Low[0];
            }
            if (mql4.High[0] > rangeHigh)
            {
                rangeHigh = mql4.High[0];
            }


            if (isNewBar()) barCounter++;



            //Waiting Period over? (deault is 10mins + 1min)
            //if(Time[0]-entryTime>=60*(context.getLengthIn1MBarsOfWaitingPeriod()+1))  {
            if (barCounter > context.getLengthIn1MBarsOfWaitingPeriod() + 1)
            {

                //adjust range or buffer pips
                rangeLow -= this.buffer;
                rangeHigh += this.buffer;

                double entryPrice = 0.0;
                double stopLoss = 0.0;
                double cancelPrice = 0.0;
                int orderType = -1;
                TradeState nextState = null;
                double positionSize = 0;
                double oneMicroPip = 1 / OrderManager.getPipConversionFactor(mql4);
                int riskPips = 0;
                double riskCapital = 0.0;

                if (context.getTradeType() == TradeType.LONG)
                {
                    entryPrice = rangeHigh;
                    stopLoss = prevSessionLow - buffer;
                    cancelPrice = prevSessionLow;
                    riskPips = (int)(mql4.MathAbs(stopLoss - entryPrice) * factor);
                    riskCapital = mql4.AccountBalance() * context.getMaxBalanceRisk(); ///Parametrize
                    positionSize = Math.Round(OrderManager.getLotSize(riskCapital, riskPips, mql4), context.getLotDigits(), MidpointRounding.AwayFromZero);
                    orderType = MqlApi.OP_BUYSTOP;
                    nextState = new BuyStopOrderTrendTradePlaced(context, mql4);
                }

                if (context.getTradeType() == TradeType.SHORT)
                {
                    entryPrice = rangeLow;
                    stopLoss = prevSessionHigh + buffer;
                    cancelPrice = prevSessionHigh;
                    riskPips = (int)(mql4.MathAbs(stopLoss - entryPrice) * factor);
                    riskCapital = mql4.AccountBalance() * context.getMaxBalanceRisk(); ///Parametrize
                    positionSize = Math.Round(OrderManager.getLotSize(riskCapital, riskPips, mql4), context.getLotDigits(), MidpointRounding.AwayFromZero);
                    orderType = MqlApi.OP_SELLSTOP;
                    nextState = new SellStopOrderTrendTradePlaced(context, mql4);
                }

                //place Order
                ErrorType result = context.Order.submitNewOrder(orderType, entryPrice, stopLoss, 0, cancelPrice, positionSize, context.getMagicNumber());
                context.setStartingBalance(mql4.AccountBalance());
                context.setOrderPlacedDate(mql4.TimeCurrent());
                context.setSpreadOrderOpen((int)mql4.MarketInfo(mql4.Symbol(), MqlApi.MODE_SPREAD));
                context.setCancelPrice(cancelPrice);
                context.setPlannedEntry(entryPrice);
                context.setStopLoss(stopLoss);
                context.setOriginalStopLoss(stopLoss);
                context.setTakeProfit(0);
                context.setCancelPrice(cancelPrice);
                context.setPositionSize(positionSize);

                if (result == ErrorType.NO_ERROR)
                {
                    context.setState(nextState);
                    //context.addLogEntry("Order successfully placed. Initial Profit target is: " + mql4.DoubleToString(context.getInitialProfitTarget(), mql4.Digits) + " (" + mql4.IntegerToString((int)(mql4.MathAbs(context.getInitialProfitTarget() - context.getPlannedEntry()) * factor)) + " micro pips)" + " Risk is: " + mql4.IntegerToString((int)riskPips) + " micro pips", true);
                    context.addLogEntry(2, "Order successfully placed", "\n",
                                            "Trade Details", "\n",
                                              "AccountBalance: $" + mql4.DoubleToString(mql4.AccountBalance(), 2), "\n",
                                              "Risk Capital: $" + mql4.DoubleToString(riskCapital, 2), "\n",
                                              "Risk pips: " + mql4.DoubleToString(riskPips, 2) + " micro pips", "\n",
                                              "Pip value: " + mql4.DoubleToString(OrderManager.getPipValue(mql4), mql4.Digits), "\n",
                                              "Initial Profit target is: " + mql4.DoubleToString(context.getInitialProfitTarget(), mql4.Digits) + "(" + mql4.IntegerToString((int)(mql4.MathAbs(context.getInitialProfitTarget() - context.getPlannedEntry()) * factor)) + " micro pips)"
                                              );
                    return;
                }
                if ((result == ErrorType.RETRIABLE_ERROR) && (context.Order.OrderTicket == -1))
                {
                    context.addLogEntry("Order entry failed. Error code: " + mql4.IntegerToString(mql4.GetLastError()) + ". Will re-try at next tick", true);

                    return;
                }

                //this should never happen...
                if ((context.Order.OrderTicket != -1) && ((result == ErrorType.RETRIABLE_ERROR) || (result == ErrorType.NON_RETRIABLE_ERROR)))
                {
                    context.addLogEntry("Error ocured but order is still open. Error code: " + mql4.IntegerToString(mql4.GetLastError()) + ". Continue with trade. Initial Profit target is: " + mql4.DoubleToString(context.getInitialProfitTarget(), mql4.Digits) + " (" + mql4.IntegerToString((int)(mql4.MathAbs(context.getInitialProfitTarget() - context.getPlannedEntry()) * factor)) + " micro pips)" + " Risk is: " + mql4.IntegerToString((int)riskPips) + " micro pips", true);
                    context.setState(nextState);
                    return;
                }

                if ((result == ErrorType.NON_RETRIABLE_ERROR) && (context.Order.OrderTicket == -1))
                {
                    context.addLogEntry("Non-recoverable error occurred. Errorcode: " + mql4.IntegerToString(mql4.GetLastError()) + ". Trade will be canceled", true);
                    context.setState(new TradeClosed(context, mql4));
                    return;
                }
            } //end of if that checks if entryPrice is 0.0
        } //end else (that checks for general trade eligibility)




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
    }
}