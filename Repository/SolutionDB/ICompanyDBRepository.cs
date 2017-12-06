using CompanyDbWebAPI.ModelsDB;
using Reporting_application.Repository.SolutionDB;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Reporting_application.Repository
{
    public interface ICompanyDBRepository
    {
        IEnumerable<BStage> listBStage { get; }

        void ExtractAllBStages();
        Task ExtractAllBStagesAsync();


        IEnumerable<IGrouping<int?, BStage>> GroupedBStagesEnquiries();

        //IEnumerable<StagesDates> RetrieveKeyStagesDates();
        Dictionary<int, StagesDates> RetrieveKeyStagesDates();
        Task<Dictionary<int, StagesDates>> RetrieveKeyStagesDatesAsync();

        IEnumerable<ContractConsultant> GetConsultantsTBAsLocations();
        Task<IEnumerable<ContractConsultant>> GetConsultantsTBAsLocationsAsync();

        HttpResponseMessage UpdateTBAinformationInDB(ContractConsultant cc, IEnumerable<ContractConsultant> CslTBAassignment);



    }
}