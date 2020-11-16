using System;
using System.Collections.Generic;
using PX.Data;

namespace SmartSheetIntegration
{
	public class DiffDateSmartSheet<Operand1, Operand2> : BqlFormulaEvaluator<Operand1, Operand2>
		where Operand1 : IBqlOperand
		where Operand2 : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
            if (pars[typeof(Operand1)] != null && pars[typeof(Operand2)] != null)
            {
                DateTime starDate = (DateTime)pars[typeof(Operand1)];
                DateTime endDate = (DateTime)pars[typeof(Operand2)];
                if (starDate != null && endDate != null)
                {
                    DateTime firstDay = starDate.Date;
                    DateTime lastDay = endDate.Date;

                        TimeSpan span = lastDay - firstDay;
                        int businessDays = span.Days + 1;
                        int fullWeekCount = businessDays / 7;
                        if (businessDays > fullWeekCount * 7)
                        {
                            int firstDayOfWeek = (int)firstDay.DayOfWeek;
                            int lastDayOfWeek = (int)lastDay.DayOfWeek;
                            if (lastDayOfWeek < firstDayOfWeek)
                                lastDayOfWeek += 7;
                            if (firstDayOfWeek <= 6)
                            {
                                if (lastDayOfWeek >= 7)
                                    businessDays -= 2;
                                else if (lastDayOfWeek >= 6)
                                    businessDays -= 1;
                            }
                            else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)
                                businessDays -= 1;
                        }
                        businessDays -= fullWeekCount + fullWeekCount;
                        return businessDays;
                }
            }
            return null;
        }
	}
}
