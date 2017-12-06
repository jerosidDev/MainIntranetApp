using CompanyDbWebAPI.ModelsDB;
using Reporting_application.Repository;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.Dates;
using Reporting_application.Utilities.GoogleCharts;
using Reporting_application.Utilities.Performance;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Reporting_application.Services.Performance
{
    public class Performance : IPerformance
    {

        public CompanySpecifics CompSpec { get; }


        public ICompanyDBRepository compDbRepo { get; set; }
        public IThirdpartyDBrepository tpRepo { get; set; }

        public Dictionary<string, object> dictAllPerformance { get; private set; }

        public Dictionary<string, IEnumerable<string>> dptCslAssociation { get; private set; }


        private IList<string> _dptList;
        public IList<string> dptList
        {
            get
            {
                if (_dptList == null) _dptList = CompSpec.DptsList.Keys.ToList();
                return _dptList;
            }
            set
            {
                _dptList = value;
            }
        }

        private IDictionary<string, string> _dictActiveConsult;
        public IDictionary<string, string> dictActiveConsult
        {
            get
            {
                if (_dictActiveConsult == null)
                    _dictActiveConsult = tpRepo.GetRecentlyActiveConsultants();
                return _dictActiveConsult;
            }
            set
            {
                _dictActiveConsult = value;
            }
        }



        public IEnumerable<ContractTBAsInfo> itemsVM { get; set; }
        public IEnumerable<ContractConsultant> CslTBAassignment { get; set; }



        public IDictionary<string, string> dictActiveContractCsl { get; private set; }


        public Performance(ICompanyDBRepository _compDbRepo, IThirdpartyDBrepository _tpRepo)
        {
            CompSpec = new CompanySpecifics();
            compDbRepo = _compDbRepo;
            tpRepo = _tpRepo;

        }



        public IList<SentEnquiry> CalculateSentEnquiries()
        {
            compDbRepo.ExtractAllBStages();

            IList<SentEnquiry> listSentEnquiries = generateSentEnquiriesList();

            return listSentEnquiries;

        }



        public async Task<IList<SentEnquiry>> CalculateSentEnquiriesAsync()
        {
            await compDbRepo.ExtractAllBStagesAsync();

            //await tpRepo.GetAllConsultantsAsync();
            IList<SentEnquiry> listSentEnquiries = await generateSentEnquiriesListAsync();

            return listSentEnquiries;

        }




        public Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>> EvalAllDepartments<T>(List<T> AllBookingsFiltered) where T : class, IPerformanceItems<T>
        {
            // departments and consultants are on the same levels
            // the success rate is not calculated directly but should be easily calculated by the client


            var dictReturned = new Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>>();
            CompanySpecifics cs = new CompanySpecifics();

            List<string> dptList = cs.DptsList.Keys.ToList();
            //  foreach department
            foreach (string dpt in dptList)
            {

                Dictionary<string, Dictionary<string, List<T>>> dictAllTheDpt = new Dictionary<string, Dictionary<string, List<T>>>();

                // split success failure items for the department selected
                List<T> itemsDpt = AllBookingsFiltered.Where(t => t.IsDepartment(cs, dpt)).ToList();
                Dictionary<string, List<T>> dictSplitItems = new Dictionary<string, List<T>>();
                dictSplitItems.Add("Success", itemsDpt.Where(t => t.IsSuccess(cs)).ToList());
                dictSplitItems.Add("Unsuccess", itemsDpt.Where(t => !t.IsSuccess(cs)).ToList());
                dictAllTheDpt.Add("DepartmentOnly", dictSplitItems);


                // split success failure items for all the consultants associated with the department selected
                T firstItem = AllBookingsFiltered.FirstOrDefault();
                List<string> listAllCslForSelectedDpt = firstItem.GetAllCslFromListOfItems(itemsDpt);
                foreach (string csl in listAllCslForSelectedDpt)
                {
                    // split success failure items for the csl selected
                    List<T> itemsCsl = AllBookingsFiltered.Where(t => t.IsCsl(csl)).ToList();
                    Dictionary<string, List<T>> dictCslSplitItems = new Dictionary<string, List<T>>();
                    dictCslSplitItems.Add("Success", itemsCsl.Where(t => t.IsSuccess(cs)).ToList());
                    dictCslSplitItems.Add("Unsuccess", itemsCsl.Where(t => !t.IsSuccess(cs)).ToList());
                    dictAllTheDpt.Add(csl, dictCslSplitItems);
                }

                // order by total success+unsuccess except for missing data
                if (typeof(T) == typeof(BHDmissingData))
                {
                    dictAllTheDpt = dictAllTheDpt.OrderByDescending(kvp => kvp.Value["Unsuccess"].Count()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                else
                {
                    dictAllTheDpt = dictAllTheDpt.OrderByDescending(kvp => kvp.Value["Success"].Count() + kvp.Value["Unsuccess"].Count()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }


                dictReturned.Add(dpt, dictAllTheDpt);
            }


            return dictReturned;
        }

        public void GeneratePerformanceSplits()
        {
            string jsFromDate = "2017-05-01";
            string jsToDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            DateTime FromDate = DateTime.ParseExact(jsFromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime ToDate = DateTime.ParseExact(jsToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            dictAllPerformance = new Dictionary<string, object>();


            //  07/06/2017: remove the dupplicated offer for the conversion rate and the sent enquiries
            //      definition of a dupplicatedOffer : a booking is part of a series defined by its first booking entered
            //              the initial booking should remain for the evaluation , not the dupplicatedOffer
            //              the dupplicatedOffer are kept for the missing data

            //  16/06/2017:  booking references are compared by number and the branch


            List<string> validStatus = CompSpec.BookingStageCodes[bookingStage.sent]
                .Concat(CompSpec.BookingStageCodes[bookingStage.confirmed])
                .Concat(CompSpec.BookingStageCodes[bookingStage.cancelled])
                .Concat(CompSpec.BookingStageCodes[bookingStage.pending])
                .ToList();


            //  read all bookings entered in FY16
            IEnumerable<string> AllBookingsReferencesFY16 = ExtractBookingRefFY16(validStatus);





            //   concerns : 
            //      I want to avoid having bedbanks
            //tpRepo.ExtractAllBookingsFromFY17();
            IEnumerable<BHDmini> AllBHDminiFY17 = tpRepo.AllBHDminiFromFY17
                .Where(b => validStatus.Contains(b.STATUS.Trim()));
            IEnumerable<string> AllReferencesFromFY17 = AllBHDminiFY17
                .Select(b => MinimiseRef(b.FULL_REFERENCE.Trim()));



            // insert task for missing data





            // grouped offers
            var GroupedOffers = AllBHDminiFY17
                .GroupBy(b => b.UDTEXT1.Trim())
                .Where(g => !AllBookingsReferencesFY16.Contains(MinimiseRef(g.Key)))  // remove the group if it belongs to FY16
                .SelectMany(g =>
                {
                    string groupKey = MinimiseRef(g.Key);
                    if (groupKey == "") return g; // if no offer reference , each element is treated as a group
                    else if (AllReferencesFromFY17.Contains(groupKey)) // common enquiry from FY17
                    {
                        // if the group contains the reference return that reference
                        // if not return null because the booking is either as "" or invalid reference
                        return g.Where(b => MinimiseRef(b.FULL_REFERENCE.Trim()) == groupKey);
                    }
                    else   // invalid series reference -> returns the first booking entered, the rest is ignored
                    {
                        return g.OrderBy(b => b.DATE_ENTERED).Take(1);
                    }
                })
                .ToList();



            // conversion rate
            List<BHDmini> EvaluatedForConversion = GroupedOffers
                .Select(b =>
                {
                    b._tooltipColChart = new TooltipConversion(b);
                    return b;
                })
                .ToList();
            dictAllPerformance.Add("Conversion", EvalAllDepartments(EvaluatedForConversion));


            // missing data
            List<BHDmissingData> AllBHDmissingDataFY17 = ExtractMissingDataFromFY17(MinimiseRef, AllBookingsReferencesFY16, AllBHDminiFY17, AllReferencesFromFY17).ToList();
            dictAllPerformance.Add("MissingData", EvalAllDepartments(AllBHDmissingDataFY17));

            // turnaround rate : Quoted enquiries and Contract enquiries

            IList<SentEnquiry> listSentEnquiries = CalculateSentEnquiries()
                .Where(se => FromDate <= se.DateSent && se.DateSent <= ToDate)
                      .Select(se =>
                      {
                          BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
                          se.SetDeadline(b, CompSpec);
                          return se;
                      })
                .ToList();


            //      Quoted enquiries
            var listGroupedOffers = GroupedOffers.Select(b => b.FULL_REFERENCE.Trim());
            List<SentEnquiry> AllQuotedEnquiriesFiltered = listSentEnquiries
                .Where(se => listGroupedOffers.Contains(se.FullReference)) // only accept the sent enquiries from groupedOffers (non dupplicates)
                .Select(se =>
                {
                    BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
                    se._tooltipColChart = new TooltipQuotedEnquiry(se, b);
                    return se;
                })
                .ToList();
            //     16/06/2017: remove potential dupplicates which slipped through the first net
            //          potential dupplicate:
            //              quoting took 1 day or less and there is an invalid or missing series reference 
            List<string> WrongSeriesReferenceBookings = AllBHDmissingDataFY17
                .Where(bmd => bmd.IncorrectSeriesReference || bmd.MissingSeriesReference)
                .Select(bmd => bmd.bhdMini.FULL_REFERENCE.Trim())
                .ToList();

            AllQuotedEnquiriesFiltered = AllQuotedEnquiriesFiltered
                .Where(se => !(se.nbDaysQuoting <= 1 && WrongSeriesReferenceBookings.Contains(se.FullReference.Trim())))
                .ToList();

            dictAllPerformance.Add("Turnaround", EvalAllDepartments(AllQuotedEnquiriesFiltered));

            //      Contract enquiries
            List<SentEnquiry> AllContractEnquiries = listSentEnquiries
                   .Select(se =>
                   {
                       BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
                       return se.SwitchToContractEvaluation(b);
                   })
                   .ToList();
            dictAllPerformance.Add("ContractEnquiries", EvalAllDepartments(AllContractEnquiries));


        }



        public async Task GeneratePerformanceSplitsAsync()
        {

            await tpRepo.GetAllConsultantsAsync();
            var listSentEnquiriesTask = CalculateSentEnquiriesAsync();



            string jsFromDate = "2017-05-01";
            string jsToDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            DateTime FromDate = DateTime.ParseExact(jsFromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime ToDate = DateTime.ParseExact(jsToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            dictAllPerformance = new Dictionary<string, object>();


            //  07/06/2017: remove the dupplicated offer for the conversion rate and the sent enquiries
            //      definition of a dupplicatedOffer : a booking is part of a series defined by its first booking entered
            //              the initial booking should remain for the evaluation , not the dupplicatedOffer
            //              the dupplicatedOffer are kept for the missing data

            //  16/06/2017:  booking references are compared by number and the branch


            List<string> validStatus = CompSpec.BookingStageCodes[bookingStage.sent]
                .Concat(CompSpec.BookingStageCodes[bookingStage.confirmed])
                .Concat(CompSpec.BookingStageCodes[bookingStage.cancelled])
                .Concat(CompSpec.BookingStageCodes[bookingStage.pending])
                .ToList();


            //  read all bookings entered in FY16
            IEnumerable<string> AllBookingsReferencesFY16 = ExtractBookingRefFY16(validStatus);





            //   concerns : 
            //      I want to avoid having bedbanks

            if (tpRepo.AllBHDminiFromFY17 == null) await tpRepo.ExtractAllBHDminiFromFY17Async();
            IEnumerable<BHDmini> AllBHDminiFY17 = tpRepo.AllBHDminiFromFY17
                .Where(b => validStatus.Contains(b.STATUS.Trim()));




            IEnumerable<string> AllReferencesFromFY17 = AllBHDminiFY17
                .Select(b => MinimiseRef(b.FULL_REFERENCE.Trim()));



            // start missing data task
            var ExtractMissingDataFromFY17Task = ExtractMissingDataFromFY17Async(MinimiseRef, AllBookingsReferencesFY16, AllBHDminiFY17, AllReferencesFromFY17);


            // grouped offers
            var GroupedOffers = AllBHDminiFY17
                .GroupBy(b => b.UDTEXT1.Trim())
                .Where(g => !AllBookingsReferencesFY16.Contains(MinimiseRef(g.Key)))  // remove the group if it belongs to FY16
                .SelectMany(g =>
                {
                    string groupKey = MinimiseRef(g.Key);
                    if (groupKey == "") return g; // if no offer reference , each element is treated as a group
                    else if (AllReferencesFromFY17.Contains(groupKey)) // common enquiry from FY17
                    {
                        // if the group contains the reference return that reference
                        // if not return null because the booking is either as "" or invalid reference
                        return g.Where(b => MinimiseRef(b.FULL_REFERENCE.Trim()) == groupKey);
                    }
                    else   // invalid series reference -> returns the first booking entered, the rest is ignored
                    {
                        return g.OrderBy(b => b.DATE_ENTERED).Take(1);
                    }
                })
                .ToList();



            // conversion rate
            List<BHDmini> EvaluatedForConversion = GroupedOffers
                .Select(b =>
                {
                    b._tooltipColChart = new TooltipConversion(b);
                    return b;
                })
                .ToList();
            dictAllPerformance.Add("Conversion", EvalAllDepartments(EvaluatedForConversion));


            // missing data
            List<BHDmissingData> AllBHDmissingDataFY17 = (await ExtractMissingDataFromFY17Task).ToList();
            dictAllPerformance.Add("MissingData", EvalAllDepartments(AllBHDmissingDataFY17));

            // turnaround rate : Quoted enquiries and Contract enquiries

            IList<SentEnquiry> listSentEnquiries = await listSentEnquiriesTask;
            //listSentEnquiries = listSentEnquiries
            //    .Where(se => FromDate <= se.DateSent && se.DateSent <= ToDate)
            //          .Select(se =>
            //          {
            //              BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
            //              se.SetDeadline(b, CompSpec);
            //              return se;
            //          })
            //    .ToList();

            //      listSentEnquiries might contains enquiries which have been entered prior to 01/05/2017  and they should be removed
            DateTime beginDt = new DateTime(2017, 5, 1);
            listSentEnquiries = listSentEnquiries
                .Where(se => FromDate <= se.DateSent && se.DateSent <= ToDate)
                .Where(se => se.Date_Entered >= beginDt)
                .Join(tpRepo.AllBHDminiFromFY17, se => se.BHD_ID, bhd => bhd.BHD_ID, (se, bhd) =>
                {
                    se.SetDeadline(bhd, CompSpec);
                    return se;
                })
                .ToList();





            //      Quoted enquiries
            var listGroupedOffers = GroupedOffers.Select(b => b.FULL_REFERENCE.Trim());
            List<SentEnquiry> AllQuotedEnquiriesFiltered = listSentEnquiries
                .Where(se => listGroupedOffers.Contains(se.FullReference)) // only accept the sent enquiries from groupedOffers (non dupplicates)
                .Select(se =>
                {
                    BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
                    se._tooltipColChart = new TooltipQuotedEnquiry(se, b);
                    return se;
                })
                .ToList();
            //     16/06/2017: remove potential dupplicates which slipped through the first net
            //          potential dupplicate:
            //              quoting took 1 day or less and there is an invalid or missing series reference 
            List<string> WrongSeriesReferenceBookings = AllBHDmissingDataFY17
                .Where(bmd => bmd.IncorrectSeriesReference || bmd.MissingSeriesReference)
                .Select(bmd => bmd.bhdMini.FULL_REFERENCE.Trim())
                .ToList();

            AllQuotedEnquiriesFiltered = AllQuotedEnquiriesFiltered
                .Where(se => !(se.nbDaysQuoting <= 1 && WrongSeriesReferenceBookings.Contains(se.FullReference.Trim())))
                .ToList();

            dictAllPerformance.Add("Turnaround", EvalAllDepartments(AllQuotedEnquiriesFiltered));

            //      Contract enquiries
            List<SentEnquiry> AllContractEnquiries = listSentEnquiries
                   .Select(se =>
                   {
                       BHDmini b = tpRepo.AllBHDminiFromFY17.FirstOrDefault(_b => _b.BHD_ID == se.BHD_ID);
                       return se.SwitchToContractEvaluation(b);
                   })
                   .ToList();
            dictAllPerformance.Add("ContractEnquiries", EvalAllDepartments(AllContractEnquiries));


        }




        private IEnumerable<string> ExtractBookingRefFY16(List<string> validStatus)
        {
            List<BHDmini> AllBookingsFromFY16 = tpRepo.ExtractAllBookingsEnteredFY16();
            // filter by valid statuses
            AllBookingsFromFY16 = AllBookingsFromFY16
                .Where(b => validStatus.Contains(b.STATUS.Trim()))
                .ToList();
            IEnumerable<string> AllBookingsReferencesFY16 = AllBookingsFromFY16
                .Select(b => MinimiseRef(b.FULL_REFERENCE.Trim()));
            return AllBookingsReferencesFY16;
        }

        public IEnumerable<BHDmissingData> ExtractMissingDataFromFY17(Func<string, string> MinimiseRef, IEnumerable<string> AllReferencesFY16, IEnumerable<BHDmini> AllBHDminiFY17, IEnumerable<string> AllReferencesFromFY17)
        {
            IEnumerable<string> validSeriesReferences = AllReferencesFY16.Concat(AllReferencesFromFY17);
            IEnumerable<BHDmissingData> AllBHDmissingDataFY17 = AllBHDminiFY17
                .Select(b =>
                {
                    var bmd = new BHDmissingData();
                    bmd.bhdMini = b;
                    bmd.SetMissingData(CompSpec, validSeriesReferences, MinimiseRef);
                    bmd._tooltipColChart = new TooltipMissingData(b, bmd);
                    return bmd;
                })
                .ToList();
            return AllBHDmissingDataFY17;
        }


        public async Task<IEnumerable<BHDmissingData>> ExtractMissingDataFromFY17Async(Func<string, string> MinimiseRef, IEnumerable<string> AllReferencesFY16, IEnumerable<BHDmini> AllBHDminiFY17, IEnumerable<string> AllReferencesFromFY17)
        {
            IEnumerable<string> validSeriesReferences = AllReferencesFY16.Concat(AllReferencesFromFY17);
            IEnumerable<BHDmissingData> AllBHDmissingDataFY17 = AllBHDminiFY17
                .Select(b =>
                {
                    var bmd = new BHDmissingData();
                    bmd.bhdMini = b;
                    bmd.SetMissingData(CompSpec, validSeriesReferences, MinimiseRef);
                    bmd._tooltipColChart = new TooltipMissingData(b, bmd);
                    return bmd;
                })
                .ToList();
            return AllBHDmissingDataFY17;
        }



        public void RetrieveDptCslRelated()
        {
            // generate a list of consultants associated with the departments

            // 01/06/2017: the consultants' name will only appear from the "Conversion"
            //          "turnaround"/"MissingData" :  contracting consultants' name can still appear due to the overlapping days on sent enquiries


            var dictDpt1 = dictAllPerformance["Turnaround"] as Dictionary<string, Dictionary<string, Dictionary<string, List<SentEnquiry>>>>;
            var dictDpt2 = dictAllPerformance["Conversion"] as Dictionary<string, Dictionary<string, Dictionary<string, List<BHDmini>>>>;
            var dictDpt3 = dictAllPerformance["MissingData"] as Dictionary<string, Dictionary<string, Dictionary<string, List<BHDmissingData>>>>;


            var t1 = dictDpt1.ToDictionary(d => d.Key,
                d => d.Value.Keys.Where(k => k.ToString() != "DepartmentOnly").ToList());
            var t2 = dictDpt2.ToDictionary(d => d.Key,
                d => d.Value.Keys.Where(k => k.ToString() != "DepartmentOnly").ToList());
            var t3 = dictDpt3.ToDictionary(d => d.Key,
                d => d.Value.Keys.Where(k => k.ToString() != "DepartmentOnly").ToList());

            dptCslAssociation = t1.Concat(t2).Concat(t3)
               .GroupBy(kvp => kvp.Key)
               .ToDictionary(g => g.Key, g => g.SelectMany(s => s.Value).Distinct());

        }

        public PendingEnquiryTableRow GenerateTableRow(BHDmini b)
        {

            PendingEnquiryTableRow tr = new PendingEnquiryTableRow();


            tr.Full_Reference = b.FULL_REFERENCE.Trim();
            tr.Booking_Name = b.NAME.Trim();
            tr.Last_Stage = CompSpec.GetCurrentStage(b.STATUS.Trim());

            //  to get the number of days of the current process:
            //    foreach BStage going backward in time
            //      if same as current stage -> add days
            //      if beyond sent stage or not pending -> exit for

            //  take into account that the last BStage will be recorded until yesterday max
            int nbDaysCurrentStage = 0;
            var listBS = compDbRepo.listBStage.Where(bs => MinimiseRef(bs.FullReference.Trim()) == MinimiseRef(b.FULL_REFERENCE.Trim()));
            foreach (BStage bs in listBS.OrderByDescending(_bs => _bs.FromDate))
            {
                string stage = CompSpec.GetCurrentStage(bs.Status.Trim());
                if (stage == tr.Last_Stage)
                {
                    // add days
                    nbDaysCurrentStage += DatesUtilities.GetNbWorkingDays(bs.FromDate, bs.ToDate);

                }
                // if not pending at all -> break for
                if (!CompSpec.BookingStageCodes[bookingStage.pending].Contains(bs.Status.Trim()))
                {
                    break;
                }
            }
            nbDaysCurrentStage++;  // to include today's working day


            // establish the number of days before deadline 
            //      if response code other than "I" -> difference between deadline and nbDaysCurrentStage
            //      if response code is "I" -> difference between today and the target date
            string responseCode = b.SALE4.Trim();
            if (responseCode == "I")
            {
                DateTime deadlineDate;
                if (b.UDTEXT4.Trim() == "")
                    deadlineDate = new DateTime(2017, 2, 1);
                else
                    deadlineDate = DateTime.Parse(b.UDTEXT4.Trim(), new System.Globalization.CultureInfo("fr-FR", true));
                tr.Days_Before_Deadline = (int)deadlineDate.Subtract(DateTime.Today).TotalDays;
            }
            else
            {
                int nbMaxDays = 0;
                if (CompSpec.dictDeadlines.ContainsKey(responseCode))
                {
                    nbMaxDays = tr.Last_Stage == "Quoting" ? CompSpec.dictDeadlines[responseCode].MaxDaysQuoting : CompSpec.dictDeadlines[responseCode].MaxDaysContracting;
                }
                tr.Days_Before_Deadline = (nbMaxDays - nbDaysCurrentStage);
            }



            //if (tpRepo.DictCsls == null) tpRepo.GetAllConsultants();
            tr.Last_Consultant = tpRepo.DictCsls[b.CONSULTANT.Trim()];
            tr.BD_Consultant = tpRepo.DictCsls[b.SALE1.Trim()];


            return tr;
        }





        private string MinimiseRef(string bigRef)
        {
            //Func<string, string> MinimiseRef = bigRef => bigRef.Length == 10 ? bigRef.Remove(2, 2).ToUpper() : bigRef;
            return bigRef.Length == 10 ? bigRef.Remove(2, 2).ToUpper() : bigRef;

        }



        public List<PendingEnquiryTableRow> GenerateDeadlineTable()
        {
            //      Extract all pending bookings from TP
            var allPending = tpRepo.ExtractQuotingContractingEnquiries(CompSpec);

            //      Extract all BStages
            if (compDbRepo.listBStage == null) compDbRepo.ExtractAllBStages();

            //          for each pending booking
            //              instantiate a new object of class PendingEnquiryTableRow
            //if (tpRepo.DictCsls == null) tpRepo.GetAllConsultants();

            var listRowsPending = allPending.Select(b => GenerateTableRow(b)).ToList();
            return listRowsPending;
        }


        private IList<SentEnquiry> generateSentEnquiriesList()
        {
            //      create "listSentEnquiries" showing a list of objects of type SentEnquiry, having the following properties:
            //              - FullReference
            //              - Consultant (full name)
            //              - Department (full name)
            //              - DateSent
            //              - nbDaysQuoting
            //              - nbDaysContracting
            //      cycle for a booking could be : QU/QR -> P/PE -> QU/QR -> Q/QF/SO -> QU/QR -> P/PE -> QU/QR -> Q/QF/SO
            //          so the evaluation will be in chronological order


            //      what happens when the booking has just been created as confirmed but does not exist yet as a BStage?
            //          the evaluation needs to start from the list of BStage



            //      11/05/2017: inclusion of the "OneDayer" : bookings which were entered on the same day as they were sent / confirmed or cancelled


            //      11/05/2017: remove the bookings which were entered prior to the 01/05/2017
            IEnumerable<IGrouping<int?, BStage>> groupedBStages = compDbRepo.GroupedBStagesEnquiries();
            //.GroupBy(bs => bs.FullReference);




            //      extract from the db the consultant name
            if (tpRepo.DictCsls == null) tpRepo.GetAllConsultants();
            Dictionary<string, string> consultNames = tpRepo.DictCsls;

            IList<SentEnquiry> listSentEnquiries = AddSentEnquiries(groupedBStages, consultNames);

            return listSentEnquiries;
        }

        private IList<SentEnquiry> AddSentEnquiries(IEnumerable<IGrouping<int?, BStage>> groupedBStages, Dictionary<string, string> consultNames)
        {
            IList<SentEnquiry> listSentEnquiries = new List<SentEnquiry>();
            foreach (var gp in groupedBStages)
            {
                // count nbDaysQuoting
                // count nbDaysContracting
                // check the date sent and add an objet sentEnquiry

                int bhdId = gp.FirstOrDefault().BHD_ID.Value;
                DateTime dateEntered = gp.OrderBy(bs => bs.FromDate)
                    .First()
                    .FromDate;
                SentEnquiry se = new SentEnquiry() { BHD_ID = bhdId, Date_Entered = dateEntered };
                bool hasQuoted = false;
                bool hasContracted = false;



                foreach (BStage bs in gp.OrderBy(bs => bs.FromDate))
                {
                    // if quoting
                    if (CompSpec.BookingStageCodes[bookingStage.pendingSalesBDOPS].Contains(bs.Status.Trim()))
                    {
                        // calculate and add the number of days
                        // add the consultant if not already there
                        se.nbDaysQuoting += DatesUtilities.GetNbWorkingDays(bs.FromDate, bs.ToDate);
                        if (!se.dictQuoting.ContainsKey(bs.Consultant.Trim()))
                        {
                            se.dictQuoting.Add(bs.Consultant.Trim(), consultNames[bs.Consultant.Trim()]);
                        }
                        hasQuoted = true;
                    }

                    // if contracting
                    if (CompSpec.BookingStageCodes[bookingStage.pendingContract].Contains(bs.Status.Trim()))
                    {
                        // calculate and add the number of days
                        // add the consultant if not already there
                        se.nbDaysContracting += DatesUtilities.GetNbWorkingDays(bs.FromDate, bs.ToDate);
                        if (!se.dictContracting.ContainsKey(bs.Consultant.Trim()))
                        {
                            se.dictContracting.Add(bs.Consultant.Trim(), consultNames[bs.Consultant.Trim()]);
                        }
                        hasContracted = true;
                    }

                    // if past the "sent" stage (not pending) and was preceded by quoting or contracting stages
                    if (!CompSpec.BookingStageCodes[bookingStage.pending].Contains(bs.Status.Trim()))
                    {

                        bool isOneDayer = bs.FromDate == dateEntered;
                        // create a new SentEnquiry object and add it to the list
                        if (hasQuoted || hasContracted || isOneDayer)
                        {
                            se.FullReference = bs.FullReference;
                            se.Department = CompSpec.RetrieveDepartment(se.FullReference);

                            //  add the consultant name if not there :
                            if (!se.dictQuoting.ContainsKey(bs.Consultant.Trim()))
                            {
                                se.dictQuoting.Add(bs.Consultant.Trim(), consultNames[bs.Consultant.Trim()]);
                            }

                            // 12/06/2017 clean dictQuoting and dictContracting of unassigned if there is at least one consultant
                            if (se.dictQuoting.Count >= 2) se.dictQuoting.Remove("");
                            if (se.dictContracting.Count >= 2) se.dictContracting.Remove("");


                            se.DateSent = bs.FromDate;
                            se.nbDaysQuoting++; // the day counts as being quoting

                            listSentEnquiries.Add(se);

                            // reset
                            se = new SentEnquiry() { BHD_ID = bhdId, Date_Entered = dateEntered };
                            hasQuoted = false;
                            hasContracted = false;
                        }
                    }

                }

            }

            return listSentEnquiries;
        }

        private async Task<IList<SentEnquiry>> generateSentEnquiriesListAsync()
        {
            //      create "listSentEnquiries" showing a list of objects of type SentEnquiry, having the following properties:
            //              - FullReference
            //              - Consultant (full name)
            //              - Department (full name)
            //              - DateSent
            //              - nbDaysQuoting
            //              - nbDaysContracting
            //      cycle for a booking could be : QU/QR -> P/PE -> QU/QR -> Q/QF/SO -> QU/QR -> P/PE -> QU/QR -> Q/QF/SO
            //          so the evaluation will be in chronological order


            //      what happens when the booking has just been created as confirmed but does not exist yet as a BStage?
            //          the evaluation needs to start from the list of BStage



            //      11/05/2017: inclusion of the "OneDayer" : bookings which were entered on the same day as they were sent / confirmed or cancelled


            //      11/05/2017: remove the bookings which were entered prior to the 01/05/2017
            IEnumerable<IGrouping<int?, BStage>> groupedBStages = compDbRepo.GroupedBStagesEnquiries();
            //.GroupBy(bs => bs.FullReference);




            //      extract from the db the consultant name
            //if (tpRepo.DictCsls == null) tpRepo.GetAllConsultants();
            //if (tpRepo.DictCsls == null) await tpRepo.GetAllConsultantsAsync();
            Dictionary<string, string> consultNames = tpRepo.DictCsls;

            IList<SentEnquiry> listSentEnquiries = AddSentEnquiries(groupedBStages, consultNames);

            return listSentEnquiries;
        }


        public void ExtractTBAsItems()
        {
            CslTBAassignment = compDbRepo.GetConsultantsTBAsLocations();
            itemsVM = tpRepo.ExtractTBAsList();

            // assign the contract consultant to the TBA location
            var dictCC = CslTBAassignment
                .Where(csl => csl.LocationsAssigned.Any())
                .ToDictionary(cc => cc.INITIALS, cc => cc.LocationsAssigned.Select(l => l.CODE));
            itemsVM = itemsVM.Select(tba =>
            {
                // find the first contract consultant who has tba.LocCode
                var _ccCode = dictCC.FirstOrDefault(kvp => kvp.Value.Contains(tba.LocCode)).Key;
                var _cc = CslTBAassignment.FirstOrDefault(_csl => _csl.INITIALS == _ccCode);
                tba._contractConsultant = new ContractConsultant
                {
                    INITIALS = _ccCode != null ? _cc.INITIALS : "",
                    NAME = _ccCode != null ? _cc.NAME : "Unassigned"
                };
                return tba;
            })
            .ToList();
        }

        public async Task ExtractTBAsItemsAsync()
        {
            var CslTBAassignmentTask = compDbRepo.GetConsultantsTBAsLocationsAsync();
            var itemsVMTask = tpRepo.ExtractTBAsListAsync();


            // assign the contract consultant to the TBA location
            CslTBAassignment = await CslTBAassignmentTask;
            var dictCC = CslTBAassignment
                .Where(csl => csl.LocationsAssigned.Any())
                .ToDictionary(cc => cc.INITIALS, cc => cc.LocationsAssigned.Select(l => l.CODE));
            //var itemsVMTask = tpRepo.ExtractTBAsListAsync();
            itemsVM = await itemsVMTask;
            itemsVM = itemsVM.Select(tba =>
            {
                // find the first contract consultant who has tba.LocCode
                var _ccCode = dictCC.FirstOrDefault(kvp => kvp.Value.Contains(tba.LocCode)).Key;
                var _cc = CslTBAassignment.FirstOrDefault(_csl => _csl.INITIALS == _ccCode);
                tba._contractConsultant = new ContractConsultant
                {
                    INITIALS = _ccCode != null ? _cc.INITIALS : "",
                    NAME = _ccCode != null ? _cc.NAME : "Unassigned"
                };
                return tba;
            })
            .ToList();
        }



        public void ApplyTBAassignmentChange(ContractConsultant cc)
        {
            HttpResponseMessage result = compDbRepo.UpdateTBAinformationInDB(cc, CslTBAassignment);
            if (result.IsSuccessStatusCode)
                CslTBAassignment = compDbRepo.GetConsultantsTBAsLocations();

        }


    }


}