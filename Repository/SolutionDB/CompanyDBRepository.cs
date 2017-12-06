using CompanyDbWebAPI.ModelsDB;
using Reporting_application.Repository.SolutionDB;
using Reporting_application.Utilities;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Reporting_application.Repository
{



    public class CompanyDBRepository : ICompanyDBRepository
    {
        private const string compDBapi = "http://192.168.75.27:83/api/";

        public IEnumerable<BStage> listBStage { get; protected set; }


        public virtual void ExtractAllBStages()
        {
            //      extract all the booking stages

            string API_Controller = "bstages";

            listBStage = APIuse.GetFromWebAPI<BStage>(compDBapi, API_Controller);

        }


        public virtual async Task ExtractAllBStagesAsync()
        {
            //      extract all the booking stages

            string API_Controller = "bstages";

            listBStage = await APIuse.GetFromWebAPIAsync<BStage>(compDBapi, API_Controller);

        }



        public IEnumerable<IGrouping<int?, BStage>> GroupedBStagesEnquiries()
        {
            // method is modified to include all booking stages including the one for enquiries entered before the 01/05/2017

            DateTime InitialDate = new DateTime(2015, 2, 1);

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



        public async Task<IEnumerable<IGrouping<int?, BStage>>> GroupedBStagesEnquiriesAsync()
        {
            // method is modified to include all booking stages including the one for enquiries entered before the 01/05/2017

            DateTime InitialDate = new DateTime(2015, 2, 1);

            if (listBStage == null)
                await ExtractAllBStagesAsync();
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
            var GroupedBStages = GroupedBStagesEnquiries();
            var cs = new CompanySpecifics();
            var AllStagesDates = GroupedBStages.Select(gp => new StagesDates(gp, cs));
            return AllStagesDates.ToDictionary(sd => sd.BHD_ID, sd => sd);
        }




        public async Task<Dictionary<int, StagesDates>> RetrieveKeyStagesDatesAsync()
        {
            //  For each booking entered after the 01/05/2017 , return the object { date sent , date confirmed , date cancelled}
            //      extract all the booking stages , for bookings entered after the 01/05/2017
            var GroupedBStages = await GroupedBStagesEnquiriesAsync();
            var cs = new CompanySpecifics();
            var AllStagesDates = GroupedBStages.Select(gp => new StagesDates(gp, cs));
            return AllStagesDates.ToDictionary(sd => sd.BHD_ID, sd => sd);
        }


        public IEnumerable<ContractConsultant> GetConsultantsTBAsLocations()
        {
            string API_Controller = "ContractConsultants";
            return APIuse.GetFromWebAPI<ContractConsultant>(compDBapi, API_Controller);

        }


        public async Task<IEnumerable<ContractConsultant>> GetConsultantsTBAsLocationsAsync()
        {
            string API_Controller = "ContractConsultants";
            return await APIuse.GetFromWebAPIAsync<ContractConsultant>(compDBapi, API_Controller);

        }




        public HttpResponseMessage UpdateTBAinformationInDB(ContractConsultant cc, IEnumerable<ContractConsultant> CslTBAassignment)
        {
            var currentTBA = cc.LocationsAssigned.FirstOrDefault();
            HttpResponseMessage result = null;

            //  17/08/2017 : added the case to unassign a TBA : cc.INITIALS == null


            // new way: post then if failed put
            //      5 cases:
            //          new csl , new TBA
            //          new csl , exist TBA
            //          exist csl , new TBA
            //          exist csl , exist TBA
            //          csl null , exist TBA (unassignment)



            // create consultant if not in DB, ignore TBA creation or update or unassigned
            if (cc.INITIALS != null)
            {
                cc.LocationsAssigned = null;
                APIuse.PostToWebAPI(compDBapi, "ContractConsultants", cc);
            }

            // create or update TBA assignment
            result = APIuse.PostToWebAPI(compDBapi, "TBALocations", currentTBA);
            if (!result.IsSuccessStatusCode)
                result = APIuse.PutToWebAPI(compDBapi, "TBALocations", currentTBA, currentTBA.CODE);


            return result;
        }



    }


}