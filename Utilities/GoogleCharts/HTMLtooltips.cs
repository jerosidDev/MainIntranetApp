using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.Performance;

namespace Reporting_application.Utilities.GoogleCharts
{
    // containers for information to be displayed on the google column chart tooltip
    public abstract class TooltipSentEnquiry
    {
        // I can't define the common properties here without affecting the way the columns are displayed in the HTML tooltip

    }

    public class TooltipQuotedEnquiry : TooltipSentEnquiry
    {
        public string Full_Reference { get; set; }
        public string Bkg_Name { get; set; }
        public string Date_Entered { get; set; }
        public string Date_Sent { get; set; }

        public string Work_Days_Quoting { get; set; }
        public string Deadline { get; set; }


        public TooltipQuotedEnquiry(SentEnquiry se, BHDmini b)
        {
            Full_Reference = se.FullReference;
            Bkg_Name = b.NAME;
            Date_Entered = b.DATE_ENTERED.ToString("dd/MM/yyyy");
            Date_Sent = se.DateSent.ToString("dd/MM/yyyy");
            Deadline = se.maxNbDaysQuoting.ToString();
            if (se.maxNbDaysQuoting == 9999 || se.maxNbDaysQuoting == -1) Deadline = b.UDTEXT4.Trim();

            Work_Days_Quoting = se.nbDaysQuoting.ToString();
        }

    }


    public class TooltipContractEnquiry : TooltipSentEnquiry
    {


        public string Full_Reference { get; set; }
        public string Bkg_Name { get; set; }
        public string Date_Entered { get; set; }
        public string Date_Sent { get; set; }

        public string Work_Days_Contracting { get; set; }

        public string Deadline { get; set; }


        public TooltipContractEnquiry(SentEnquiry se, BHDmini b)
        {
            Full_Reference = se.FullReference;
            Bkg_Name = b.NAME;
            Date_Entered = b.DATE_ENTERED.ToString("dd/MM/yyyy");
            Date_Sent = se.DateSent.ToString("dd/MM/yyyy");
            Deadline = se.maxNbDaysQuoting.ToString();
            if (se.maxNbDaysQuoting == 9999 || se.maxNbDaysQuoting == -1) Deadline = b.UDTEXT4.Trim();



            Work_Days_Contracting = se.nbDaysContracting.ToString();
        }

    }



    public class TooltipConversion
    {
        public string Full_Reference { get; set; }
        public string Booking_Name { get; set; }
        public string Date_Entered { get; set; }
        public string Travel_Date { get; set; }
        public string Current_Status { get; set; }


        public TooltipConversion(BHDmini b)
        {
            Full_Reference = b.FULL_REFERENCE.Trim();
            Booking_Name = b.NAME.Trim();
            Date_Entered = b.DATE_ENTERED.ToString("dd/MM/yyyy");
            Travel_Date = b.TRAVELDATE.ToString("dd/MM/yyyy");
            Current_Status = b.STATUS.Trim();
        }

    }


    public class TooltipMissingData
    {
        public string Full_Reference { get; set; }
        public string Booking_Name { get; set; }
        public string Estimated_Turnover { get; set; }
        public string Sales_Update { get; set; }
        public string Series_Reference { get; set; }


        public TooltipMissingData(BHDmini b, BHDmissingData bmd)
        {
            Full_Reference = b.FULL_REFERENCE.Trim();
            Booking_Name = b.NAME.Trim();
            Estimated_Turnover = !bmd.MissingEstimatedTurnover ? b.UDTEXT3.Trim() : "MISSING ESTIMATED TURNOVER";
            Sales_Update = !bmd.MissingSalesUpdate ? b.UDTEXT5.Trim() : "MISSING SALES UPDATE";
            Series_Reference = !bmd.MissingSeriesReference ? b.UDTEXT1.Trim() : "MISSING SERIES REFERENCE";
            Series_Reference = !bmd.IncorrectSeriesReference ? Series_Reference : "INVALID SERIES REFERENCE";
        }

    }


    // corresponds to a row in the google table for each pending enquiry
    public class PendingEnquiryTableRow
    {
        public string Full_Reference { get; set; }
        public string Booking_Name { get; set; }
        public string Last_Stage { get; set; }     // quoting or contracting
        public int Days_Before_Deadline { get; set; }
        public string Last_Consultant { get; set; }
        public string BD_Consultant { get; set; }

    }



}
