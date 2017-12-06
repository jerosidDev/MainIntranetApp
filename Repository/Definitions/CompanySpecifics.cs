using Reporting_application.Repository.ThirdpartyDB;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Reporting_application.Utilities.CompanyDefinition
{

    public enum bookingStage { received, sent, confirmed, cancelled, pending, pendingPastDeadline, pendingSalesBDOPS, pendingContract, pendingSalesBDOPSRequote, pendingSalesBDOPSFirstQuote, Bedbank };


    public enum ServiceLineType { Unconfirmed };


    public class CompanySpecifics
    {

        // department list with the ThirdpartyDB code associated with each
        public Dictionary<string, List<string>> DptsList { get; set; } = new Dictionary<string, List<string>>();

        // classification of booking type:
        public Dictionary<string, List<string>> bookingTypes { get; set; } = new Dictionary<string, List<string>>();

        // definition of the different codes used at different booking stage
        public Dictionary<bookingStage, List<string>> BookingStageCodes { get; set; } = new Dictionary<bookingStage, List<string>>();

        // pendingPlaces where the pending offers are currently blocked
        public Dictionary<string, List<string>> pendingOffersPlaces { get; set; } = new Dictionary<string, List<string>>();


        public Dictionary<string, DeadlineItem> dictDeadlines { get; set; } = new Dictionary<string, DeadlineItem>();



        public Func<string, bool> IsSent { get; private set; }
        public Func<string, bool> IsConfirmed { get; private set; }
        public Func<string, bool> IsCancelled { get; private set; }
        public Func<string, bool> IsUnconfirmed { get; private set; }



        // definition of the different codes used for the service lines
        public Dictionary<ServiceLineType, List<string>> ServiceLinesCodes { get; set; }


        public CompanySpecifics()
        {


            // definition of the departements :
            DptsList["French"] = new List<string>() { "FR" };
            DptsList["Benelux"] = new List<string>() { "BE", "TM" };
            DptsList["German_and_other_Countries"] = new List<string>() { "AT", "GR", "EO" };
            DptsList["Canadian"] = new List<string>() { "CD" };
            DptsList["Italian"] = new List<string>() { "IL", "IY", "IT" };
            DptsList["Sports"] = new List<string>() { "SV" };       // has to be evaluated as a branch or as a dpt
            DptsList["Incentive_Group"] = new List<string>() { "EI", "IS" };    // IS when it used as a branch ex: ISFR
            DptsList["MICE"] = new List<string>() { "MI" };
            DptsList["FIT"] = new List<string>() { "FI", "FO" };




            // definition of the classification of booking type:
            bookingTypes["Adhoc"] = new List<string>() { "AA", "AD", "SO", "FO", "FI" };
            //      definition of what is Brochure
            bookingTypes["Brochure"] = new List<string>() { "BR", "SB" };
            //      definition of what is famtrip
            bookingTypes["Famtrip"] = new List<string>() { "FA" };



            // definition of the different booking stages depending on the booking status :

            //      offer at all possible booking stages : received = sent + pending + confirmed +  cancelled
            BookingStageCodes[bookingStage.sent] = new List<string>() { "Q", "QF", "SO" };
            BookingStageCodes[bookingStage.confirmed] = new List<string>() { "S", "CS", "CF", "IN" };
            BookingStageCodes[bookingStage.cancelled] = new List<string>() { "X", "TX" };
            BookingStageCodes[bookingStage.pending] = new List<string>() { "P", "PE", "QU", "QR" };


            //      received contains everything in the 4 primary stages
            BookingStageCodes[bookingStage.received] = BookingStageCodes[bookingStage.sent]
                .Concat(BookingStageCodes[bookingStage.confirmed])
                .Concat(BookingStageCodes[bookingStage.cancelled])
                .Concat(BookingStageCodes[bookingStage.pending])
                .ToList();


            // bed bank
            BookingStageCodes[bookingStage.Bedbank] = new List<string>() { "B" };

            //      break down of the pending types
            BookingStageCodes[bookingStage.pendingPastDeadline] = BookingStageCodes[bookingStage.pending];
            BookingStageCodes[bookingStage.pendingContract] = new List<string>() { "P" };
            BookingStageCodes[bookingStage.pendingSalesBDOPS] = new List<string>() { "QU", "QR" };
            BookingStageCodes[bookingStage.pendingSalesBDOPSRequote] = new List<string>() { "QR" };
            BookingStageCodes[bookingStage.pendingSalesBDOPSFirstQuote] = new List<string>() { "QU" };

            // definition of the pendingPlaces where the pending offers are currently blocked
            pendingOffersPlaces = new Dictionary<string, List<string>>();
            pendingOffersPlaces["Contract process"] = new List<string>() { "P" };
            pendingOffersPlaces["with Clients"] = new List<string>() { "PE" };
            pendingOffersPlaces["Quote Sales/Bd Ops"] = new List<string>() { "QU" };
            pendingOffersPlaces["Requote Sales/Bd Ops"] = new List<string>() { "QR" };


            // definition of the deadline depending on the type of offer defined by its TP code
            dictDeadlines.Add("A", new DeadlineItem() { MaxDaysQuoting = 2, MaxDaysContracting = 0 });
            dictDeadlines.Add("B", new DeadlineItem() { MaxDaysQuoting = 1, MaxDaysContracting = 1 });
            dictDeadlines.Add("C", new DeadlineItem() { MaxDaysQuoting = 3, MaxDaysContracting = 0 });
            dictDeadlines.Add("D", new DeadlineItem() { MaxDaysQuoting = 2, MaxDaysContracting = 3 });
            dictDeadlines.Add("E", new DeadlineItem() { MaxDaysQuoting = 5, MaxDaysContracting = 0 });
            dictDeadlines.Add("F", new DeadlineItem() { MaxDaysQuoting = 3, MaxDaysContracting = 5 });
            dictDeadlines.Add("G", new DeadlineItem() { MaxDaysQuoting = 5, MaxDaysContracting = 0 });
            dictDeadlines.Add("H", new DeadlineItem() { MaxDaysQuoting = 3, MaxDaysContracting = 7 });


            // delegates used to identify stages from the status
            IsSent = s => BookingStageCodes[bookingStage.sent].Contains(s.Trim());
            IsConfirmed = s => BookingStageCodes[bookingStage.confirmed].Contains(s.Trim());
            IsCancelled = s => BookingStageCodes[bookingStage.cancelled].Contains(s.Trim());
            IsUnconfirmed = s => !BookingStageCodes[bookingStage.confirmed].Concat(BookingStageCodes[bookingStage.cancelled]).Contains(s.Trim());


            // service lines definition
            ServiceLinesCodes = new Dictionary<ServiceLineType, List<string>>();
            ServiceLinesCodes[ServiceLineType.Unconfirmed] = new List<string>() { "CX", "XP", "TB" };

        }

        public string RetrieveDepartment(string FullReference)
        {
            // return the long name department associated with the code branch and the code department

            string branch = FullReference.Trim().Substring(0, 2);
            string dptCode = FullReference.Trim().Substring(2, 2);

            // test on the branch field
            // test on the department field
            string fullNameDpt = DptsList.Where(d => d.Value.Contains(branch)).FirstOrDefault().Key
                ?? DptsList.Where(d => d.Value.Contains(dptCode)).FirstOrDefault().Key
                ?? "unassigned";


            return fullNameDpt;

        }

        public bool IsBHDunconfirmed(BHDmini b)
        {
            return !BookingStageCodes[bookingStage.confirmed].Concat(BookingStageCodes[bookingStage.cancelled])
              .Contains(b.STATUS.Trim());
        }


        public string GetCurrentStage(string bStatus)
        {

            // Stages defined : 
            //      Quoting , Contracting

            if (BookingStageCodes[bookingStage.pendingSalesBDOPS].Contains(bStatus)) return "Quoting";

            if (BookingStageCodes[bookingStage.pendingContract].Contains(bStatus)) return "Contracting";

            return "";


        }

        public List<BookingStageItem> LoadBookingStageItems()
        {


            List<BookingStageItem> bsi = new List<BookingStageItem>();


            bsi.Add(new BookingStageItem() { Name = "All", Statuses = BookingStageCodes[bookingStage.received] });


            bsi.Add(new BookingStageItem() { Name = "Unconfirmed", Statuses = BookingStageCodes[bookingStage.sent].Concat(BookingStageCodes[bookingStage.pending]).ToList() });


            bsi.Add(new BookingStageItem() { Name = "Confirmed", Statuses = BookingStageCodes[bookingStage.confirmed] });


            bsi.Add(new BookingStageItem() { Name = "Cancelled", Statuses = BookingStageCodes[bookingStage.cancelled] });



            return bsi;


        }




    }

    public class BookingStageItem
    {
        public string Name { get; set; }

        public List<string> Statuses { get; set; }


        public bool IsStage(string bookingStatus)
        {
            return Statuses.Contains(bookingStatus.Trim());
        }


    }

    public class DeadlineItem
    {
        public int MaxDaysQuoting { get; set; }
        public int MaxDaysContracting { get; set; }

    }



}