using ERAwebAPI.ModelsDB;
using Reporting_application.Repository.ERADB;
using Reporting_application.Utilities;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Reporting_application.Repository
{



    public class CompanyDBRepository : ICompanyDBRepository
    {
        private const string compDBapi = "http://111.111.11.11:11/api/";  // fictious url

        public IEnumerable<BStage> listBStage { get; protected set; }


        public virtual void ExtractAllBStages()
        {
            //      extract all the booking stages

            string API_Controller = "bstages";

            listBStage = APIuse.GetFromWebAPI<BStage>(compDBapi, API_Controller);

        }


        public IEnumerable<IGrouping<int?, BStage>> GroupedBStagesEnquiriesEnteredAfter01052017()
        {
            DateTime InitialDate = new DateTime(2017, 5, 1);

            if (listBStage == null) ExtractAllBStages();
            IList<int> listBStagesPrior = listBStage
                .Where(bs => bs.FromDate < InitialDate)
                .Select(bs => bs.BHD_ID ?? 0)
                .OrderBy(i => i)
                .Distinct().ToList();

            var groupedBStages = listBStage
                .Where(bs => !listBStagesPrior.Contains(bs.BHD_ID ?? 0))
                .GroupBy(bs => bs.BHD_ID)
                .OrderBy(g => g.Key);

            return groupedBStages;
        }


        public Dictionary<int, StagesDates> RetrieveKeyStagesDates()
        {
            //  For each booking entered after the 01/05/2017 , return the object { date sent , date confirmed , date cancelled}
            //      extract all the booking stages , for bookings entered after the 01/05/2017
            var GroupedBStages = GroupedBStagesEnquiriesEnteredAfter01052017();
            var cs = new CompanySpecifics();
            var AllStagesDates = GroupedBStages.Select(gp => new StagesDates(gp, cs));
            return AllStagesDates.ToDictionary(sd => sd.BHD_ID, sd => sd);
        }


        public IEnumerable<ContractConsultant> GetConsultantsTBAsLocations()
        {
            string API_Controller = "ContractConsultants";
            return APIuse.GetFromWebAPI<ContractConsultant>(compDBapi, API_Controller);

        }




        public HttpResponseMessage UpdateTBAinformationInDB(ContractConsultant cc, IEnumerable<ContractConsultant> CslTBAassignment)
        {
            var currentTBA = cc.LocationsAssigned.FirstOrDefault();
            HttpResponseMessage result = null;

            // create consultant if not in DB, ignore TBA creation or update 
            if (!CslTBAassignment.Any(c => c.INITIALS == cc.INITIALS))
            {
                cc.LocationsAssigned = null;
                result = APIuse.PostToWebAPI(compDBapi, "ContractConsultants", cc);
                if (!result.IsSuccessStatusCode) return result;
            }

            // create or update TBA assignment
            var existingTBAs = CslTBAassignment.SelectMany(c => c.LocationsAssigned);
            if (existingTBAs.Any(tba => tba.CODE == currentTBA.CODE))
                result = APIuse.PutToWebAPI(compDBapi, "TBALocations", currentTBA, currentTBA.CODE);
            else
                result = APIuse.PostToWebAPI(compDBapi, "TBALocations", currentTBA);

            return result;

        }



    }


}
