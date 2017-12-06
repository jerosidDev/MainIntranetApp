using Reporting_application.Models;
using Reporting_application.Services.Performance;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Reporting_application.Repository.ThirdpartyDB
{
    public class BHDmini : IPerformanceItems<BHDmini>
    {
        // version of BHD with less information
        //      if I need to add a property the name should be exactly the same as the original


        public string NAME { get; set; }
        public System.DateTime TRAVELDATE { get; set; }
        public string AGENT { get; set; }
        public string STATUS { get; set; }
        public string CONSULTANT { get; set; }
        public string UDTEXT1 { get; set; }
        public string UDTEXT2 { get; set; }
        public string UDTEXT3 { get; set; }
        public string UDTEXT4 { get; set; }
        public string UDTEXT5 { get; set; }
        public string SALE1 { get; set; }
        public string SALE2 { get; set; }
        public string SALE3 { get; set; }
        public string BOOKING_TYPE { get; set; }
        public System.DateTime DATE_ENTERED { get; set; }
        public string FULL_REFERENCE { get; set; }
        public string SALE4 { get; set; }
        public string SALE5 { get; set; }
        public string SALE6 { get; set; }
        public int BHD_ID { get; set; }

        [XmlIgnore]
        public TooltipConversion _tooltipColChart { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public Func<CompanySpecifics, string, bool> IsDepartment { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public Func<CompanySpecifics, bool> IsSuccess { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public Func<string, bool> IsCsl { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public Func<List<BHDmini>, List<string>> GetAllCslFromListOfItems { get; set; }


        public BHDmini(BHD _bhd, Dictionary<string, string> Sa1ToCsl)
        {
            NAME = _bhd.NAME;
            TRAVELDATE = _bhd.TRAVELDATE;
            AGENT = _bhd.AGENT;
            STATUS = _bhd.STATUS;
            CONSULTANT = _bhd.CONSULTANT;
            UDTEXT1 = _bhd.UDTEXT1;
            UDTEXT2 = _bhd.UDTEXT2;
            UDTEXT3 = _bhd.UDTEXT3;
            UDTEXT4 = _bhd.UDTEXT4;
            UDTEXT5 = _bhd.UDTEXT5;
            SALE1 = Sa1ToCsl[_bhd.SALE1.Trim()];
            SALE2 = _bhd.SALE2;
            SALE3 = _bhd.SALE3;
            BOOKING_TYPE = _bhd.BOOKING_TYPE;
            DATE_ENTERED = _bhd.DATE_ENTERED;
            FULL_REFERENCE = _bhd.FULL_REFERENCE;
            SALE4 = _bhd.SALE4;
            SALE5 = _bhd.SALE5;
            SALE6 = _bhd.SALE6;
            BHD_ID = _bhd.BHD_ID;

            SetPerformanceItems();
        }

        public BHDmini()
        {
            SetPerformanceItems();
        }

        private void SetPerformanceItems()
        {
            IsDepartment = (cs, dpt) => cs.RetrieveDepartment(FULL_REFERENCE) == dpt;
            IsSuccess = cs => !cs.IsBHDunconfirmed(this);
            IsCsl = csl => SALE1.Trim() == csl;
            GetAllCslFromListOfItems = _listItems => _listItems.Select(t => t.SALE1.Trim()).Distinct().ToList();
        }


    }

}