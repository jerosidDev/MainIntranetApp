using Reporting_application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Reporting_application.Controllers
{
    [Authorize]
    public class FollowUpBkgsController : Controller
    {


        static private IEnumerable<BkgsFollowUp> AllFollowUpBookings;


        // GET: FollowUpBkgs
        public ActionResult Index()
        {

            // extract all the pending bookings that fit the criterias
            AllFollowUpBookings = ExtractAllFollowUp();

            return View();
        }


        public JsonResult FollowUpTableData(string FollowUpCategory)
        {

            // 3 types of FollowUpCategory
            //      "PastDeadline" : follow up date is less than today
            //      "ToBeDoneToday" : follow up date is today and until the next 2 following days if today is Friday
            //      "ToBeDoneFuture" : follow up date is the future (not including today or the next 2 days if today is friday)



            DateTime beginDate = AllFollowUpBookings.FirstOrDefault().FollowUpdate;
            DateTime endDate = AllFollowUpBookings.LastOrDefault().FollowUpdate;
            switch (FollowUpCategory)
            {
                case "PastDeadline":
                    endDate = DateTime.Today.AddDays(-1);
                    break;
                case "ToBeDoneToday":
                    beginDate = DateTime.Today;
                    endDate = DateTime.Today.DayOfWeek == DayOfWeek.Friday ? DateTime.Today.AddDays(2) : DateTime.Today;
                    break;
                case "ToBeDoneWithin7Days":
                    beginDate = DateTime.Today.DayOfWeek == DayOfWeek.Friday ? DateTime.Today.AddDays(3) : DateTime.Today.AddDays(1);
                    endDate = DateTime.Today.AddDays(8);
                    break;
                default: // all of the bookings
                    break;
            }


            IEnumerable<BkgsFollowUp> BkgsToBeSent = AllFollowUpBookings.Where(b => b.FollowUpdate >= beginDate && b.FollowUpdate <= endDate);



            // to JSON transformation

            System.Reflection.PropertyInfo[] propInfo = typeof(BkgsFollowUp).GetProperties();

            // create a copy of the object BkgsFollowUp with properties for dates being strings in format : yyyy-mm-dd 
            Func<BkgsFollowUp, object> ChangeDateToISOstring = bk =>
            {
                Dictionary<string, object> BkDict = new Dictionary<string, object>();
                foreach (var p in propInfo)
                {
                    // if type of p is datetime return a value which is in format "yyyy-mm-dd"
                    if (p.PropertyType.Name == "DateTime")
                    {
                        DateTime d = (DateTime)p.GetValue(bk);
                        string s = d.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        BkDict.Add(p.Name, s);
                    }
                    else
                    {
                        BkDict.Add(p.Name, p.GetValue(bk));
                    }
                }
                return BkDict;
            };


            IEnumerable<object> JSONdata = BkgsToBeSent.Select(bk => ChangeDateToISOstring(bk));


            // BkgsFollowUp class :the first object of the JSON should be an object containing the type of the properties and their names
            Dictionary<string, string> ClassDescr = new Dictionary<string, string>();
            foreach (var p in propInfo)
            {
                ClassDescr.Add(p.Name, p.PropertyType.Name);
            }



            List<object> JSONdataToBeSent = new List<object>();
            JSONdataToBeSent.Add(ClassDescr);
            foreach (object b in JSONdata)
            {
                JSONdataToBeSent.Add(b);
            }


            return Json(JSONdataToBeSent, JsonRequestBehavior.AllowGet);
        }



        private IEnumerable<BkgsFollowUp> ExtractAllFollowUp()
        {
            // extract all the bookings that fit the criterias
            // attention there can be bookings with multiple service line being "follow up"



            ThirdpartyDBContext te = new ThirdpartyDBContext();

            //ReportingModel rm = new ReportingModel(te,"FollowUp");


            // find the option_ID related to the "OPTION" service line
            int Opt_ID = te.OPTs.Where(o => o.CODE.Trim() == "OPTION").Select(o => o.OPT_ID).FirstOrDefault();

            // find all the service lines being "OPTION"
            IEnumerable<BSL> BSLSelected = te.BSLs.Where(bs => bs.OPT_ID == Opt_ID);



            //  method using dictionaries


            Dictionary<string, string> _SA1 = te.SA1.ToDictionary(s => s.CODE, s => s.DESCRIPTION);
            Dictionary<string, string> _CSL = te.CSLs.ToDictionary(c => c.INITIALS, c => c.NAME);


            //      creation of a dictionary for BHDs

            //          filter by Date after the "01/02/2015" and booking status belonging to { "PE" , "Q" , "SO" } 
            DateTime BeginDate = new DateTime(2015, 2, 1);
            List<string> bStatus = new List<string>() { "PE", "Q", "SO" };
            IEnumerable<BHD> _BHDfiltered = te.BHDs.Where(b => b.DATE_ENTERED >= BeginDate && (bStatus.Contains(b.STATUS.Trim())));
            //IEnumerable<BHD> _BHDfiltered = te.BHDs.Where(b => b.DATE_ENTERED >= BeginDate);

            Dictionary<int, BHD> _BHDdict = _BHDfiltered.ToDictionary(b => b.BHD_ID, b => b);



            // for each BSL in BSLSelected create the obj BkgsFollowUp
            Func<BSL, BkgsFollowUp> ProjectToBkgsFollowUp = _bsl =>
            {

                // incase of BHD is not in the dictionary
                if (!_BHDdict.ContainsKey(_bsl.BHD_ID)) return null;

                BkgsFollowUp bk = new BkgsFollowUp();


                bk.AgentCode = _BHDdict[_bsl.BHD_ID].AGENT;
                bk.BDConsultant = _SA1[_BHDdict[_bsl.BHD_ID].SALE1];
                bk.BookingName = _BHDdict[_bsl.BHD_ID].NAME;
                bk.BookingRef = _BHDdict[_bsl.BHD_ID].FULL_REFERENCE;
                bk.BookingStatus = _BHDdict[_bsl.BHD_ID].STATUS;
                bk.BookingType = _BHDdict[_bsl.BHD_ID].SALE6;
                bk.Booking_Date_Entered = _BHDdict[_bsl.BHD_ID].DATE_ENTERED;
                bk.Consultant = _CSL[_BHDdict[_bsl.BHD_ID].CONSULTANT];
                bk.Estimated_Turnover = _BHDdict[_bsl.BHD_ID].UDTEXT3;
                bk.FollowUpdate = _bsl.DATE;

                bk.TravelDate = _BHDdict[_bsl.BHD_ID].TRAVELDATE;

                return bk;
            };


            IEnumerable<BkgsFollowUp> _BksSelected = BSLSelected.Select(_bsl => ProjectToBkgsFollowUp(_bsl)).ToList();
            // remove the null bkgs
            _BksSelected = _BksSelected.Where(b => b != null).OrderBy(b => b.FollowUpdate);


            return _BksSelected;

        }
    }


    public class BkgsFollowUp
    {
        // class defined by the minimal elements needed for the displaying of the information


        //public string OptionCode { get; set; }
        //public string OptionDescr { get; set; }
        public string BookingRef { get; set; }
        public string BookingName { get; set; }
        public DateTime FollowUpdate { get; set; }
        public string BDConsultant { get; set; }
        public string Consultant { get; set; }
        public string BookingStatus { get; set; }
        public string BookingType { get; set; }
        public string AgentCode { get; set; }
        public DateTime Booking_Date_Entered { get; set; }

        public DateTime TravelDate { get; set; }

        public string Estimated_Turnover { get; set; }



    }

}
