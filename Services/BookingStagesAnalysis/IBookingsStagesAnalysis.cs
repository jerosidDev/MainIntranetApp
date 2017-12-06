using Reporting_application.Repository.ThirdpartyDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting_application.ReportingModels
{
    public interface IBookingsStagesAnalysis
    {
        IEnumerable<BkgAnalysisInfo> BkgsSelectedInView2 { get; set; }
        IQueryable<ChartDisplayed> ListChartsCreated { get; set; }
        void UpdateChartsList(ViewDataDictionary ViewDataTransmitted);
        Task<Tuple<IEnumerable<object>, Dictionary<string, List<string>>>> AllTravellingsFrom2015Async();
    }
}