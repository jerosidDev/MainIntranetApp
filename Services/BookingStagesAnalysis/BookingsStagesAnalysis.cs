using Reporting_application.Models;
using Reporting_application.Repository;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reporting_application.ReportingModels
{
    public class BookingsStagesAnalysis : IBookingsStagesAnalysis
    {

        //public ThirdpartyDBContext db;


        // List of Charts of types ChartDisplayed associated with the current view
        public IQueryable<ChartDisplayed> ListChartsCreated { get; set; }


        // all bookings since feb 2015
        public IEnumerable<BkgAnalysisInfo> AllBookingsForAnalysis { get; set; }




        // format used when converting strings to datetime
        public IFormatProvider culture { get; set; } = new System.Globalization.CultureInfo("fr-FR", true);


        //  bookings selected by the user in the index view parameters selection
        public IEnumerable<BkgAnalysisInfo> BkgsSelectedInView { get; private set; }


        // full booking list associated with the timeframe , department and booking type selected in order to get exported
        public IEnumerable<BkgAnalysisInfo> BkgsSelectedInView2 { get; set; }


        private DateTime BeginningFY15 { get; set; } = DateTime.Parse("01/02/2015", new System.Globalization.CultureInfo("fr-FR", true));





        public CompanySpecifics compSpec { get; set; } = new CompanySpecifics();


        private ICompanyDBRepository compDbRepo { get; set; }
        private IThirdpartyDBrepository tpRepo { get; set; }

        private IPerformance perfRepo { get; set; }


        public BookingsStagesAnalysis(ICompanyDBRepository _compDbRepo, IThirdpartyDBrepository _tpRepo, IPerformance _perfRepo)
        {


            compDbRepo = _compDbRepo;
            tpRepo = _tpRepo;
            perfRepo = _perfRepo;



            // used by the select in the view
            compSpec.DptsList["All"] = compSpec.DptsList.SelectMany(kvp => kvp.Value).ToList();
            compSpec.bookingTypes["All"] = compSpec.bookingTypes.SelectMany(kvp => kvp.Value).ToList();

        }



        public void UpdateChartsList(ViewDataDictionary ViewDataTransmitted)
        {

            // this should be a reflection of the Index view

            // method called from the controller before initialisation of the Index view or on form submit inside Index view
            // charts are all created before the call to the view

            ListChartsCreated = null;



            int ChartId = 1;



            DateTime bTF = DateTime.Parse(ViewDataTransmitted["BeginningTF"].ToString(), culture);
            DateTime eTF = DateTime.Parse(ViewDataTransmitted["EndTF"].ToString(), culture);
            string ds = ViewDataTransmitted["DeptSelected"].ToString();
            string bTS = ViewDataTransmitted["bkgTypeSelected"].ToString();




            // the bookings are only selected after  the "01/02/2015"
            var deadlineTable = perfRepo.GenerateDeadlineTable();
            var keyStagesDates = compDbRepo.RetrieveKeyStagesDates();
            AllBookingsForAnalysis = AllBookingsForAnalysis ?? tpRepo.TransformAllBookings(BeginningFY15, deadlineTable, keyStagesDates);


            // list of all precalculated charts that  will be displayed on the view
            IList<ChartDisplayed> lCD = new List<ChartDisplayed>();


            // Year on year analysis charts
            ChartParam cpYOY1 = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = analysisType.YearOnYear, bs = bookingStage.received, vd = valueDisplayed.bookingsAmount };
            ChartDisplayed cdYOY1 = new ChartDisplayed(cpYOY1, this, ReportType: "YearOnYear");
            cdYOY1.ChartId = ChartId;
            ChartId++;
            lCD.Add(cdYOY1);

            ChartParam cpYOY2 = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = analysisType.YearOnYearCumulative, bs = bookingStage.received, vd = valueDisplayed.bookingsAmount };
            ChartDisplayed cdYOY2 = new ChartDisplayed(cpYOY2, this, ReportType: "YearOnYear");
            cdYOY2.ChartId = ChartId;
            ChartId++;
            lCD.Add(cdYOY2);





            // Departmental and clients overview

            for (int iAT = 0; iAT < 2; iAT++)
            {

                ChartParam cp = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = (analysisType)iAT, bs = bookingStage.received, vd = valueDisplayed.bookingsAmount };
                ChartDisplayed cd = new ChartDisplayed(cp, this);
                cd.ChartId = ChartId;
                ChartId++;
                lCD.Add(cd);
            }


            //  turnover:  location analysis , Clients Category analysis ,client name analysis 
            for (int iBS = 1; iBS < 4; iBS++)
            {
                for (int iAT = 1; iAT < 4; iAT++)
                {

                    ChartParam cp = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = (analysisType)iAT, bs = (bookingStage)iBS, vd = valueDisplayed.turnover };
                    ChartDisplayed cd = new ChartDisplayed(cp, this);
                    cd.ChartId = ChartId;
                    ChartId++;
                    lCD.Add(cd);

                }
            }


            // offers received , sent , confirmed , cancelled , pending , pending past deadline , pendingSalesBDOPS
            for (int iAT = 2; iAT < 7; iAT++)
            {
                for (int iBS = 0; iBS < 8; iBS++)
                {
                    ChartParam cp = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = (analysisType)iAT, bs = (bookingStage)iBS, vd = valueDisplayed.bookingsAmount };
                    ChartDisplayed cd = new ChartDisplayed(cp, this);
                    cd.ChartId = ChartId;
                    ChartId++;
                    lCD.Add(cd);
                }
            }

            // offers pending as requotes , per consultant 
            ChartParam cpRequote = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = analysisType.BDconsultant, bs = bookingStage.pendingSalesBDOPSRequote, vd = valueDisplayed.bookingsAmount };
            ChartDisplayed cdRequote = new ChartDisplayed(cpRequote, this);
            cdRequote.ChartId = ChartId;
            ChartId++;
            lCD.Add(cdRequote);

            // offers pending as first quotes , per consultant 
            ChartParam cpFirstquote = new ChartParam { BeginningTF = bTF, EndTF = eTF, dptSelected = ds, btSelected = bTS, at = analysisType.BDconsultant, bs = bookingStage.pendingSalesBDOPSFirstQuote, vd = valueDisplayed.bookingsAmount };
            ChartDisplayed cdFirstquote = new ChartDisplayed(cpFirstquote, this);
            cdFirstquote.ChartId = ChartId;
            ChartId++;
            lCD.Add(cdFirstquote);



            ListChartsCreated = lCD.AsQueryable<ChartDisplayed>();




            // Creation of a Table of bookings used to check the charts

            BkgsSelectedInView = AllBookingsForAnalysis;

            //   filter by department:
            if (ds != "All") BkgsSelectedInView = BkgsSelectedInView.Where(b => b.CompanyDepartment == ds);

            //   filter by booking type:
            IList<string> btCodesSelected = (IList<string>)compSpec.bookingTypes[bTS];
            BkgsSelectedInView = BkgsSelectedInView.Where(b => btCodesSelected.Contains(b.BkgType.Trim()));

            //      sort by date entered belonging to the time frame
            IEnumerable<BkgAnalysisInfo> BkgsSelectedDE = BkgsSelectedInView.Where(b => (b.DateEntered >= bTF) && (b.DateEntered <= eTF)).OrderBy(b => b.DateEntered);

            //      sort by date confirmed belonging to the timeframe
            IEnumerable<BkgAnalysisInfo> BkgsSelectedDC = BkgsSelectedInView.Where(b => !string.Equals(b.DateConfirmed, ""));

            BkgsSelectedDC = BkgsSelectedDC
                .Where(b => b.DateConfirmed >= bTF && b.DateConfirmed <= eTF)
                .OrderBy(b => b.DateConfirmed);


            //      merge the 2 lists sorted by timeframe
            BkgsSelectedInView = BkgsSelectedDC.Union(BkgsSelectedDE);




            // creation of a second table where data are to be exported to Excel and sorted by date entered 
            BkgsSelectedInView2 = BookingsToBeExported(bTF, eTF, ds, bTS);





        }

        private IEnumerable<BkgAnalysisInfo> BookingsToBeExported(DateTime bTF, DateTime eTF, string ds, string bTS)
        {

            IEnumerable<BkgAnalysisInfo> _BkgsSelectedInView2 = AllBookingsForAnalysis;

            //   filter by department:
            if (ds != "All") _BkgsSelectedInView2 = _BkgsSelectedInView2.Where(b => b.CompanyDepartment == ds);

            //   filter by booking type:
            IList<string> btCodesSelected = (IList<string>)compSpec.bookingTypes[bTS];
            _BkgsSelectedInView2 = _BkgsSelectedInView2.Where(b => btCodesSelected.Contains(b.BkgType.Trim()));



            //      sort by date entered belonging to the time frame
            IEnumerable<BkgAnalysisInfo> BkgsSelectedDE2 = _BkgsSelectedInView2.Where(b => (b.DateEntered >= bTF) && (b.DateEntered <= eTF));

            //      sort by date confirmed belonging to the timeframe
            IEnumerable<BkgAnalysisInfo> BkgsSelectedDC2 = _BkgsSelectedInView2.Where(b => !string.Equals(b.DateConfirmed, ""));
            BkgsSelectedDC2 = BkgsSelectedDC2.Where(b => b.DateConfirmed >= bTF && b.DateConfirmed <= eTF);

            //      add all the pending bookings:
            //          filter by the booking stage pending
            List<string> StatusCodes = (List<string>)compSpec.BookingStageCodes[bookingStage.pending];
            IEnumerable<BkgAnalysisInfo> PendingBookings = _BkgsSelectedInView2.Where(b => StatusCodes.Contains(b.BookingStatus));


            //      merge the 3 lists
            _BkgsSelectedInView2 = BkgsSelectedDE2.Union(BkgsSelectedDC2).Union(PendingBookings).OrderBy(b => b.DateEntered);


            return _BkgsSelectedInView2;

        }

        public async Task<Tuple<IEnumerable<object>, Dictionary<string, List<string>>>> AllTravellingsFrom2015Async()
        {
            // the method will return 2 values:
            //   the first one is the list of bookings 
            //   the second one is the list of booking stages and their  status

            // used for Groups Travelling Overview 

            // booking stages:
            //      Unconfirmed ( Sent and pending)
            //      Confirmed
            //      Cancelled


            // returns a list of anonymous objects containing travelling information used for filtering on the view



            // launch all the tasks at once
            var GetDRMsTask = tpRepo.GetDRMsAsync();
            var GetSA3Task = tpRepo.GetSA3Async();
            var GetCSLTask = tpRepo.GetCSLAsync();
            var GetPaxNumbersTask = tpRepo.GetPaxNumbersAsync();
            var keyStagesDatesTask = compDbRepo.RetrieveKeyStagesDatesAsync();
            var BkgsSelected2Task = tpRepo.GetBHDsFromDateEnteredAsync(BeginningFY15);








            // Extraction of dictionaries
            Dictionary<string, string> _DRM = (await GetDRMsTask).ToDictionary(d => d.CODE, d => d.NAME);


            //      location name
            Dictionary<string, string> _SA3 = (await GetSA3Task).ToDictionary(c => c.CODE, c => c.DESCRIPTION);


            Dictionary<string, string> _CSL = (await GetCSLTask).ToDictionary(c => c.INITIALS, c => c.NAME);


            //     pax number
            //Dictionary<int, int> _BSD = db.BSDs.Where(bsd => bsd.BSL_ID == 0)
            //    .ToDictionary(bsd => bsd.BHD_ID, bsd => bsd.PAX);
            Dictionary<int, int> _BSD = await GetPaxNumbersTask;


            // creation of a dictionary for the booking types
            Dictionary<string, string> _BookingTypes = compSpec.bookingTypes
                .Where(kvp => kvp.Key != "All")
                .SelectMany(kvp => kvp.Value).ToDictionary(s => s,
                s => compSpec.bookingTypes.FirstOrDefault(kvp => kvp.Value.Contains(s)).Key);



            // definition of the booking stages depending on the booking status
            //      unconfirmed is BookingStageCodes[bookingStage.sent] + BookingStageCodes[bookingStage.pending]
            //      confirmed is BookingStageCodes[bookingStage.confirmed]
            //      cancelled is BookingStageCodes[bookingStage.cancelled]
            Dictionary<string, List<string>> Stages = new Dictionary<string, List<string>>();
            Stages.Add("Unconfirmed", compSpec.BookingStageCodes[bookingStage.sent].Concat(compSpec.BookingStageCodes[bookingStage.pending]).ToList());
            Stages.Add("Confirmed", compSpec.BookingStageCodes[bookingStage.confirmed]);
            Stages.Add("Cancelled", compSpec.BookingStageCodes[bookingStage.cancelled]);

            Dictionary<string, string> CodeToStage = Stages.SelectMany(kvp => kvp.Value)
                .ToDictionary(s => s, s => Stages.FirstOrDefault(kvp => kvp.Value.Contains(s)).Key);



            // bug fix 13/04/2017
            //      issue when there is a booking with no BSD.BSL_ID == 0
            //      Pax = _BSD[b.BHD_ID],
            Func<BHD, int> ReturnPAX = b => _BSD.ContainsKey(b.BHD_ID) ? _BSD[b.BHD_ID] : 0;



            var keyStagesDates = await keyStagesDatesTask;
            Func<BHD, object> ProjectToTravelInfo = b =>
             {

                 DateTime wc = b.TRAVELDATE.DayOfWeek == DayOfWeek.Sunday ? b.TRAVELDATE.AddDays(-6) : b.TRAVELDATE.AddDays(1 - (int)b.TRAVELDATE.DayOfWeek); // week commencing this year , always the first Monday of the week

                 DateTime wc1YearForward = b.TRAVELDATE.AddYears(1).DayOfWeek == DayOfWeek.Sunday ? b.TRAVELDATE.AddYears(1).AddDays(-6) : b.TRAVELDATE.AddYears(1).AddDays(1 - (int)b.TRAVELDATE.AddYears(1).DayOfWeek);// for Year on Year comparison : it will be the first Monday of the date in one year


                 // based on date Entered of the enquiry
                 DateTime wcEnt = b.DATE_ENTERED.DayOfWeek == DayOfWeek.Sunday ? b.DATE_ENTERED.AddDays(-6) : b.DATE_ENTERED.AddDays(1 - (int)b.DATE_ENTERED.DayOfWeek); // week commencing this year , always the first Monday of the week

                 DateTime wcEnt1YearForward = b.DATE_ENTERED.AddYears(1).DayOfWeek == DayOfWeek.Sunday ? b.DATE_ENTERED.AddYears(1).AddDays(-6) : b.DATE_ENTERED.AddYears(1).AddDays(1 - (int)b.DATE_ENTERED.AddYears(1).DayOfWeek);// for Year on Year comparison : it will be the first Monday of the date in one year


                 // based on the finalised date of the enquiry (date sent , confirmed , cancelled)
                 //     if the date is missing it will be the travel date
                 DateTime finalisedDate = keyStagesDates.ContainsKey(b.BHD_ID) ? keyStagesDates[b.BHD_ID].GetLastStageDate() : b.DATE_ENTERED;


                 DateTime wcFin = finalisedDate.DayOfWeek == DayOfWeek.Sunday ? finalisedDate.AddDays(-6) : finalisedDate.AddDays(1 - (int)finalisedDate.DayOfWeek);

                 DateTime wcFin1YearForward = finalisedDate.AddYears(1).DayOfWeek == DayOfWeek.Sunday ? finalisedDate.AddYears(1).AddDays(-6) : finalisedDate.AddYears(1).AddDays(1 - (int)finalisedDate.AddYears(1).DayOfWeek);

                 //     detection of the finalised stage : "Sent" , "Confirmed" , "Cancelled" or "None"
                 string finStage = "None";
                 if (compSpec.IsSent(b.STATUS)) finStage = "Sent";
                 if (compSpec.IsConfirmed(b.STATUS)) finStage = "Confirmed";
                 if (compSpec.IsCancelled(b.STATUS)) finStage = "Cancelled";


                 object objInfo = new
                 {
                     AgentName = _DRM[b.AGENT],
                     BookingType = _BookingTypes[b.SALE6],
                     LocationName = _SA3[b.SALE3],
                     CompanyDpt = compSpec.RetrieveDepartment(b.FULL_REFERENCE),
                     WeekCommencing = wc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), // transformation of the format to be converted to date in javascript
                     WillBeCompared1YearForwardWith = wc1YearForward.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),   // the week commencing on the next year which will start by a Monday
                     BookingStage = CodeToStage[b.STATUS.Trim()],
                     Full_Reference = b.FULL_REFERENCE,
                     DatetimeWeekCommencing = wc,       // will be used for grouping

                     // new properties to be displayed in the chart's tooltips
                     TravelDate = b.TRAVELDATE.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                     EstimatedTurnover = b.UDTEXT3,
                     SalesUpdate = b.UDTEXT5,
                     Consultant = _CSL[b.CONSULTANT],
                     AgentCode = b.AGENT,
                     Status = b.STATUS,
                     BookingName = b.NAME,
                     Pax = ReturnPAX(b),
                     ConsultantCode = b.CONSULTANT,

                     // used for enquiries entered
                     DateEntered = b.DATE_ENTERED.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                     WeekCommencingEnt = wcEnt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), // transformation of the format to be converted to date in javascript
                     WillBeCompared1YearForwardWithEnt = wcEnt1YearForward.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),   // the week commencing on the next year which will start by a Monday
                     DatetimeWeekCommencingEnt = wcEnt,      // will be used for grouping

                     // used for finalised enquiries
                     DateFinalised = finalisedDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                     WeekCommencingFin = wcFin.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                     WillBeCompared1YearForwardWithFin = wcFin1YearForward.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                     DatetimeWeekCommencingFin = wcFin,
                     FinalStage = finStage,
                     //DateMissing = b.UDTEXT4.Trim() == "" && finStage != "None" ? "Yes" : "No",
                     DateMissing = "No",
                     SeriesReferenceMissing = b.UDTEXT1.Trim() == "" && compSpec.bookingTypes["Brochure"].Contains(b.SALE6.Trim()) ? "Yes" : "No"


                 };


                 return objInfo;
             };


            IEnumerable<BHD> BkgsSelected2 = (await BkgsSelected2Task)
                .Where(b => compSpec.BookingStageCodes[bookingStage.received].Contains(b.STATUS.Trim()));
            IEnumerable<object> TravelBkgs = BkgsSelected2.Select(b => ProjectToTravelInfo(b)).ToList();

            return new Tuple<IEnumerable<object>, Dictionary<string, List<string>>>(TravelBkgs, Stages);

        }

    }






}