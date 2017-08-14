using ERAwebAPI.ModelsDB;
using Reporting_application.Utilities;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Reporting_application.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {


            //      Display the Log_Update table on the index view
            //      this table should only be visible by myself and JM
            IEnumerable<Log_Update> listUpdates = APIuse.Extract_Log_Update(User.Identity.Name);
            if (listUpdates != null) ViewData["listUpdates"] = listUpdates;


            Dictionary<string, Dictionary<string, string>> CalendarLinksModel = ReturnCalendarLinksModel();

            return View(CalendarLinksModel);
        }

        private Dictionary<string, Dictionary<string, string>> ReturnCalendarLinksModel()
        {


            Dictionary<string, Dictionary<string, string>> CalendarLinks = new Dictionary<string, Dictionary<string, string>>();


            Dictionary<string, string> TFC = new Dictionary<string, string>();
            TFC.Add("View calendar", urlToCalendar1);


            CalendarLinks.Add("Trade Fair calendar", TFC);


            // Sales calendar
            
            Dictionary<string, string> SCC = new Dictionary<string, string>();
            SCC.Add("View calendar", urlToCalendar2);
            CalendarLinks.Add("Sales events calendar", SCC);


            // Marketing calendar
             Dictionary<string, string> MC = new Dictionary<string, string>();
            MC.Add("View calendar", urlToCalendar3);
            CalendarLinks.Add("Marketing calendar", MC);

            return CalendarLinks;

        }

        public ActionResult About()
        {
            ViewBag.Message = "Intranet App";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "contacts";

            return View();
        }
    }
}
