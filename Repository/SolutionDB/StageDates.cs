using ERAwebAPI.ModelsDB;
using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reporting_application.Repository.ERADB
{
    public class StagesDates
    {
        public DateTime? DateEntered { get; }
        public DateTime? DateSent { get; }
        public DateTime? DateConfirmed { get; }
        public DateTime? DateCancelled { get; }
        public int BHD_ID { get; }

        public StagesDates(IEnumerable<BStage> iBStage, CompanySpecifics cs)
        {
            //              get the first stage which is beyond sent ( sent , confirmed , cancelled) -> DateSent
            //              get the first stage which is beyond confirmed ( confirmed , cancelled)-> DateConfirmed
            //              get the first stage which is cancelled ( cancelled)-> DateCancelled

            iBStage = iBStage.OrderBy(bs => bs.FromDate);

            var firstSent = iBStage.FirstOrDefault(bs =>
            cs.IsSent(bs.Status) || cs.IsConfirmed(bs.Status) || cs.IsCancelled(bs.Status));
            if (firstSent != null) DateSent = firstSent.FromDate;

            var firstConf = iBStage.FirstOrDefault(bs => cs.IsConfirmed(bs.Status) || cs.IsCancelled(bs.Status));
            if (firstConf != null) DateConfirmed = firstConf.FromDate;

            var firstCanc = iBStage.FirstOrDefault(bs => cs.IsCancelled(bs.Status));
            if (firstCanc != null) DateCancelled = firstCanc.FromDate;

            BHD_ID = (iBStage.FirstOrDefault().BHD_ID).Value;

            DateEntered = iBStage.FirstOrDefault().FromDate;

        }

        public DateTime GetLastStageDate()
        {
            return (DateCancelled ?? DateConfirmed ?? DateSent ?? DateEntered).Value;
        }


    }
}
