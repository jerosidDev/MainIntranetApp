using Reporting_application.Models;
using Reporting_application.Repository.SolutionDB;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using Reporting_application.Services.SuppliersAnalysis;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reporting_application.Repository
{
    public interface IThirdpartyDBrepository
    {
        IDictionary<string, string> GetRecentlyActiveConsultants();
        void GetAllConsultants();
        Task GetAllConsultantsAsync();
        Dictionary<string, string> DictCsls { get; }
        Dictionary<string, string> DictSA1 { get; }


        List<BHDmini> AllBHDminiFromFY17 { get; }


        List<BHDmini> ExtractAllBookingsEnteredFY16();


        List<BHDmini> ExtractQuotingContractingEnquiries(CompanySpecifics cs);


        IEnumerable<BkgAnalysisInfo> TransformAllBookings(DateTime BeginningFY15, List<PendingEnquiryTableRow> deadlineTable, Dictionary<int, StagesDates> keyStagesDates);


        IEnumerable<ContractTBAsInfo> ExtractTBAsList();
        Task<IEnumerable<ContractTBAsInfo>> ExtractTBAsListAsync();


        IThirdpartyDBContext tpContext3 { get; set; }



        Task ExtractAllBHDminiFromFY17Async();


        Task<IEnumerable<BHD>> GetBHDsFromDateEnteredAsync(DateTime from);
        Task<IEnumerable<DRM>> GetDRMsAsync();
        Task<IEnumerable<SA3>> GetSA3Async();
        Task<IEnumerable<CSL>> GetCSLAsync();
        Task<Dictionary<int, int>> GetPaxNumbersAsync();




        Task<List<SuppliersAnalysisData>> InitialLoadingSuppliersAnalysisAsync(int currentFY, List<string> validStatuses);




    }
}