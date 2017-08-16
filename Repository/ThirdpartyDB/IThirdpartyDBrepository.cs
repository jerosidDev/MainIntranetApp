using Reporting_application.Models;
using Reporting_application.Repository.ERADB;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using System;
using System.Collections.Generic;

namespace Reporting_application.Repository
{
    public interface IThirdpartyDBrepository
    {
        IDictionary<string, string> GetRecentlyActiveConsultants();
        void GetAllConsultants();
        Dictionary<string, string> DictCsls { get; }
        Dictionary<string, string> DictSA1 { get; }


        List<BHDmini> AllBookingsFromFY17 { get; }
        void ExtractAllBookingsFromFY17();

        List<BHDmini> ExtractAllBookingsEnteredFY16();


        List<BHDmini> ExtractQuotingContractingEnquiries(CompanySpecifics cs);


        IEnumerable<BkgAnalysisInfo> TransformAllBookings(DateTime BeginningFY15, List<PendingEnquiryTableRow> deadlineTable, Dictionary<int, StagesDates> keyStagesDates);


        IEnumerable<ContractTBAsViewModel> ExtractTBAsViewModel();


        IThirdpartyDBContext tpContext3 { get; set; }


    }
}
