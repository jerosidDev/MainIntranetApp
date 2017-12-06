using System;

namespace Reporting_application.Services.SuppliersAnalysis
{
    public class SuppliersAnalysisData
    {
        public int BSL_ID { get; set; }
        public int OPT_ID { get; set; }
        public DateTime DATE { get; set; }

        public int BHD_ID { get; set; }
        public string FULL_REFERENCE { get; set; }
        public string DEPARTMENT { get; set; }
        public string BookingStatus { get; set; }


        public string SUPPLIER_CODE { get; set; }
        public string SUPPLIER_NAME { get; set; }


        public string CHAIN_CODE { get; set; }
        public string CHAIN_NAME { get; set; }


        public double COST { get; set; }
        public string CURRENCY { get; set; }
        public double BASE_DIV_RATE { get; set; }
        public double BASE_MUT_RATE { get; set; }
        public string SL_STATUS { get; set; }
        public double Base_Cost { get; set; }
        public int ROOMCOUNT { get; set; }
        public int SCU_FOC { get; set; }
        public int SCU_QTY { get; set; }
        public int Rooms_Nights { get; set; }

        public string Service_Type_Code { get; set; }
        public string Service_Type_Name { get; set; }


        public string LOCATION_CODE { get; set; }
        public string LOCATION_NAME { get; set; }


        public string BDConsultant_CODE { get; set; }
        public string BDConsultant_NAME { get; set; }


        public string Client_CODE { get; set; }
        public string Client_NAME { get; set; }



    }
}