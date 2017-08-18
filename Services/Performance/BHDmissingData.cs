using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Reporting_application.Services.Performance
{
    public class BHDmissingData : IPerformanceItems<BHDmissingData>
    {
        // missing data class
        public BHDmini bhdMini { get; set; }

        public bool MissingSeriesReference { get; set; } = false;
        public bool IncorrectSeriesReference { get; set; } = false;

        public bool MissingSalesUpdate { get; set; } = false;
        public bool MissingEstimatedTurnover { get; set; } = false;


        public bool AtLeastOneMissingItem { get; set; } = false;

        public TooltipMissingData _tooltipColChart { get; set; }


        [ScriptIgnore]
        public Func<CompanySpecifics, string, bool> IsDepartment { get; set; }
        [ScriptIgnore]
        public Func<CompanySpecifics, bool> IsSuccess { get; set; }
        [ScriptIgnore]
        public Func<string, bool> IsCsl { get; set; }
        [ScriptIgnore]
        public Func<List<BHDmissingData>, List<string>> GetAllCslFromListOfItems { get; set; }


        public BHDmissingData()
        {
            IsDepartment = (cs, dpt) => cs.RetrieveDepartment(bhdMini.FULL_REFERENCE) == dpt;
            IsSuccess = cs => !AtLeastOneMissingItem;
            IsCsl = csl => bhdMini.SALE1.Trim() == csl;
            GetAllCslFromListOfItems = _listItems => _listItems.Select(t => t.bhdMini.SALE1.Trim()).Distinct().ToList();
        }


        public void SetMissingData(CompanySpecifics cs, IEnumerable<string> validSeriesReferences, Func<string, string> MinimiseRef)
        {


            if (cs.bookingTypes["Brochure"].Contains(bhdMini.SALE6.Trim()))
            {
                // brochure

                // Missing series reference
                if (bhdMini.UDTEXT1.Trim() == "")
                {
                    MissingSeriesReference = true;
                    AtLeastOneMissingItem = true;
                }

                // Incorrect series reference
                if (bhdMini.UDTEXT1.Trim() != "")
                {
                    //string refPattern = @"^[A-Z]{4}\d{6}$";
                    //if (!Regex.IsMatch(bhdMini.UDTEXT1.Trim(), refPattern))
                    //{
                    //    IncorrectSeriesReference = true;
                    //    AtLeastOneMissingItem = true;
                    //}
                    if (!validSeriesReferences.Contains(MinimiseRef(bhdMini.UDTEXT1.Trim())))
                    {
                        IncorrectSeriesReference = true;
                        AtLeastOneMissingItem = true;
                    }
                }

            }

            // Missing Sales Update
            //      22/06/2017 : missing Sales update is only for bookings which are not pending
            var pendingStatuses = cs.BookingStageCodes[bookingStage.pending];
            var bStatus = bhdMini.STATUS.Trim();
            if (bhdMini.UDTEXT5.Trim() == "" && !pendingStatuses.Contains(bStatus))
            {
                // only if it is not FIT department
                if (cs.RetrieveDepartment(bhdMini.FULL_REFERENCE.Trim()) != "FIT")
                {
                    MissingSalesUpdate = true;
                    AtLeastOneMissingItem = true;
                }
            }

            // Missing estimated turnover
            if (!cs.BookingStageCodes[bookingStage.pending].Contains(bhdMini.STATUS.Trim()))
            {
                // not pending
                if (bhdMini.UDTEXT3.Trim() == "")
                {
                    MissingEstimatedTurnover = true;
                    AtLeastOneMissingItem = true;
                }
            }


        }


    }

}
