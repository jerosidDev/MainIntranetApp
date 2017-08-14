using ERAwebAPI.ModelsDB;
using Reporting_application.Repository;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.GoogleCharts;
using Reporting_application.Utilities.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Index()
        {

            perfRepo.ExtractTBAsItems();

            perfRepo.GeneratePerformanceSplits();
            perfRepo.RetrieveDptsAndConsultantFromFY17();
            perfRepo.RetrieveDptCslRelated();

            // to fill the select on the view
            ViewData["dptList"] = perfRepo.dptList;

            Dictionary<string, string> dictLocations = perfRepo.itemsVM
                .ToLookup(vm => vm.LocCode, vm => vm.LocName)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());
            ViewData["dictLocations"] = dictLocations;

            ViewData["DictCsls"] = perfRepo.tpRepo.DictCsls;


            var dictCslLocsCode = perfRepo.CslTBAassignment.ToDictionary(csl => csl.INITIALS, csl => csl.LocationsAssigned.Select(l => l.CODE));
            var LocCsl = dictLocations.ToDictionary(kvpLoc => kvpLoc.Key, kvpLoc =>
            {
                string cslFound = dictCslLocsCode.FirstOrDefault(kvp => kvp.Value.Contains(kvpLoc.Key)).Key;
                return cslFound;
            });
            ViewData["LocCsl"] = LocCsl;


            return View();

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

            List<PendingEnquiryTableRow> listRowsPending = perfRepo.GenerateDeadlineTable();
            return Json(listRowsPending.OrderBy(r => r.Days_Before_Deadline), JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult PostTBAsInformation(ContractConsultant cc)
        {

            perfRepo.ApplyTBAassignmentChange(cc);

            return null;
        }


        // the method is overriden to allow for bigger amount of data to be sent : MaxJsonLength = Int32.MaxValue
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
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
