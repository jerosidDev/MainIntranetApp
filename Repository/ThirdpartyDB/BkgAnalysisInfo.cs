using System;

namespace Reporting_application.Repository.ThirdpartyDB
{
    public class BkgAnalysisInfo
    {

        public BkgAnalysisInfo()
        {
        }


        public string Full_Reference { get; internal set; }

        public string AgentName { get; internal set; }
        public string AgentChain { get; set; }

        public string EstimatedTurnover { get; internal set; }
        public DateTime DateEntered { get; internal set; }
        public DateTime DateConfirmed { get; internal set; }
        public string DEPARTMENT { get; internal set; }
        public string LocationName { get; internal set; }
        public string BkgType { get; internal set; }
        public string BookingStatus { get; internal set; }
        public string ClientCategory { get; internal set; }
        public string BDConsultant { get; internal set; }
        public string Consultant { get; internal set; }
        public string CompanyDepartment { get; internal set; }
        public string PendingArea { get; internal set; }

        public bool PastDeadline { get; internal set; }

        public DateTime TravelDate { get; internal set; }
    }
}