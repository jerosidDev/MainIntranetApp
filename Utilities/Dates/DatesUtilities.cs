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

            //Func<DateTime, DateTime, int> nbWorkingDays = (fromDt, toDt) =>
            //{
            //    int nWD = AlldatesCalendar.Where(dt => dt >= fromDt && dt <= toDt)
            //    .Where(dt => !(dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday))
            //    .Count();
            //    return nWD;
            //};


            return AlldatesCalendar.Where(dt => dt >= FromDate && dt <= ToDate)
                .Where(dt => !(dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday))
                .Count();


        }

    }
}
