using Reporting_application.ReportingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting_application.Controllers
{
    [Authorize]
    public class GroupsTravellingOverviewController : Controller
    {


        // TravelData grouped by week commencing
        static private Dictionary<string, List<object>> GroupedTravelData { get; set; }
        // Enquiries Entered grouped by week commencing
        static private Dictionary<string, List<object>> GroupedEnquiriesEntered { get; set; }
        // Enquiries finalised grouped by week commencing
        static private Dictionary<string, List<object>> GroupedEnquiriesFinalised { get; set; }
        // Missing second dates and series reference grouped by date entered
        static private Dictionary<string, List<object>> GroupedMissingInformation { get; set; }


        static private IBookingsStagesAnalysis BStagesAnalysis;


        public GroupsTravellingOverviewController(IBookingsStagesAnalysis _bStagesAnalysis)
        {
            if (BStagesAnalysis == null)
                BStagesAnalysis = _bStagesAnalysis;
        }




        [HttpGet]
        // GET: GroupsTravellingOverview
        public async Task<ActionResult> Index()
        {




            Tuple<IEnumerable<object>, Dictionary<string, List<string>>> tup = await BStagesAnalysis.AllTravellingsFrom2015Async();
            IEnumerable<object> TravelData = tup.Item1;
            Dictionary<string, List<string>> stages = tup.Item2;


            // fill ViewData for the options display , the value name is where  the filter will be applied and the text string is what will be visible on the page
            object objTest = TravelData.FirstOrDefault();
            Dictionary<string, string> SelectValueText = new Dictionary<string, string>();
            SelectValueText.Add("CompanyDpt", "CompanyDpt");
            SelectValueText.Add("BookingType", "BookingType");
            SelectValueText.Add("ConsultantCode", "Consultant");
            SelectValueText.Add("LocationName", "LocationName");
            SelectValueText.Add("AgentCode", "AgentName");

            // insert in ViewData
            foreach (KeyValuePair<string, string> kvp in SelectValueText)
            {
                string filterName = kvp.Key;
                string textName = kvp.Value;

                PropertyInfo pFilterName = objTest.GetType().GetProperty(filterName);
                PropertyInfo pTextName = objTest.GetType().GetProperty(textName);

                ViewData[filterName] = TravelData.OrderBy(o => pTextName.GetValue(o)).Select(o => new { fName = pFilterName.GetValue(o), tName = pTextName.GetValue(o) }).Distinct();

            }



            // add the stages (unconfirmed , cancelled , ...) to the view
            ViewData["Stages"] = stages;




            // Grouping by Travel dates, date entered , date Finalised:
            //  14/01/2017 : issue with blank weeks
            // group the data week by week commencing , making sure that there is no gap in weeks
            //PropertyInfo PropWC = objTest.GetType().GetProperty("DatetimeWeekCommencing");
            Dictionary<string, Dictionary<string, List<object>>> GroupedItems = new Dictionary<string, Dictionary<string, List<object>>>();   // the dictionary key is only a property name
            GroupedItems.Add("DatetimeWeekCommencing", GroupedTravelData = new Dictionary<string, List<object>>());
            GroupedItems.Add("DatetimeWeekCommencingEnt", GroupedEnquiriesEntered = new Dictionary<string, List<object>>());
            GroupedItems.Add("DatetimeWeekCommencingFin", GroupedEnquiriesFinalised = new Dictionary<string, List<object>>());


            PropertyInfo pWC = objTest.GetType().GetProperty(GroupedItems.FirstOrDefault().Key);
            DateTime FirstWeek = (DateTime)TravelData.Select(o => pWC.GetValue(o)).Min();
            DateTime LastWeek = (DateTime)TravelData.Select(o => pWC.GetValue(o)).Max();

            // create the continuous line of xvalues
            for (DateTime wc = FirstWeek; wc <= LastWeek; wc = wc.AddDays(7))
            {
                string wcString = wc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                foreach (KeyValuePair<string, Dictionary<string, List<object>>> kvp in GroupedItems)
                {
                    Dictionary<string, List<object>> gi = kvp.Value;
                    gi.Add(wcString, new List<object>());
                }
            }

            // populate week by week for each type of grouped items
            foreach (object o in TravelData)
            {
                foreach (KeyValuePair<string, Dictionary<string, List<object>>> kvp in GroupedItems)
                {
                    PropertyInfo pWC2 = objTest.GetType().GetProperty(kvp.Key);
                    DateTime wc = (DateTime)pWC2.GetValue(o);
                    string wcString = wc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    Dictionary<string, List<object>> gi = kvp.Value;

                    // exception for finalised date not to include o in GroupedEnquiriesFinalised if o is not finalised
                    PropertyInfo pFinalStage = objTest.GetType().GetProperty("FinalStage");
                    string finStage = pFinalStage.GetValue(o).ToString();
                    if (gi.ContainsKey(wcString) && !(kvp.Key == "DatetimeWeekCommencingFin" && finStage == "None")) gi[wcString].Add(o);
                }
            }


            //  missing information enquiries
            //      the missing information bookings are based on DateMissing == true and SeriesReferenceMissing == true
            PropertyInfo dateMissing = objTest.GetType().GetProperty("DateMissing");
            PropertyInfo seriesMissing = objTest.GetType().GetProperty("SeriesReferenceMissing");
            GroupedMissingInformation = GroupedTravelData.ToDictionary(g => g.Key, g =>
            {
                List<object> objList = g.Value.Where(o =>
                {

                    string _dm = dateMissing.GetValue(o).ToString();
                    string _sm = seriesMissing.GetValue(o).ToString();

                    return _dm == "Yes" || _sm == "Yes";
                }).ToList();
                return objList;
            });

            return View();

        }


        public JsonResult ReturnJSONTravelData()
        {

            // this method should be called from the js using AJAX

            return Json(GroupedTravelData, JsonRequestBehavior.AllowGet);

        }


        public JsonResult ReturnJSONEnquiriesEntered()
        {
            // this method should be called from the js using AJAX

            return Json(GroupedEnquiriesEntered, JsonRequestBehavior.AllowGet);

        }


        public JsonResult ReturnJSONEnquiriesFinalised()
        {
            // this method should be called from the js using AJAX

            return Json(GroupedEnquiriesFinalised, JsonRequestBehavior.AllowGet);

        }


        public JsonResult ReturnJSONMissingInformation()
        {
            // this method should be called from the js using AJAX

            return Json(GroupedMissingInformation, JsonRequestBehavior.AllowGet);

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