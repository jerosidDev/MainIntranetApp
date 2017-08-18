using ERAwebAPI.ModelsDB;
using System.Web.Script.Serialization;

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
        public ContractConsultant _contractConsultant { get; set; }

    }

}
