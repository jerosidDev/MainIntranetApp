using System;
using System.Linq;

namespace Reporting_application.Utilities.Dates
{
    public static class DatesUtilities
    {

        public static int GetNbWorkingDays(DateTime FromDate, DateTime ToDate)
        {
            // create a list of all dates to get the number of working days later on
            DateTime firstDateCalendar = new DateTime(2017, 5, 1);
            DateTime lastDateCalendar = DateTime.Today;
            var AlldatesCalendar = Enumerable.Range(0, lastDateCalendar.Subtract(firstDateCalendar).Days + 1)
                .Select(r => firstDateCalendar.AddDays(r));

            return AlldatesCalendar.Where(dt => dt >= FromDate && dt <= ToDate)
                .Where(dt => !(dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday))
                .Count();


        }

        public static int GetCurrentFinancialYear()
        {
            var ItIsJanuary = DateTime.Today.Month == 1;

            return ItIsJanuary ? DateTime.Today.Year - 1 : DateTime.Today.Year;

        }

    }
}