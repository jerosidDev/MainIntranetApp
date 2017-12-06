using Reporting_application.Repository;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Reporting_application.Services.SuppliersAnalysis
{
    public class SuppliersAnalysis : ISuppliersAnalysis
    {

        public IThirdpartyDBrepository tpRepo { get; set; }


        public List<SuppliersAnalysisData> supplierAnalysisData { get; set; }
        public Dictionary<string, object> DataToBeJSONed { get; set; }

        private List<FilterItem> filterItems;
        private List<EvalItem> evalItems;
        private List<TimeframeEval> timelineProjectedMonthly;
        private List<TimeframeEval> timelineProjectedWeekly;
        private List<FYItem> FYitems;
        private List<BookingStageItem> bookingStageItems;
        private Dictionary<DateTime, Dictionary<string, Dictionary<string, IEnumerable<SuppliersAnalysisData>>>> AllDataUnderlying;

        public SuppliersAnalysis(IThirdpartyDBrepository _tpRepo)
        {
            tpRepo = _tpRepo;
        }


        public async Task InitialLoadingAsync(int currentFY)
        {
            GenerateBookingStageItems();
            supplierAnalysisData = await tpRepo.InitialLoadingSuppliersAnalysisAsync(currentFY, bookingStageItems.FirstOrDefault(bsi => bsi.Name == "All").Statuses);

            filterItems = GenerateFilterItems();
            evalItems = GenerateEvalItems();
            GenerateTimelines(currentFY);
            GenerateFYitems();

        }


        public Dictionary<string, string> GetEvalItemsForTheView()
        {
            return evalItems.ToDictionary(ei => ei.Code, ei => ei.DisplayedName);
        }

        private List<FilterItem> GenerateFilterItems()
        {
            var returnedList = new List<FilterItem>();

            // by service types
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Service_types",
                SortbyCode = sad => sad.Service_Type_Code,
                displayName = sad => sad.Service_Type_Name
            });




            // by chains
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Chains",
                SortbyCode = sad => sad.CHAIN_CODE,
                displayName = sad => sad.CHAIN_NAME
            });




            // by suppliers
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Suppliers",
                SortbyCode = sad => sad.SUPPLIER_CODE,
                displayName = sad => sad.SUPPLIER_NAME
            });





            // by departments
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Departments",
                SortbyCode = sad => sad.DEPARTMENT,
                displayName = sad => sad.DEPARTMENT
            });




            // by clients
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Clients",
                SortbyCode = sad => sad.Client_CODE,
                displayName = sad => sad.Client_NAME
            });





            // by locations
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "Locations",
                SortbyCode = sad => sad.LOCATION_CODE,
                displayName = sad => sad.LOCATION_NAME
            });




            // by BD consultants
            returnedList.Add(new FilterItem()
            {
                Filter_Name = "BD_Consultants",
                SortbyCode = sad => sad.BDConsultant_CODE,
                displayName = sad => sad.BDConsultant_NAME
            });




            return returnedList;
        }



        public Dictionary<string, Dictionary<string, string>> GenerateFilterValuesForDropDowns(List<SuppliersAnalysisData> _sadList)
        {
            var FilterReturned = filterItems.ToDictionary(fi => fi.Filter_Name, fi =>
               GeneratePairsDictionary(_sadList, sad => fi.SortbyCode(sad),
               sad => fi.displayName(sad)));

            return FilterReturned;

        }



        private Dictionary<string, string> GeneratePairsDictionary(List<SuppliersAnalysisData> _supplierAnalysisData, Func<SuppliersAnalysisData, string> funcSelectedKey, Func<SuppliersAnalysisData, string> funcSelectedValue)
        {
            var ret = _supplierAnalysisData
                .Select(sad => funcSelectedKey(sad))
                .Distinct().ToList()
                .ToDictionary(s => s,
                s => funcSelectedValue(_supplierAnalysisData.FirstOrDefault(sad => funcSelectedKey(sad) == s)))
                .OrderBy(kvp => kvp.Value.Trim())
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return ret;
        }

        private List<EvalItem> GenerateEvalItems()
        {

            // 3 types of evaluation:
            //      cost in £
            //      number of service lines
            //      room_nights

            List<EvalItem> ret = new List<EvalItem>();



            ret.Add(new EvalItem()
            {
                DisplayedName = "Cost in Pounds",
                Code = "evalCost",
                Evaluator = sads => sads.Count == 0 ? 0 : (int)(sads.Select(sad => sad.Base_Cost).Aggregate((bc1, bc2) => bc1 + bc2))
            });

            ret.Add(new EvalItem()
            {
                DisplayedName = "Number of service lines",
                Code = "evalNumberSL",
                Evaluator = sads => sads.Count
            });

            ret.Add(new EvalItem()
            {
                DisplayedName = "Number of Room_Nights",
                Code = "evalNumberRN",
                Evaluator = sads => sads.Count == 0 ? 0 : sads.Select(sad => sad.Rooms_Nights).Aggregate((rn1, rn2) => rn1 + rn2)
            });

            return ret;


        }


        public List<SuppliersAnalysisData> FilterDataBasedOnViewSelection(NameValueCollection RequestForm)
        {
            var sadListFiltered = supplierAnalysisData.Where(sad =>
            {

                var pass2 = filterItems.Select(fi =>
               {

                   var selectedValue = RequestForm[fi.Filter_Name];

                   return selectedValue == null || selectedValue == "All" || fi.SortbyCode(sad) == selectedValue;


               }).ToList();

                return pass2.TrueForAll(p => p);

            }).ToList();


            return sadListFiltered;
        }


        public void EvaluateDataFromUserSelection(NameValueCollection RequestForm)
        {

            // for test: 
            if (RequestForm.Count == 0)
            {
                RequestForm = new NameValueCollection();
                RequestForm["EvaluationType"] = "evalCost";
                RequestForm["Service_types"] = "All";
                RequestForm["Chains"] = "All";
                RequestForm["Suppliers"] = "All";
                RequestForm["Departments"] = "All";
                RequestForm["Clients"] = "All";
                RequestForm["Locations"] = "All";
                RequestForm["BD_Consultants"] = "All";
                RequestForm["All"] = "false";
                RequestForm["Unconfirmed"] = "true";
                RequestForm["Confirmed"] = "true";
                RequestForm["Cancelled"] = "true";
                RequestForm["currentFY"] = "true";
                RequestForm["previousFY"] = "true";
                RequestForm["nextFY"] = "true";
                RequestForm["radioMonthly"] = "true";
                RequestForm["radioWeekly"] = "false";
            }

            // Filter the data using the narrowBy

            var sadFiltered = FilterDataBasedOnViewSelection(RequestForm);



            List<TimeframeEval> timeLineProjected = RequestForm["radioMonthly"] == "true" ? timelineProjectedMonthly : timelineProjectedWeekly;



            // get the common data
            AllDataUnderlying = timeLineProjected.ToDictionary(tf => tf.startDate, tf =>
            {
                // for each financial year selected
                return FYitems.Where(fy => RequestForm[fy.Code] == "true").ToDictionary(fy2 => fy2.Code, fy2 =>
                 {
                     // for each booking stages selected
                     return bookingStageItems.Where(bsi => RequestForm[bsi.Name] == "true").ToDictionary(bsi2 => bsi2.Name, bsi2 =>
                      {
                          var sadGroup = sadFiltered
                          .Where(sad => fy2.validateFY(sad.DATE, tf))
                          .Where(sad => bsi2.IsStage(sad.BookingStatus));
                          return sadGroup;
                      });

                 }
                     );

            });


            // calculate the data for the column chart : Period_Only
            var evaluator = evalItems.FirstOrDefault(e => e.Code == RequestForm["EvaluationType"]).Evaluator;

            var DataEvaluated = AllDataUnderlying
                .ToDictionary(kvpByDate => kvpByDate.Key,
                kvpByDate => kvpByDate.Value
                .ToDictionary(kvpByFY => kvpByFY.Key, kvpByFY => kvpByFY.Value
                .ToDictionary(kvpByBookingStage => kvpByBookingStage.Key, kvpByBookingStage => evaluator(kvpByBookingStage.Value.ToList()))));


            // calculate the data for the line chart: Cumulative
            var dataCumulative = DataEvaluated
                .ToDictionary(kvpByDate => kvpByDate.Key, kvpByDate => kvpByDate.Value.ToDictionary(kvpByFY => kvpByFY.Key, kvpByFY => kvpByFY.Value.ToDictionary(kvpByBookingStage => kvpByBookingStage.Key, kvpByBookingStage =>
                {
                    // get the sub dictionary until kvpByDate included

                    var cumulativeVal = DataEvaluated
                    .Where(kvpSub => kvpSub.Key <= kvpByDate.Key)
                    .Select(kvpSub2 => kvpSub2.Value[kvpByFY.Key][kvpByBookingStage.Key])
                    .Sum();


                    return cumulativeVal;
                })));



            // calculate the variation table for suppliers and master suppliers
            var suppliersVariation = GenerateVariationTableByKey(timeLineProjected, sadFiltered, evaluator, false);
            var sadFilteredChainsOnly = sadFiltered.Where(sad => sad.CHAIN_NAME != "No chain").ToList();
            var chainsVariation = GenerateVariationTableByKey(timeLineProjected, sadFilteredChainsOnly, evaluator, true);


            var fullVariationTable = chainsVariation.Union(suppliersVariation);

            // order by Total evalType previous FY + current FY 
            var fullVariationTable2 = fullVariationTable
                .OrderByDescending(supp => supp.Value.Select(fy => fy.Value.Total_Evaluated).Sum())
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .Take(300);





            DataToBeJSONed = new Dictionary<string, object>();
            DataToBeJSONed["Period_Only"] = DataEvaluated.ToDictionary(pOnly => pOnly.Key.ToString("yyyy-MM-dd"), pOnly => pOnly.Value);
            DataToBeJSONed["Cumulative"] = dataCumulative.ToDictionary(cumu => cumu.Key.ToString("yyyy-MM-dd"), cumu => cumu.Value);
            DataToBeJSONed["Variations_Table"] = fullVariationTable2;


        }

        private Dictionary<string, Dictionary<string, VariationTableItem>> GenerateVariationTableByKey(List<TimeframeEval> timeLineProjected, List<SuppliersAnalysisData> sadFiltered, Func<List<SuppliersAnalysisData>, int> evaluator, bool ChainEvaluator)
        {

            // calculate the data for the variation table : 1 process for the suppliers , 1 process for the chains


            string addStrChain = ChainEvaluator ? "Chain: " : "";

            Func<SuppliersAnalysisData, string> keyEval;
            Func<SuppliersAnalysisData, string> GetFullName;
            if (ChainEvaluator)
            {
                keyEval = sad => sad.CHAIN_CODE;
                GetFullName = sad => sad.CHAIN_NAME;
            }
            else
            {
                keyEval = sad => sad.SUPPLIER_CODE;
                GetFullName = sad => sad.SUPPLIER_NAME;
            }


            var bsiConfirmed = bookingStageItems.FirstOrDefault(bsi => bsi.Name == "Confirmed");
            var bsiCancelled = bookingStageItems.FirstOrDefault(bsi => bsi.Name == "Cancelled");




            //      get the timeframe of the comparison evaluation
            var ComparisonTF = new TimeframeEval()
            {
                startDate = timeLineProjected.FirstOrDefault().startDate,
                endDate = timeLineProjected.LastOrDefault(tf => tf.startDate <= DateTime.Today).endDate
            };


            //      group by suppliers or master suppliers
            var variationsTable = sadFiltered.GroupBy(sad => keyEval(sad));


            //      split between previous fy and current fy
            var variationsTable2 = variationsTable
                .ToDictionary(g => g.Key, g => g.ToList());

            var variationsTable3 = variationsTable2
              .ToDictionary(g => g.Key, g =>
              {
                  return FYitems.ToDictionary(fy => fy.Code, fy => g.Value.Where(sad => fy.validateFY(sad.DATE, ComparisonTF)));
              });


            // "Chain:_" is added to the chain code to allow for the dictionary union in the calling method

            var variationsTable4 = variationsTable3
                .ToDictionary(supp => addStrChain + supp.Key,
                supp =>
                {
                    var firstSad = supplierAnalysisData.FirstOrDefault(sad => keyEval(sad) == supp.Key);
                    string supplierOrChainName = addStrChain + GetFullName(firstSad);

                    return supp.Value.ToDictionary(
                      fy => fy.Key, fy => new VariationTableItem
                      {
                          Total_Evaluated = evaluator(fy.Value.ToList()),
                          nb_Bookings_Confirmed = fy.Value
                          .Where(sad => bsiConfirmed.IsStage(sad.BookingStatus))
                          .Select(sad => sad.BHD_ID)
                          .Distinct()
                          .Count(),
                          nb_Bookings_Cancelled = fy.Value
                          .Where(sad => bsiCancelled.IsStage(sad.BookingStatus))
                          .Select(sad => sad.BHD_ID)
                          .Distinct()
                          .Count(),
                          SupplierOrChainName = supplierOrChainName
                      });


                }

                   );




            return variationsTable4;

        }

        private void GenerateTimelines(int currentFY)
        {
            //var currentFY = DatesUtilities.GetCurrentFinancialYear();


            DateTime beginDT = new DateTime(currentFY, 2, 1);
            DateTime endDT = new DateTime(currentFY + 1, 1, 31);



            // generate the monthly timeline
            timelineProjectedMonthly = new List<TimeframeEval>();
            for (DateTime dt = beginDT; dt <= endDT; dt = dt.AddMonths(1))
            {

                DateTime lastDayOfMonth = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

                timelineProjectedMonthly.Add(new TimeframeEval() { startDate = dt, endDate = lastDayOfMonth });

            }



            // generate the weekly timeline
            timelineProjectedWeekly = new List<TimeframeEval>();

            //  find the first Sunday of the current FY and add the first timeframe element which is truncated
            var aSunday = beginDT.AddDays(7 - (int)beginDT.DayOfWeek);
            timelineProjectedWeekly.Add(new TimeframeEval() { startDate = beginDT, endDate = aSunday });

            for (DateTime dt = aSunday.AddDays(1); dt <= endDT; dt = dt.AddDays(7))
            {
                timelineProjectedWeekly.Add(new TimeframeEval() { startDate = dt, endDate = dt.AddDays(6) });
            }
        }


        private void GenerateFYitems()
        {
            // generate Financial year items


            FYitems = new List<FYItem>();
            FYitems.Add(new FYItem()
            {
                DisplayedName = "Current",
                DisplayedNameInFull = "Current financial year",
                Code = "currentFY",
                validateFY = (dt, tf) => dt >= tf.startDate && dt <= tf.endDate
            });

            FYitems.Add(new FYItem()
            {
                DisplayedName = "Previous",
                DisplayedNameInFull = "Previous financial year",
                Code = "previousFY",
                validateFY = (dt, tf) => dt.AddYears(1) >= tf.startDate && dt.AddYears(1) <= tf.endDate
            });

            FYitems.Add(new FYItem()
            {
                DisplayedName = "Next",
                DisplayedNameInFull = "Next financial year",
                Code = "nextFY",
                validateFY = (dt, tf) => dt.AddYears(-1) >= tf.startDate && dt.AddYears(-1) <= tf.endDate
            });


        }

        private void GenerateBookingStageItems()
        {
            var cs = new CompanySpecifics();
            bookingStageItems = cs.LoadBookingStageItems();

            // exceptionally for this service the bedbanks are added because they contain a valid cost/room_nights
            bookingStageItems = bookingStageItems.Select(bsi =>
            {
                if (bsi.Name == "All" || bsi.Name == "Unconfirmed")
                    bsi.Statuses.AddRange(cs.BookingStageCodes[bookingStage.Bedbank]);

                return bsi;
            }).ToList();

        }


        public Dictionary<string, string> GetFYsForTheView()
        {
            return FYitems.ToDictionary(fy => fy.Code, fy => fy.DisplayedName);
        }



        public Dictionary<string, List<string>> GetBookingStatusesForTheView()
        {

            return bookingStageItems.ToDictionary(bsi => bsi.Name, bsi => bsi.Statuses);
        }


        public object GetDataToBeExported(DateTime dateRequested)
        {
            var dataRequested = AllDataUnderlying[dateRequested].ToDictionary(fy => FYitems.FirstOrDefault(fy2 => fy2.Code == fy.Key).DisplayedNameInFull.Replace(' ', '_'), fy => fy.Value.ToDictionary(bs => (bs.Key + " bookings").Replace(' ', '_'), bs => bs.Value.Select(
               sad => new
               {
                   Service_Date = sad.DATE.ToString("dd/MM/yyyy"),
                   Full_Reference = sad.FULL_REFERENCE,
                   Department = sad.DEPARTMENT,
                   Booking_Status = sad.BookingStatus,
                   Supplier_Code = sad.SUPPLIER_CODE,
                   Supplier_Name = sad.SUPPLIER_NAME,
                   Chain_Code = sad.CHAIN_CODE,
                   Chain_Name = sad.CHAIN_NAME,
                   GBP_Base_Cost = sad.Base_Cost,
                   Room_Nights = sad.Rooms_Nights,
                   Service_Code = sad.Service_Type_Code,
                   Service_Name = sad.Service_Type_Name,
                   Location_Code = sad.LOCATION_CODE,
                   Location_Name = sad.LOCATION_NAME,
                   BD_Consultant_Initials = sad.BDConsultant_CODE,
                   BD_Consultant_Name = sad.BDConsultant_NAME,
                   Client_Code = sad.Client_CODE,
                   Client_Name = sad.Client_NAME
               })));

            return dataRequested;
        }


        private class TimeframeEval
        {
            public DateTime startDate;
            public DateTime endDate;
        }


        private class FYItem
        {
            // a financial year item

            public string DisplayedName { get; set; }
            public string DisplayedNameInFull { get; set; }
            public string Code { get; set; }
            public Func<DateTime, TimeframeEval, bool> validateFY { get; set; }
        }


        private class FilterItem
        {
            public string Filter_Name { get; set; }
            public Func<SuppliersAnalysisData, string> SortbyCode { get; set; }
            public Func<SuppliersAnalysisData, string> displayName { get; set; }

        }


        private class EvalItem
        {

            public string Code { get; set; }
            public string DisplayedName { get; set; }
            public Func<List<SuppliersAnalysisData>, int> Evaluator { get; set; }

        }

        public class VariationTableItem
        {

            public string SupplierOrChainName { get; set; }
            public int Total_Evaluated { get; set; }
            public int nb_Bookings_Confirmed { get; set; }
            public int nb_Bookings_Cancelled { get; set; }
        }


    }
}