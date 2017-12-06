using CompanyDbWebAPI.ModelsDB;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Reporting_application.Services.Performance
{
    public class ContractTBAsInfo
    {
        public string Service_Date { get; set; }
        public string FULL_REFERENCE { get; set; }
        public string Booking_Name { get; set; }
        public string Supplier_Name { get; set; }
        public string Location_Name { get; set; }
        public string Option_Name { get; set; }
        [ScriptIgnore]
        public int BSL_ID { get; set; }
        [ScriptIgnore]
        public string LocCode { get; set; }
        [XmlIgnore]
        public ContractConsultant _contractConsultant { get; set; }

    }

}