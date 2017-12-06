using CompanyDbWebAPI.ModelsDB;
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
            //// return a list of links to calendar based on the users rights

            //List<string> AdminRightList = new List<string>();
            //AdminRightList.Add("Administrator");
            //AdminRightList.Add("Noemie.Lambermont");
            ////AdminRightList.Add("marketing");
            //AdminRightList.Add("Emma.Treu");


            //List<string> AccountManagerList = new List<string>();
            //AccountManagerList.Add("Frederic.Kiesel");
            //AccountManagerList.Add("Caroline.Burnier");
            //AccountManagerList.Add("Millie.Pitton");
            //AccountManagerList.Add("Mariangela.Felton");
            //AccountManagerList.Add("Susanne.Keller");
            //AccountManagerList.Add("Gael.Philippe");
            //AccountManagerList.Add("Sandrine.VrayDumas");
            //AccountManagerList.Add("jerome.siddiqi");
            //AccountManagerList.Add("Sophie.Toulou");


            Dictionary<string, Dictionary<string, string>> CalendarLinks = new Dictionary<string, Dictionary<string, string>>();



            string userName = User.Identity.Name.Replace("E-VOYAGES\\", "");
            // Trade Fair calendar
            //      readonly: everyone
            //      modify : AdminRightList+AccountManagerList
            //      administer : AdminRightList
            Dictionary<string, string> TFC = new Dictionary<string, string>();
            TFC.Add("View calendar", "https://teamup.com/ks9c2a6ee80b7fd609");
            //if (AccountManagerList.Contains(userName, StringComparer.CurrentCultureIgnoreCase) || AdminRightList.Contains(userName,StringComparer.CurrentCultureIgnoreCase))
            //{
            //    TFC.Add("Modify calendar", "https://teamup.com/ks952171fdffd03de1");
            //}
            //if (AdminRightList.Contains(userName, StringComparer.CurrentCultureIgnoreCase))
            //{
            //    TFC.Add("Administer calendar", "https://teamup.com/ks4133c352cdf9156c");
            //}

            CalendarLinks.Add("Trade Fair calendar", TFC);


            // Sales calendar
            //      readonly: everyone
            //      modify : AdminRightList+AccountManagerList

            Dictionary<string, string> SCC = new Dictionary<string, string>();
            SCC.Add("View calendar", "https://teamup.com/ks38e12333e2930bb9");
            //if (AccountManagerList.Contains(userName, StringComparer.CurrentCultureIgnoreCase) || AdminRightList.Contains(userName, StringComparer.CurrentCultureIgnoreCase))
            //{
            //    SCC.Add("Modify calendar", "https://teamup.com/ks69cfe3163f138e4f");
            //}
            CalendarLinks.Add("Sales events calendar", SCC);




            // Marketing calendar
            //      readonly: everyone
            //      modify : AdminRightList
            //      administer : AdminRightList
            Dictionary<string, string> MC = new Dictionary<string, string>();
            MC.Add("View calendar", "https://teamup.com/ksde0f72f2518db3ec");
            //if (AdminRightList.Contains(userName, StringComparer.CurrentCultureIgnoreCase))
            //{
            //    MC.Add("Modify calendar", "https://teamup.com/ks897acf28a7ccee90");
            //    MC.Add("Administer calendar", "https://teamup.com/ksbffe97fd0f481038");
            //}
            CalendarLinks.Add("Marketing calendar", MC);




            return CalendarLinks;

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}