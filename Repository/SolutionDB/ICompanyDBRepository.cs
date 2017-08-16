using ERAwebAPI.ModelsDB;
using Reporting_application.Repository.ERADB;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Reporting_application.Repository
{
    public interface ICompanyDBRepository
    {
        IEnumerable<BStage> listBStage { get; }

        void ExtractAllBStages();


        IEnumerable<IGrouping<int?, BStage>> GroupedBStagesEnquiriesEnteredAfter01052017();

        //IEnumerable<StagesDates> RetrieveKeyStagesDates();
        Dictionary<int, StagesDates> RetrieveKeyStagesDates();

        IEnumerable<ContractConsultant> GetConsultantsTBAsLocations();

        HttpResponseMessage UpdateTBAinformationInDB(ContractConsultant cc, IEnumerable<ContractConsultant> CslTBAassignment);



    }
}
