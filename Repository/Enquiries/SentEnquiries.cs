using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Reporting_application.Utilities.Performance
{
    public class SentEnquiry : IPerformanceItems<SentEnquiry>
    {

        public string FullReference { get; set; }

        public Dictionary<string, string> dictQuoting { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> dictContracting { get; set; } = new Dictionary<string, string>();

        public string Department { get; set; }

        public DateTime DateSent { get; set; }

        public int nbDaysQuoting { get; set; } = 0;
        public int maxNbDaysQuoting { get; set; } = 1;  //  to be extracted from BHD

        public int nbDaysContracting { get; set; } = 0;
        public int maxNbDayContracting { get; set; } = 0;   // to be extracted from BHD


        public TooltipSentEnquiry _tooltipColChart { get; set; }



        [ScriptIgnore]
        public Func<CompanySpecifics, string, bool> IsDepartment { get; set; }
        [ScriptIgnore]
        public Func<CompanySpecifics, bool> IsSuccess { get; set; }
        [ScriptIgnore]
        public Func<string, bool> IsCsl { get; set; }
        [ScriptIgnore]
        public Func<List<SentEnquiry>, List<string>> GetAllCslFromListOfItems { get; set; }


        public SentEnquiry()
        {
            IsDepartment = (cs, dpt) => Department == dpt;
            IsSuccess = cs => nbDaysQuoting <= maxNbDaysQuoting;
            IsCsl = csl => dictQuoting.ContainsKey(csl);
            GetAllCslFromListOfItems = _listItems => _listItems.SelectMany(t => t.dictQuoting.Keys).Distinct().ToList();
        }


        public void SetDeadline(BHDmini b, CompanySpecifics CompSpec)
        {
            string tpCode = b.SALE4.Trim();
            if (CompSpec.dictDeadlines.ContainsKey(tpCode))
            {
                maxNbDaysQuoting = CompSpec.dictDeadlines[tpCode].MaxDaysQuoting;
                maxNbDayContracting = CompSpec.dictDeadlines[tpCode].MaxDaysContracting;
            }
            else if (tpCode == "I")
            {
                // Several dates placed offer
                if (b.UDTEXT4.Trim() != "")
                {
                    DateTime deadlineDate = DateTime.Parse(b.UDTEXT4.Trim(),
                        new System.Globalization.CultureInfo("fr-FR", true));
                    DateTime dtSent = DateSent;
                    if (dtSent <= deadlineDate)
                    {
                        maxNbDaysQuoting = 9999;
                        maxNbDayContracting = 9999;
                    }
                    else
                    {
                        maxNbDaysQuoting = -1;
                        maxNbDayContracting = -1;
                    }
                }
            }
        }



        public SentEnquiry SwitchToContractEvaluation(BHDmini b)
        {
            // create a clone of se
            SentEnquiry seCloned = this.MemberwiseClone() as SentEnquiry;
            seCloned.IsDepartment = (cs, dpt) => true;
            seCloned.IsSuccess = cs => nbDaysContracting <= maxNbDayContracting;
            seCloned.IsCsl = csl => dictContracting.ContainsKey(csl);
            seCloned.GetAllCslFromListOfItems = _listItems => _listItems.SelectMany(t => t.dictContracting.Keys).Distinct().ToList();


            seCloned._tooltipColChart = new TooltipContractEnquiry(this, b);
            return seCloned;
        }


    }


}
