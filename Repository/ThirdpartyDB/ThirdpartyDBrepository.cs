using Reporting_application.Models;
using Reporting_application.Repository.ERADB;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using Reporting_application.Utilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Reporting_application.Repository
{
    public class ThirdpartyDBrepository : IThirdpartyDBrepository
    {
        public List<BHDmini> AllBookingsFromFY17 { get; set; }


        private Dictionary<string, string> _dictCsls;
        public Dictionary<string, string> DictCsls
        {
            get
            {
                if (_dictCsls == null)
                    GetAllConsultants();
                return _dictCsls;
            }
            set
            {
                _dictCsls = value;
            }
        }


        private Dictionary<string, string> _dictSA1;
        public Dictionary<string, string> DictSA1
        {
            get
            {
                if (_dictSA1 == null)
                    GetAllConsultants();
                return _dictSA1;
            }
            set
            {
                _dictSA1 = value;
            }
        }


        public IThirdpartyDBContext tpContext3 { get; set; }


        public ThirdpartyDBrepository(IThirdpartyDBContext _tpContext3)
        {
            tpContext3 = _tpContext3;
        }


        public virtual IDictionary<string, string> GetRecentlyActiveConsultants()
        {

            DateTime beginDt = DateTime.Today.AddMonths(-4);

            //var dictAllCsl = te.CSLs.ToDictionary(csl => csl.INITIALS.Trim(), csl => csl.NAME.Trim());

            var listRecentBhdMini = AllBookingsFromFY17.Where(b => b.DATE_ENTERED >= beginDt);

            var listCsl = listRecentBhdMini.Select(b => b.CONSULTANT.Trim());
            var listBdCsl = listRecentBhdMini.Select(b => b.SALE1.Trim());


            return listCsl.Concat(listBdCsl).Distinct().ToDictionary(csl => csl, csl => DictCsls[csl]);

            //}

        }

        public virtual void GetAllConsultants()
        {
            DictCsls = tpContext3.CSLs.ToDictionary(c => c.INITIALS.Trim(), c => c.NAME.Trim());
            DictSA1 = tpContext3.SA1.ToDictionary(sa1 => sa1.CODE.Trim(), sa1 => sa1.DESCRIPTION.Trim());
            //}
        }


        public virtual void ExtractAllBookingsFromFY17()
        {
            //      extract all bookings from FY17 

            // 12/06/2017 : SALE1 property needs to be corrected to match the CSL table in ThirdparyDB


            Dictionary<string, string> Sa1ToCsl = GenerateUniqueCslList();

            DateTime beginDate = new DateTime(2017, 02, 01);
            AllBookingsFromFY17 = tpContext3.BHDs
                .Where(b => b.DATE_ENTERED >= beginDate)
                .ToList()
                .Select(_bhd => new BHDmini(_bhd, Sa1ToCsl))
                .ToList();
            //}

        }

        public Dictionary<string, string> GenerateUniqueCslList()
        {
            //var cslTable = te.CSLs
            //    .ToDictionary(csl => csl.INITIALS.Trim(), csl => csl.NAME.Trim());
            //var bdCslTable = te.SA1
            //    .ToDictionary(bdCsl => bdCsl.CODE.Trim(), bdCsl => bdCsl.DESCRIPTION.Trim());

            // compare from the 2 tables CSL and SA1 in order to remove dupplicate consultants created by diacritics differences

            var Sa1ToCsl = DictSA1
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    string BDconsultName = TextUtilities.RemoveDiacritics(kvp.Value);

                    var kvpConsult = DictCsls.FirstOrDefault(kvp2 => TextUtilities.RemoveDiacritics(kvp2.Value) == BDconsultName);
                    // if found then return kvp2.key else return its own value
                    return kvpConsult.Key != null ? kvpConsult.Key : kvp.Key;
                });
            return Sa1ToCsl;
        }

        public virtual List<BHDmini> ExtractAllBookingsEnteredFY16()
        {
            XmlSerializer s = new XmlSerializer(typeof(List<BHDmini>));
            string xmlFile = @"~\App_Data\DataFY16\AllBookingsEnteredInFY16.xml";
            string physicalPath = System.Web.HttpContext.Current.Server.MapPath(xmlFile);
            List<BHDmini> AllBookingsFromFY16 = null;
            using (StreamReader sr = new StreamReader(physicalPath))
            {
                AllBookingsFromFY16 = s.Deserialize(sr) as List<BHDmini>;
            }

            return AllBookingsFromFY16;
        }


        public void ExtractAllBookingsFromFY16()
        {
            //      extract all bookings from FY16 

            //      method should only be used once it will write down inside App_Data the bookings entered from 01/02/2016 to 31/01/2017

            DateTime beginDate = new DateTime(2016, 02, 01);
            DateTime endDate = new DateTime(2017, 1, 31);
            Dictionary<string, string> Sa1ToCsl = GenerateUniqueCslList();
            List<BHDmini> AllBookingsFromFY16 = tpContext3.BHDs
                .Where(b => b.DATE_ENTERED >= beginDate && b.DATE_ENTERED <= endDate)
                .ToList()
                .Select(_bhd => new BHDmini(_bhd, Sa1ToCsl))
                .ToList();
            //}

            // write down the data to the XML file
            string xmlFile = @"~\App_Data\DataFY16\AllBookingsEnteredInFY16.xml";
            string physicalPath = System.Web.HttpContext.Current.Server.MapPath(xmlFile);
            XmlSerializer s = new XmlSerializer(typeof(List<BHDmini>));
            using (StreamWriter sw = new StreamWriter(physicalPath, false))
            {
                s.Serialize(sw, AllBookingsFromFY16);
            }

        }


        public virtual List<BHDmini> ExtractQuotingContractingEnquiries(CompanySpecifics cs)
        {
            // get all the enquiries which are currently under quoting or contracting

            Dictionary<string, string> Sa1ToCsl = GenerateUniqueCslList();
            var filterStatuses = cs.BookingStageCodes[bookingStage.pendingContract]
                .Concat(cs.BookingStageCodes[bookingStage.pendingSalesBDOPS]);
            return tpContext3.BHDs
                .Where(_bhd => filterStatuses.Contains(_bhd.STATUS.Trim()))
                .ToList()
                .Select(_bhd => new BHDmini(_bhd, Sa1ToCsl))
                .ToList();
            //}
        }



        public IEnumerable<BkgAnalysisInfo> TransformAllBookings(DateTime BeginningFY15, List<PendingEnquiryTableRow> deadlineTable, Dictionary<int, StagesDates> keyStagesDates)
        {

            // used for the Bookings Overview analysis


            CompanySpecifics compSpec = new CompanySpecifics();


            IEnumerable<BHD> BkgsSelected = tpContext3.BHDs
                .Where(b => b.DATE_ENTERED >= BeginningFY15)
                .ToList()
                .Where(b => compSpec.BookingStageCodes[bookingStage.received].Contains(b.STATUS.Trim()));


            // Extraction of DRM , SA3 , DA6 ,  SA1 , CSL and using them as dictionaries
            var _DRM = tpContext3.DRMs.ToDictionary(d => d.CODE, d => new { d.NAME, d.ANALYSIS_MASTER6 });
            Dictionary<string, string> _SA3 = tpContext3.SA3.ToDictionary(c => c.CODE, c => c.DESCRIPTION);
            Dictionary<string, string> _DA6 = tpContext3.DA6.ToDictionary(d => d.CODE, d => d.DESCRIPTION);
            //Dictionary<string, string> _SA1 = tpContext3.SA1.ToDictionary(s => s.CODE, s => s.DESCRIPTION);
            //Dictionary<string, string> _CSL = tpContext3.CSLs.ToDictionary(c => c.INITIALS, c => c.NAME);


            // creation of a dictionary for the pending areas
            Dictionary<string, string> _PendingAreas = compSpec.BookingStageCodes[bookingStage.received]
                .ToDictionary(s => s,
                s => compSpec.pendingOffersPlaces.FirstOrDefault(kvp => kvp.Value.Contains(s)).Key ?? "");


            // calculate the pending bookings past deadline
            Func<BHD, Boolean> PastDeadline = b =>
            {
                var TableRow = deadlineTable.FirstOrDefault(tr => tr.Full_Reference == b.FULL_REFERENCE.Trim());
                return TableRow == null ? false : TableRow.Days_Before_Deadline < 0;
            };


            Func<BHD, BkgAnalysisInfo> ProjectOnBkgMinInfo = b =>
            {
                BkgAnalysisInfo bk = new BkgAnalysisInfo();

                bk.AgentName = _DRM[b.AGENT].NAME.Trim();
                bk.BDConsultant = DictSA1[b.SALE1.Trim()].Trim();
                bk.BkgType = b.SALE6.Trim();
                bk.BookingStatus = b.STATUS.Trim();
                bk.ClientCategory = _DA6[_DRM[b.AGENT].ANALYSIS_MASTER6].Trim();
                bk.Consultant = DictCsls[b.CONSULTANT.Trim()].Trim();

                // agreed with JM on the 8/12/2016 that if no date confirmed , date entered should be default
                bk.DateConfirmed = keyStagesDates.ContainsKey(b.BHD_ID) ?
            keyStagesDates[b.BHD_ID].GetLastStageDate() : b.DATE_ENTERED;

                bk.DateEntered = b.DATE_ENTERED;
                bk.DEPARTMENT = b.DEPARTMENT.Trim();
                bk.EstimatedTurnover = b.UDTEXT3.Trim();
                bk.Full_Reference = b.FULL_REFERENCE.Trim();
                bk.LocationName = _SA3[b.SALE3].Trim();
                bk.CompanyDepartment = compSpec.RetrieveDepartment(b.FULL_REFERENCE);
                bk.PendingArea = _PendingAreas[b.STATUS.Trim()];

                bk.PastDeadline = PastDeadline(b);

                bk.DateConfirmedOnTP = b.UDTEXT4.Trim();


                return bk;
            };


            //  projection with use of dictionary :
            return BkgsSelected.Select(b => ProjectOnBkgMinInfo(b)).ToList();
            //}

        }


        public virtual IEnumerable<ContractTBAsViewModel> ExtractTBAsViewModel()
        {


            var selectedCRMs = tpContext3.CRMs.Where(c => c.NAME.Contains("advised"));
            var validCRMs = selectedCRMs.Select(c => c.CRM_ID);

            var selectedOPTs = tpContext3.OPTs.Where(o => o.AC == "Y");
            var validOPTs = selectedOPTs.Select(o => o.OPT_ID);

            var selectedLOCs = tpContext3.LOCs;

            var cs = new CompanySpecifics();
            var validStatus = cs.BookingStageCodes[bookingStage.confirmed];
            var filteredBSLs = tpContext3.BSLs
                //.Include(_bsl => _bsl.BHD)
                .Where(_bsl => _bsl.SL_STATUS.Trim() == "TB")
                .Where(_bsl => validStatus.Contains(_bsl.BHD.STATUS.Trim()))
                .Where(_bsl => validCRMs.Contains(_bsl.CRM_ID))
                .Where(_bsl => validOPTs.Contains(_bsl.OPT_ID));

            if (!filteredBSLs.Any()) return null;

            var selectedBSLs = filteredBSLs.Select(_bsl => _bsl.BSL_ID).ToList();

            var dictBSLs = tpContext3.BSLs.Where(sl => selectedBSLs.Contains(sl.BSL_ID))
                .ToDictionary(sl => sl.BSL_ID, sl => sl);

            var dictCRMs = selectedCRMs.ToDictionary(c => c.CRM_ID, c => c);

            var listOptsId = dictBSLs.Select(kvp => kvp.Value.OPT_ID).Distinct();
            var dictOPTs = selectedOPTs.Where(o => listOptsId.Contains(o.OPT_ID))
                .ToDictionary(o => o.OPT_ID, o => o);

            var listLocs = dictOPTs.Select(kvp => kvp.Value.LOCATION).Distinct();
            var dictLOCs = selectedLOCs.Where(l => listLocs.Contains(l.CODE))
                .ToDictionary(l => l.CODE, l => l);

            IEnumerable<ContractTBAsViewModel> itemsVM = selectedBSLs.Select(_bslId =>
            {
                var vm = new ContractTBAsViewModel();

                vm.BSL_ID = _bslId;

                var bsl = dictBSLs[_bslId];
                vm.BookingName = bsl.BHD.NAME;
                vm.FULL_REFERENCE = bsl.BHD.FULL_REFERENCE;
                vm.ServiceDate = bsl.DATE;

                vm.SupplierName = dictCRMs[bsl.CRM_ID].NAME;

                var opt = dictOPTs[bsl.OPT_ID];
                vm.LocCode = opt.LOCATION;
                vm.OptName = opt.DESCRIPTION;

                vm.LocName = dictLOCs[opt.LOCATION].NAME;

                return vm;
            })
            .ToList();

            return itemsVM;


        }


    }




    public class ContractTBAsViewModel
    {
        public int BSL_ID { get; set; }
        public string SupplierName { get; set; }
        public string FULL_REFERENCE { get; set; }
        public string BookingName { get; set; }
        public string LocCode { get; set; }
        public string LocName { get; set; }
        public string OptName { get; set; }
        public DateTime ServiceDate { get; set; }

    }

}
