using CompanyDbWebAPI.ModelsDB;
using Reporting_application.Repository;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using Reporting_application.Utilities.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting_application.Controllers
{
    [Authorize]
    public class PerformanceOverviewController : Controller
    {

        public static Performance perfRepo;

        public PerformanceOverviewController(ICompanyDBRepository _compDbRepo, IThirdpartyDBrepository _tpRepo)
        {
            perfRepo = perfRepo ?? new Performance(_compDbRepo, _tpRepo);
        }


        // GET: PerformanceOverview
        //public ActionResult Index()
        //{

        //    perfRepo.ExtractTBAsItems();
        //    perfRepo.GeneratePerformanceSplits();
        //    //perfRepo.RetrieveDptsAndConsultantFromFY17();
        //    perfRepo.RetrieveDptCslRelated();

        //    // to fill the dropdowns on the view

        //    ViewData["dptList"] = perfRepo.dptList;

        //    Dictionary<string, string> dictLocations = GetDictLocations();
        //    ViewData["dictLocations"] = dictLocations;

        //    ViewData["DictCsls"] = perfRepo.tpRepo.DictCsls;

        //    ViewData["LocCsl"] = GetLocCsl(dictLocations);


        //    //      display the contract consultants:
        //    ViewData["listContractCsl"] = GetDictContractCsl();



        //    return View();

        //}



        public async Task<ActionResult> Index()
        {

            // set parallel tasks
            var GeneratePerformanceSplitsTask = perfRepo.GeneratePerformanceSplitsAsync();
            await GeneratePerformanceSplitsTask;

            perfRepo.RetrieveDptCslRelated();




            // to fill the dropdowns on the view

            ViewData["dptList"] = perfRepo.dptList;


            //if (perfRepo.tpRepo.DictCsls == null) await perfRepo.tpRepo.GetAllConsultantsAsync();
            ViewData["DictCsls"] = perfRepo.tpRepo.DictCsls;


            var ExtractTBAsItemsTask = perfRepo.ExtractTBAsItemsAsync();
            await ExtractTBAsItemsTask;

            Dictionary<string, string> dictLocations = GetDictLocations();
            ViewData["dictLocations"] = dictLocations;


            ViewData["LocCsl"] = GetLocCsl(dictLocations);


            //      display the contract consultants:
            ViewData["listContractCsl"] = await GetDictContractCslAsync();



            return View("Index");

        }




        private static async Task<Dictionary<string, string>> GetDictContractCslAsync()
        {

            // perfRepo.dictActiveConsult should be replaced by the list of active contract colleagues

            //var dict = perfRepo.dictAllPerformance["ContractEnquiries"] as Dictionary<string, Dictionary<string, Dictionary<string, List<SentEnquiry>>>>;
            //var listContractCsl = dict.ToDictionary(kvp => kvp.Key,
            //    kvp => kvp.Value.Select(kvp2 => kvp2.Key))
            //    .SelectMany(kvp3 => kvp3.Value)
            //    .Distinct()
            //    .Where(csl => perfRepo.dictActiveConsult.ContainsKey(csl)) // active consultants only
            //    .ToList();
            //var dictListContractCsl = listContractCsl.OrderBy(csl => csl)
            //    .ToDictionary(csl => csl, csl => perfRepo.tpRepo.DictCsls[csl]);
            //return dictListContractCsl;


            // extract all the booking stages after today -3 months
            DateTime beginDt = DateTime.Today.AddMonths(-3);
            var listContractCslFromBStages = perfRepo.compDbRepo.listBStage
                .Where(bs => bs.FromDate >= beginDt)
                .Where(bs =>
                perfRepo.CompSpec.BookingStageCodes[bookingStage.pendingContract].Contains(bs.Status.Trim()))
                .Select(bs => bs.Consultant.Trim())
                .Distinct();

            var dict = perfRepo.dictAllPerformance["ContractEnquiries"] as Dictionary<string, Dictionary<string, Dictionary<string, List<SentEnquiry>>>>;
            var listContractCsl = dict.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Select(kvp2 => kvp2.Key))
                .SelectMany(kvp3 => kvp3.Value)
                .Distinct()
                .Where(csl => listContractCslFromBStages.Contains(csl)) // active contract consultants only
                .ToList();
            //if (perfRepo.tpRepo.DictCsls == null) await perfRepo.tpRepo.GetAllConsultantsAsync();
            var dictListContractCsl = listContractCsl.OrderBy(csl => csl)
                .ToDictionary(csl => csl, csl => perfRepo.tpRepo.DictCsls[csl]);
            return dictListContractCsl;





        }

        //private static async Task<Dictionary<string, string>> GetDictContractCslAsync()
        //{

        //    // perfRepo.dictActiveConsult should be replaced by the list of active contract colleagues

        //    //var dict = perfRepo.dictAllPerformance["ContractEnquiries"] as Dictionary<string, Dictionary<string, Dictionary<string, List<SentEnquiry>>>>;
        //    //var listContractCsl = dict.ToDictionary(kvp => kvp.Key,
        //    //    kvp => kvp.Value.Select(kvp2 => kvp2.Key))
        //    //    .SelectMany(kvp3 => kvp3.Value)
        //    //    .Distinct()
        //    //    .Where(csl => perfRepo.dictActiveConsult.ContainsKey(csl)) // active consultants only
        //    //    .ToList();
        //    //var dictListContractCsl = listContractCsl.OrderBy(csl => csl)
        //    //    .ToDictionary(csl => csl, csl => perfRepo.tpRepo.DictCsls[csl]);
        //    //return dictListContractCsl;


        //    // extract all the booking stages after today -3 months
        //    DateTime beginDt = DateTime.Today.AddMonths(-3);
        //    var listContractCslFromBStages = perfRepo.compDbRepo.listBStage
        //        .Where(bs => bs.FromDate >= beginDt)
        //        .Where(bs =>
        //        perfRepo.CompSpec.BookingStageCodes[bookingStage.pendingContract].Contains(bs.Status.Trim()))
        //        .Select(bs => bs.Consultant.Trim())
        //        .Distinct();

        //    var dict = perfRepo.dictAllPerformance["ContractEnquiries"] as Dictionary<string, Dictionary<string, Dictionary<string, List<SentEnquiry>>>>;
        //    var listContractCsl = dict.ToDictionary(kvp => kvp.Key,
        //        kvp => kvp.Value.Select(kvp2 => kvp2.Key))
        //        .SelectMany(kvp3 => kvp3.Value)
        //        .Distinct()
        //        .Where(csl => listContractCslFromBStages.Contains(csl)) // active contract consultants only
        //        .ToList();
        //    var dictListContractCsl = listContractCsl.OrderBy(csl => csl)
        //        .ToDictionary(csl => csl, csl => perfRepo.tpRepo.DictCsls[csl]);
        //    return dictListContractCsl;





        //}



        private static Dictionary<string, string> GetLocCsl(Dictionary<string, string> dictLocations)
        {
            var dictCslLocsCode = perfRepo.CslTBAassignment.ToDictionary(csl => csl.INITIALS, csl => csl.LocationsAssigned.Select(l => l.CODE));
            var LocCsl = dictLocations.ToDictionary(kvpLoc => kvpLoc.Key, kvpLoc =>
            {
                string cslFound = dictCslLocsCode.FirstOrDefault(kvp => kvp.Value.Contains(kvpLoc.Key)).Key;
                return cslFound;
            });
            return LocCsl;
        }

        private static Dictionary<string, string> GetDictLocations()
        {
            return perfRepo.itemsVM
                            .ToLookup(vm => vm.LocCode, vm => vm.Location_Name)
                            .OrderBy(g => g.Key)
                            .ToDictionary(g => g.Key, g => g.FirstOrDefault());
        }

        public JsonResult JsonEnquiriesSent()
        {
            // send a list of enquiries sent to be used for calculation in the google charts
            //      tested with: EnquiriesSentTest

            IList<SentEnquiry> listSentEnquiries = perfRepo.CalculateSentEnquiries();

            return Json(listSentEnquiries, JsonRequestBehavior.AllowGet);


        }






        public JsonResult JsonPerformanceItems()
        {
            var returned = Json(perfRepo.dictAllPerformance, JsonRequestBehavior.AllowGet);
            return returned;
        }


        public JsonResult RetrieveAllCslAssociatedToAllDpt()
        {

            // retrieve only the active consultants associated with each department
            var dict = perfRepo.dptCslAssociation
                .ToDictionary(d => d.Key, d =>
                {
                    // remove the consultants which are not active:
                    IEnumerable<string> listCsl = d.Value
                .Where(csl => perfRepo.dictActiveConsult.ContainsKey(csl));
                    var t = listCsl.ToDictionary(csl => csl, csl => perfRepo.dictActiveConsult[csl]);
                    return t;
                });


            return Json(dict, JsonRequestBehavior.AllowGet);

        }



        public JsonResult RetrieveUnsentEnquiries()
        {


            //if (perfRepo.tpRepo.DictCsls == null) await perfRepo.tpRepo.GetAllConsultantsAsync();


            List<PendingEnquiryTableRow> listRowsPending = perfRepo.GenerateDeadlineTable();
            return Json(listRowsPending.OrderBy(r => r.Days_Before_Deadline), JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult PostTBAsInformation(ContractConsultant cc)
        {

            perfRepo.ApplyTBAassignmentChange(cc);

            return null;
        }


        public JsonResult RetrieveTBAsInfo()
        {
            var ret = perfRepo.itemsVM;

            var dictSent = ret
                .ToLookup(tba => tba._contractConsultant.NAME).OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.OrderBy(info => DateTime.Parse(info.Service_Date)));

            return Json(dictSent, JsonRequestBehavior.AllowGet);


            //return Json(ret, JsonRequestBehavior.AllowGet);
        }



        // the method is overriden to allow for bigger amount of data to be sent : MaxJsonLength = Int32.MaxValue
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {

            //var jsonMediaTypeFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //jsonMediaTypeFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            //jsonMediaTypeFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;


            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }


    }




}