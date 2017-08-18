using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.DataVisualization.Charting;

namespace Reporting_application.ReportingModels
{


    // see the text file "ERA , chart types" for the full structure
    public enum valueDisplayed { turnover, bookingsAmount };

    public enum analysisType { departmentName, clientName, location, clientCategory, BDconsultant, pendingPlaces, consultant, YearOnYear, YearOnYearCumulative };





    //  object containing parameters from the view
    public class ChartParam
    {


        public DateTime BeginningTF { get; set; }
        public DateTime EndTF { get; set; }

        public valueDisplayed vd { get; set; }
        public bookingStage bs { get; set; }
        public analysisType at { get; set; }
        public string dptSelected { get; internal set; }
        public string btSelected { get; internal set; }
    }



    // class managing the display of a chart
    public class ChartDisplayed
    {
        // the parameters set from the view
        public ChartParam CP { get; set; }
        // the data model
        public BookingsStagesAnalysis pcModel { get; set; }


        // used to access the chart from the view
        public int ChartId { get; internal set; }

        public double Total { get; private set; }


        // culture format is the same as in pcModel
        private IFormatProvider culture { get; set; }


        // the chart created
        public Chart ChartCreated { get; set; }


        // labels and values of the pie chart exposed in order to be visualised in a table if necessary
        // they are ordered by descending values
        public Dictionary<string, double> PieChartLabelsValues { get; private set; }



        // bookings data to be displayed on the view for data verification
        public IEnumerable<BkgAnalysisInfo> BkgsSelectedForChart;


        // class containing the popover elements used to display the popover
        public PopoverElements PopElem;




        public ChartDisplayed(ChartParam CPTransmitted, BookingsStagesAnalysis pcModelTransmitted)
        {

            CP = CPTransmitted;
            pcModel = pcModelTransmitted;
            culture = pcModelTransmitted.culture;




            ChartCreated = new Chart();
            Series s1 = ChartCreated.Series.Add("s1");


            ChartCreated.Width = 900;
            ChartCreated.Height = 600;
            ChartCreated.ChartAreas.Add("c1");



            // fill the series in the chart
            FillDataPoints(s1);



            // Graphics and visuals

            // set the chart as pie
            s1.ChartType = SeriesChartType.Pie;

            // if many labels
            if (s1.Points.Count >= 5)
            {

                // labels are then set outside
                s1.SetCustomProperty("PieLabelStyle", "Outside");
                s1.BorderWidth = 1;
                s1.BorderDashStyle = ChartDashStyle.Dot;
                s1.BorderColor = System.Drawing.Color.FromArgb(200, 26, 59, 105);

                // Set the pie chart to be 3D
                ChartCreated.ChartAreas[0].Area3DStyle.Enable3D = true;

                // By setting the inclination to 0, the chart essentially goes back to being a 2D chart
                ChartCreated.ChartAreas[0].Area3DStyle.Inclination = 0;

            }


            //  if client or departemental overview : add a big title
            if (((CP.at == analysisType.departmentName) || (CP.at == analysisType.clientName)) && (CP.vd == valueDisplayed.bookingsAmount))
            {
                // add title
                Title t1 = ChartCreated.Titles.Add("Total offers received: " + Total);
                System.Drawing.Font f1 = new System.Drawing.Font("Microsoft Sans Serif", 16);
                t1.Font = f1;
            }
            else
            {
                //  else being: secondary chart in size
                // increase the font size for the label
                if (s1.Points.Count != 0)
                {
                    for (int p = 0; p < s1.Points.Count; p++)
                    {
                        s1.Points[p].Font = new System.Drawing.Font("Microsoft Sans Serif", 16);
                    }
                }
            }


            // generate data for the popover window
            PopElem = new PopoverElements(this);




        }

        // constructor used for year on year analysis
        public ChartDisplayed(ChartParam CPTransmitted, BookingsStagesAnalysis pcModelTransmitted, string ReportType)
        {



            CP = CPTransmitted;
            pcModel = pcModelTransmitted;

            if (ReportType == "YearOnYear")
            {
                // calculation of the data

                IEnumerable<BkgAnalysisInfo> BkgsSelected = pcModel.AllBookingsForAnalysis;


                //   filter by department:
                if (CP.dptSelected != "All") BkgsSelected = BkgsSelected.Where(b => b.CompanyDepartment == CP.dptSelected);

                //   filter by booking type:
                IList<string> btCodesSelected = (IList<string>)pcModel.compSpec.bookingTypes[CP.btSelected];
                BkgsSelected = BkgsSelected.Where(b => btCodesSelected.Contains(b.BkgType.Trim()));


                // filter by the booking stage
                List<string> StatusCodes = (List<string>)pcModel.compSpec.BookingStageCodes[CP.bs];
                BkgsSelected = BkgsSelected.Where(b => StatusCodes.Contains(b.BookingStatus));



                string tCurrentYear = "";
                string tPreviousYear = "";
                switch (CP.at)
                {
                    case analysisType.YearOnYear:
                        tCurrentYear = "Current year weekly";
                        tPreviousYear = "Previous year weekly";
                        break;
                    case analysisType.YearOnYearCumulative:
                        tCurrentYear = "Current year cumulative";
                        tPreviousYear = "Previous year cumulative";
                        break;
                }

                // chart initialisation
                int chartsWidth = 1200;
                int chartsHeight = 600;
                ChartCreated = new Chart() { Width = chartsWidth, Height = chartsHeight };
                ChartArea ca = ChartCreated.ChartAreas.Add("c1");
                Series CurrFY = ChartCreated.Series.Add(tCurrentYear);
                Series PrevFY = ChartCreated.Series.Add(tPreviousYear);



                // find the first Sunday of the current FY:

                int CurrentFY = DateTime.Today.Month != 1 ? DateTime.Today.Year : DateTime.Today.Year - 1;

                // starting from the 01/02 of the current FY
                DateTime FirstDayWeek = new DateTime(CurrentFY, 2, 1);

                // first Sunday of the current FY
                DateTime LastDayWeek = FirstDayWeek.DayOfWeek == DayOfWeek.Sunday ? FirstDayWeek : FirstDayWeek.AddDays(7 - (int)FirstDayWeek.DayOfWeek);




                // populating the data for CurrentFY and PreviousFY
                // the data are populated until the end of the current FY to allow the year on year comparison
                // calculation by sum of the date entered in [FirstDayWeek;LastDayWeek]
                DateTime FirstDayNextFY = new DateTime(CurrentFY + 1, 2, 1);


                // populating the series but for the current FY it stops at the last sunday
                DateTime LastSunday = DateTime.Today.AddDays(-1 * (int)DateTime.Today.DayOfWeek);
                do
                {
                    int curr = BkgsSelected.Where(b => b.DateEntered >= FirstDayWeek && b.DateEntered <= LastDayWeek).Count();
                    int prev = BkgsSelected.Where(b => b.DateEntered >= FirstDayWeek.AddYears(-1) && b.DateEntered <= LastDayWeek.AddYears(-1)).Count();


                    // only show label for cumulative if the LastDayWeek is LastSunday

                    bool ShowLabel = CP.at == analysisType.YearOnYear || (CP.at == analysisType.YearOnYearCumulative && LastDayWeek == LastSunday);

                    if (curr != 0 && LastDayWeek <= LastSunday)
                    {
                        int pt = CurrFY.Points.AddXY(LastDayWeek, curr);
                        if (ShowLabel) CurrFY.Points[pt].Label = curr.ToString();
                    }
                    if (prev != 0)
                    {
                        int pt = PrevFY.Points.AddXY(LastDayWeek, prev);
                        if (ShowLabel) PrevFY.Points[pt].Label = prev.ToString();
                    }



                    if (CP.at != analysisType.YearOnYearCumulative) FirstDayWeek = LastDayWeek.AddDays(1);
                    LastDayWeek = LastDayWeek.AddDays(7);
                } while (LastDayWeek < FirstDayNextFY);


                // graphics and visuals

                CurrFY.ChartType = SeriesChartType.Line;
                PrevFY.ChartType = SeriesChartType.Line;

                ca.AxisX.LabelStyle.Format = "dd MMM";
                ca.AxisX.Interval = 30;  // 30 days interval
                ca.AxisX.MajorGrid.Enabled = false;
                ca.AxisY.MajorGrid.Enabled = false;
                ca.AxisY.LabelStyle.Enabled = false;
                ca.AxisY.MajorTickMark.Enabled = false;
                ChartCreated.Legends.Add("l1").Docking = Docking.Top;


                ca.BackColor = System.Drawing.Color.Azure;



            }

        }


        private void FillDataPoints(Series s)
        {

            // funnelling down the data to present only valid bookings

            Total = 0;
            IEnumerable<BkgAnalysisInfo> BkgsSelected = pcModel.AllBookingsForAnalysis;



            //   filter by department:
            if (CP.dptSelected != "All") BkgsSelected = BkgsSelected.Where(b => b.CompanyDepartment == CP.dptSelected);

            //   filter by booking type:
            IList<string> btCodesSelected = (IList<string>)pcModel.compSpec.bookingTypes[CP.btSelected];
            BkgsSelected = BkgsSelected.Where(b => btCodesSelected.Contains(b.BkgType.Trim()));



            //      filter by Timeframe:  >= dateBeginning and <= dateEnd (the time should always be 0)
            //      no timeframe filter for pending offers

            //          booking stages :  received is evaluated by date entered , sent/confirmed/cancelled are evaluated by date confirmed(UDTEXT4) , pending/../.. are not date dependent



            // filter by the booking stage
            List<string> StatusCodes = (List<string>)pcModel.compSpec.BookingStageCodes[CP.bs];
            BkgsSelected = BkgsSelected.Where(b => StatusCodes.Contains(b.BookingStatus));




            // additional filtering depending on the type of booking stage
            switch (CP.bs)
            {
                // filter by date entered
                case bookingStage.received:

                    BkgsSelected = BkgsSelected.Where(b => (b.DateEntered >= CP.BeginningTF) && (b.DateEntered <= CP.EndTF)).OrderBy(b => b.DateEntered);

                    break;


                // filter by date confirmed
                case bookingStage.sent:
                case bookingStage.confirmed:
                case bookingStage.cancelled:

                    // filter by Timeframe
                    BkgsSelected = BkgsSelected.Where(b => !string.Equals(b.DateConfirmed, ""));
                    BkgsSelected = BkgsSelected.Where(b => b.DateConfirmed >= CP.BeginningTF && b.DateConfirmed <= CP.EndTF).OrderBy(b => b.DateConfirmed);

                    break;

                case bookingStage.pendingPastDeadline:
                case bookingStage.pendingSalesBDOPS:
                case bookingStage.pendingContract:
                case bookingStage.pendingSalesBDOPSRequote:
                case bookingStage.pendingSalesBDOPSFirstQuote:
                    // only select the bookings past deadline
                    BkgsSelected = BkgsSelected.Where(b => b.PastDeadline == true);
                    break;

            }


            BkgsSelectedForChart = BkgsSelected;

            // hashtable of bookings associated with a key being an element of the analysis type
            Hashtable CatBookingsSelection = new Hashtable();


            // bookings selection per analysis type

            Func<BkgAnalysisInfo, string> FieldEvaluator = b =>
           {
               string evaluator = "";
               switch (CP.at)
               {
                   case analysisType.clientName:
                       evaluator = b.AgentName;
                       break;
                   case analysisType.location:
                       evaluator = b.LocationName;
                       break;
                   case analysisType.clientCategory:
                       evaluator = b.ClientCategory;
                       break;
                   case analysisType.BDconsultant:
                       evaluator = b.BDConsultant;
                       break;
                   case analysisType.departmentName:
                       evaluator = b.CompanyDepartment;
                       break;
                   case analysisType.pendingPlaces:
                       evaluator = b.PendingArea;
                       break;
                   case analysisType.consultant:
                       evaluator = b.Consultant;
                       break;
               }
               return evaluator;
           };



            List<string> CategoryList = BkgsSelected.Select(b => FieldEvaluator(b)).Distinct().ToList();
            foreach (string ct in CategoryList)
            {
                CatBookingsSelection[ct] = BkgsSelected.Where(b => FieldEvaluator(b) == ct);
            }




            // function depending on the right type of value depending on booking count or booking turnover
            Func<IEnumerable<BkgAnalysisInfo>, double> ReturnValueDisplayed = ListBkg =>
             {
                 switch (CP.vd)
                 {
                     case valueDisplayed.turnover:
                         var l = ListBkg.Select(b =>
                         {
                             double r;
                             string s1 = b.EstimatedTurnover.Replace(" ", "").Replace("£", "").Replace("'", "");
                             double.TryParse(s1, out r);
                             return r;
                         }).Sum();
                         return l;
                     case valueDisplayed.bookingsAmount:
                         return ListBkg.Count();
                 }
                 return 0;
             };


            PieChartLabelsValues = new Dictionary<string, double>();


            // filling the points in the series
            foreach (string k in CatBookingsSelection.Keys)
            {

                IEnumerable<BkgAnalysisInfo> b = (IEnumerable<BkgAnalysisInfo>)CatBookingsSelection[k];
                double n = ReturnValueDisplayed(b);
                if (n != 0)
                {
                    int pt = s.Points.AddXY(k, n);


                    // if turnover change the format to £ with ","
                    string nStr = n.ToString();
                    if (CP.vd == valueDisplayed.turnover) nStr = "£" + string.Format("{0:#,##0}", n);


                    s.Points[pt].Label = k.Trim() + ":" + nStr;

                    // store the label and value 
                    PieChartLabelsValues.Add(s.Points[pt].Label, n);


                    Total += n;

                }

            }

            // order PieChartLabelsValues by descending values
            PieChartLabelsValues = PieChartLabelsValues.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);




        }

    }

    public class PopoverElements
    {

        public PopoverElements(ChartDisplayed chartDisplayed)
        {



            // create the title
            switch (chartDisplayed.CP.bs)
            {
                case bookingStage.received:
                    title = "Offers received";
                    break;
                case bookingStage.sent:
                    title = "Offers sent";
                    break;
                case bookingStage.confirmed:
                    title = "Confirmed bookings";
                    break;
                case bookingStage.cancelled:
                    title = "Cancelled bookings";
                    break;
                case bookingStage.pending:
                    title = "Pending offers";
                    break;
                case bookingStage.pendingPastDeadline:
                    title = "Pending offers past deadline";
                    break;
                case bookingStage.pendingSalesBDOPS:
                    title = "past deadline with Sale/Bd Ops";
                    break;
                case bookingStage.pendingContract:
                    title = "past deadline on contracting";
                    break;
                case bookingStage.pendingSalesBDOPSRequote:
                    title = "past deadline being requoted";
                    break;
                case bookingStage.pendingSalesBDOPSFirstQuote:
                    title = "pastdeadline being quoted";
                    break;
                default:
                    title = "Booking stage not yet titled";
                    break;
            }

            title = (title + " :" + chartDisplayed.Total).Replace(" ", "&nbsp;");


            // create the content
            content = "";
            foreach (KeyValuePair<string, double> kvp in chartDisplayed.PieChartLabelsValues)
            {
                content += $"{kvp.Key}                                                                                       &#010";
            }
            content = content.Replace(" ", "&nbsp");


        }

        // the title on top of the popover window
        public string title { get; set; }
        // the content inside of the popover window
        public string content { get; set; }



    }
}
