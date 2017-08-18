using Reporting_application.Utilities.GoogleCharts;
using System.Collections.Generic;

namespace Reporting_application.Services.Performance
{
    public interface IPerformance
    {
        List<PendingEnquiryTableRow> GenerateDeadlineTable();

    }
}
