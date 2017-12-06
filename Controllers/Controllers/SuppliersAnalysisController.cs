using Reporting_application.Services.SuppliersAnalysis;
using Reporting_application.Utilities.Dates;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting_application.Controllers
{
    [Authorize]
    public class SuppliersAnalysisController : Controller
    {


        private static ISuppliersAnalysis suppAnalysis;



        public SuppliersAnalysisController(ISuppliersAnalysis _suppAnalysis)
        {

            if (suppAnalysis == null)
                suppAnalysis = _suppAnalysis;

        }


        // GET: SuppliersAnalysis
        public async Task<ActionResult> Index()
        {

            var currentFY = DatesUtilities.GetCurrentFinancialYear();


            // initial loading
            if (suppAnalysis.supplierAnalysisData == null)
                await suppAnalysis.InitialLoadingAsync(currentFY);





            ViewData["EvaluationType"] = suppAnalysis.GetEvalItemsForTheView();
            ViewData["FinancialYears"] = suppAnalysis.GetFYsForTheView();



            // definition of the filter narrowing items
            ViewData["FilterVal"] = suppAnalysis.GenerateFilterValuesForDropDowns(suppAnalysis.supplierAnalysisData);



            //  booking stage statuses , Viewdata because the format will be different
            ViewData["StagesStatuses"] = suppAnalysis.GetBookingStatusesForTheView();


            return View();
        }




        public JsonResult GetDropdownsValuesFromFilter()
        {
            // one filter at a time (done at the browser script level)

            // filter the data and json return them

            var sadListFiltered = suppAnalysis.FilterDataBasedOnViewSelection(Request.Form);


            var filteredDropdowns = suppAnalysis.GenerateFilterValuesForDropDowns(sadListFiltered);


            return Json(filteredDropdowns, JsonRequestBehavior.AllowGet);
        }




        public JsonResult CalculateAllDataBasedOnSelection()
        {
            suppAnalysis.EvaluateDataFromUserSelection(Request.Form);

            var dJSONed = suppAnalysis.DataToBeJSONed;



            return Json(dJSONed, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDataToBeExportedToExcel()
        {

            DateTime dateRequested = DateTime.Parse(Request.Form[0]).Date;

            var dataToExport = suppAnalysis.GetDataToBeExported(dateRequested);


            return Json(dataToExport, JsonRequestBehavior.AllowGet);
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
                MaxJsonLength = int.MaxValue
            };
        }


    }
}