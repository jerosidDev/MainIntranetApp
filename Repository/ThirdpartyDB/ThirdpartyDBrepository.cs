using Reporting_application.Models;
using Reporting_application.Repository.SolutionDB;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using Reporting_application.Services.SuppliersAnalysis;
using Reporting_application.Utilities.CompanyDefinition;
using Reporting_application.Utilities.GoogleCharts;
using Reporting_application.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Reporting_application.Repository
{
    public class ThirdpartyDBrepository : IThirdpartyDBrepository
    {

        public List<BHDmini> AllBHDminiFromFY17 { get; set; }


        public Dictionary<string, string> DictCsls { get; set; }


        public Dictionary<string, string> DictSA1 { get; set; }


        // will need to gradually get rid of it because not thread safe
        //      reason of its existence using MOQ at the unit test level to mock dbcontext
        public IThirdpartyDBContext tpContext3 { get; set; }


        public ThirdpartyDBrepository(IThirdpartyDBContext _tpContext3)
        {
            tpContext3 = _tpContext3;
        }


        public virtual IDictionary<string, string> GetRecentlyActiveConsultants()
        {
            DateTime beginDt = DateTime.Today.AddMonths(-4);
            var listRecentBhdMini = AllBHDminiFromFY17.Where(b => b.DATE_ENTERED >= beginDt);

            var listCsl = listRecentBhdMini.Select(b => b.CONSULTANT.Trim());
            var listBdCsl = listRecentBhdMini.Select(b => b.SALE1.Trim());

            if (DictCsls == null) GetAllConsultants();
            return listCsl.Concat(listBdCsl).Distinct().ToDictionary(csl => csl, csl => DictCsls[csl]);

        }

        public virtual void GetAllConsultants()
        {
            DictCsls = tpContext3.CSLs.ToDictionary(c => c.INITIALS.Trim(), c => c.NAME.Trim());
            DictSA1 = tpContext3.SA1.ToDictionary(sa1 => sa1.CODE.Trim(), sa1 => sa1.DESCRIPTION.Trim());
        }


        public virtual async Task GetAllConsultantsAsync()
        {

            await GetCsl();
            await GetSA1();

        }


        private async Task GetCsl()
        {
            DictCsls = await tpContext3.CSLs.ToDictionaryAsync(c => c.INITIALS.Trim(), c => c.NAME.Trim());
        }


        private async Task GetSA1()
        {
            DictSA1 = await tpContext3.SA1.ToDictionaryAsync(sa1 => sa1.CODE.Trim(), sa1 => sa1.DESCRIPTION.Trim());
        }


        public virtual void ExtractAllBHDminiFromFY17()
        {
            //      extract all bookings from FY17 

            // 12/06/2017 : SALE1 property needs to be corrected to match the CSL table in ThirdpartyDB


            Dictionary<string, string> Sa1ToCsl = GenerateUniqueCslList();

            DateTime beginDate = new DateTime(2017, 02, 01);
            AllBHDminiFromFY17 = tpContext3.BHDs
                .Where(b => b.DATE_ENTERED >= beginDate)
                .ToList()
                .Select(_bhd => new BHDmini(_bhd, Sa1ToCsl))
                .ToList();

        }


        public virtual async Task ExtractAllBHDminiFromFY17Async()
        {
            //      extract all bookings from FY17 


            DateTime beginDate = new DateTime(2017, 02, 01);
            var _bhdsTask = tpContext3.BHDs.Where(b => b.DATE_ENTERED >= beginDate).ToListAsync();


            // 12/06/2017 : SALE1 property needs to be corrected to match the CSL table in ThirdpartyDB
            Dictionary<string, string> Sa1ToCsl = await GenerateUniqueCslListAsync();



            var _bhds = await _bhdsTask;
            AllBHDminiFromFY17 = _bhds.Select(_bhd => new BHDmini(_bhd, Sa1ToCsl)).ToList();



        }




        public Dictionary<string, string> GenerateUniqueCslList()
        {
            // compare from the 2 tables CSL and SA1 in order to remove dupplicate consultants created by diacritics differences

            if (DictCsls == null || DictSA1 == null)
                GetAllConsultants();

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

        public async Task<Dictionary<string, string>> GenerateUniqueCslListAsync()
        {
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
            var _DRM = tpContext3.DRMs.ToDictionary(d => d.CODE, d => new { d.NAME, d.ANALYSIS_MASTER6, d.AGENCYCHAIN });
            Dictionary<string, string> _SA3 = tpContext3.SA3.ToDictionary(c => c.CODE, c => c.DESCRIPTION);
            Dictionary<string, string> _DA6 = tpContext3.DA6.ToDictionary(d => d.CODE, d => d.DESCRIPTION);


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


            if (DictCsls == null || DictSA1 == null)
                GetAllConsultants();
            Func<BHD, BkgAnalysisInfo> ProjectOnBkgMinInfo = b =>
            {
                BkgAnalysisInfo bk = new BkgAnalysisInfo();

                bk.AgentName = _DRM[b.AGENT].NAME.Trim();
                bk.AgentChain = _DRM[b.AGENT].AGENCYCHAIN.Trim();

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

                //bk.TravelDate = b.UDTEXT4.Trim();
                bk.TravelDate = b.TRAVELDATE;

                return bk;
            };


            //  projection with use of dictionary :
            return BkgsSelected.Select(b => ProjectOnBkgMinInfo(b)).ToList();

        }


        public virtual IEnumerable<ContractTBAsInfo> ExtractTBAsList()
        {


            var selectedCRMs = tpContext3.CRMs.Where(c => c.NAME.Contains("advised"));
            var validCRMs = selectedCRMs.Select(c => c.CRM_ID);

            var selectedOPTs = tpContext3.OPTs.Where(o => o.AC == "Y");
            var validOPTs = selectedOPTs.Select(o => o.OPT_ID);

            var selectedLOCs = tpContext3.LOCs;

            var cs = new CompanySpecifics();
            var validStatus = cs.BookingStageCodes[bookingStage.confirmed];
            var filteredBSLs = tpContext3.BSLs
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

            IEnumerable<ContractTBAsInfo> itemsVM = selectedBSLs.Select(_bslId =>
            {
                var vm = new ContractTBAsInfo();

                vm.BSL_ID = _bslId;

                var bsl = dictBSLs[_bslId];
                vm.Booking_Name = bsl.BHD.NAME;
                vm.FULL_REFERENCE = bsl.BHD.FULL_REFERENCE;
                vm.Service_Date = bsl.DATE.ToString("dd MMM yyyy");

                vm.Supplier_Name = dictCRMs[bsl.CRM_ID].NAME;

                var opt = dictOPTs[bsl.OPT_ID];
                vm.LocCode = opt.LOCATION;
                vm.Option_Name = opt.DESCRIPTION;

                vm.Location_Name = dictLOCs[opt.LOCATION].NAME;

                return vm;
            })
            .ToList();

            return itemsVM;


        }



        public virtual async Task<IEnumerable<ContractTBAsInfo>> ExtractTBAsListAsync()
        {


            var selectedCRMs = tpContext3.CRMs.Where(c => c.NAME.Contains("advised"));
            var validCRMs = selectedCRMs.Select(c => c.CRM_ID);

            var selectedOPTs = tpContext3.OPTs.Where(o => o.AC == "Y");
            var validOPTs = selectedOPTs.Select(o => o.OPT_ID);

            var selectedLOCs = tpContext3.LOCs;

            var cs = new CompanySpecifics();
            var validStatus = cs.BookingStageCodes[bookingStage.confirmed];
            DateTime beginDt = new DateTime(2016, 2, 1); // to remove past irrelevant service lines
            var filteredBSLs = tpContext3.BSLs
                //.Include(_bsl => _bsl.BHD)
                .Where(_bsl => _bsl.SL_STATUS.Trim() == "TB")
                .Where(_bsl => validStatus.Contains(_bsl.BHD.STATUS.Trim()))
                .Where(_bsl => validCRMs.Contains(_bsl.CRM_ID))
                .Where(_bsl => validOPTs.Contains(_bsl.OPT_ID))
                .Where(_bsl => _bsl.DATE > beginDt);

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

            IEnumerable<ContractTBAsInfo> itemsVM = selectedBSLs.Select(_bslId =>
            {
                var vm = new ContractTBAsInfo();

                vm.BSL_ID = _bslId;

                var bsl = dictBSLs[_bslId];
                vm.Booking_Name = bsl.BHD.NAME;
                vm.FULL_REFERENCE = bsl.BHD.FULL_REFERENCE;
                vm.Service_Date = bsl.DATE.ToString("dd MMM yyyy");

                vm.Supplier_Name = dictCRMs[bsl.CRM_ID].NAME;

                var opt = dictOPTs[bsl.OPT_ID];
                vm.LocCode = opt.LOCATION;
                vm.Option_Name = opt.DESCRIPTION;

                vm.Location_Name = dictLOCs[opt.LOCATION].NAME;

                return vm;
            })
            .ToList();

            return itemsVM;


        }


        public virtual async Task<IEnumerable<BHD>> GetBHDsFromDateEnteredAsync(DateTime from)
        {

            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.BHDs
                    .Where(b => b.DATE_ENTERED >= from)
                    .ToListAsync();
            }

        }

        public virtual async Task<IEnumerable<DRM>> GetDRMsAsync()
        {
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.DRMs.ToListAsync();
            }
        }

        public virtual async Task<IEnumerable<SA3>> GetSA3Async()
        {
            // locations
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.SA3.ToListAsync();
            }
        }



        public async Task<IEnumerable<LOC>> GetLOCAsync()
        {
            // locations for services/suppliers
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.LOCs.ToListAsync();
            }
        }





        public virtual async Task<IEnumerable<SA1>> GetSA1Async()
        {
            // bd consultants
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.SA1.ToListAsync();
            }
        }


        public virtual async Task<IEnumerable<CSL>> GetCSLAsync()
        {
            // locations
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.CSLs.ToListAsync();
            }
        }


        public virtual async Task<Dictionary<int, int>> GetPaxNumbersAsync()
        {
            //     pax number
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.BSDs
                    .Where(bsd => bsd.BSL_ID == 0)
                    .ToDictionaryAsync(bsd => bsd.BHD_ID, bsd => bsd.PAX);
            }
        }



        public async Task<List<SuppliersAnalysisData>> GetSuppliersFromOptsAsync(IEnumerable<int> listOpt)
        {
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.OPTs
                     .Where(o => listOpt.Contains(o.OPT_ID))
                     .Join(tpContext.CRMs, o => o.SUPPLIER, c => c.CODE,
                     (o, c) => new SuppliersAnalysisData
                     {
                         OPT_ID = o.OPT_ID,
                         SUPPLIER_CODE = o.SUPPLIER,
                         SUPPLIER_NAME = c.NAME,
                         CHAIN_CODE = c.SUPPLIERCHAIN
                     })
                     .ToListAsync();

            }

        }


        public async Task<Dictionary<string, string>> GetSuppliersChainsAsync()
        {
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.CRMs
                    .Where(crm => crm.SUPPLIERMASTER == "Y")
                    .ToDictionaryAsync(crm => crm.CODE, crm => crm.NAME);
            }

        }


        public async Task<IEnumerable<SRV>> GetServiceTypesAsync()
        {
            using (var tpContext = new ThirdpartyDBContext())
            {
                return await tpContext.SRVs.ToListAsync();
            }

        }


        public async Task<List<SuppliersAnalysisData>> GetServicesAndCostsForSAD(DateTime beginDt, DateTime endDt, List<string> BookingStatuses, List<string> excludedSL)
        {
            using (var tpContext = new ThirdpartyDBContext())
            {


                //  join tables BSL-BHD -> BSD 
                var sadList = await tpContext.BSLs
                  .Where(bsl => bsl.DATE >= beginDt && bsl.DATE <= endDt)
                  .Where(bsl => BookingStatuses.Contains(bsl.BHD.STATUS))
                  .Where(bsl => !excludedSL.Contains(bsl.SL_STATUS))
                  .Join(tpContext.BSDs, bsl => bsl.BSL_ID, bsd => bsd.BSL_ID,
                  (bsl, bsd) => new SuppliersAnalysisData()
                  {
                      BSL_ID = bsl.BSL_ID,
                      OPT_ID = bsl.OPT_ID,

                      // from BSL and BHD tables 
                      DATE = bsl.DATE,
                      BHD_ID = bsl.BHD_ID,
                      FULL_REFERENCE = bsl.BHD.FULL_REFERENCE,
                      BookingStatus = bsl.BHD.STATUS,
                      CURRENCY = bsl.BHD.CURRENCY,
                      BASE_DIV_RATE = (double)bsl.BHD.BASE_DIV_RATE,
                      BASE_MUT_RATE = (double)bsl.BHD.BASE_MUT_RATE,
                      SL_STATUS = bsl.SL_STATUS,
                      Service_Type_Code = bsl.SERVICE,
                      Client_CODE = bsl.BHD.AGENT,
                      Client_NAME = bsl.BHD.DRM.NAME,
                      LOCATION_CODE = bsl.LOCATION,
                      BDConsultant_CODE = bsl.BHD.SALE1,


                      // from BSD table
                      COST = (double)bsd.COST,
                      ROOMCOUNT = bsd.ROOMCOUNT,
                      SCU_FOC = bsd.SCU_FOC,
                      SCU_QTY = bsd.SCU_QTY,


                      // calculate  £ cost 
                      Base_Cost = (double)(bsd.COST * bsl.BHD.BASE_MUT_RATE / bsl.BHD.BASE_DIV_RATE),


                  })
                  .ToListAsync();
                return sadList;

            }

        }



        public virtual async Task<List<SuppliersAnalysisData>> InitialLoadingSuppliersAnalysisAsync(int currentFY,List<string> validStatuses)
        {

            //      get list of master suppliers for later
            var taskSuppliersChains = GetSuppliersChainsAsync();
            //      get locations
            var locTask = GetLOCAsync();
            //      get bd consultant
            var bdCslTask = GetSA1Async();
            //      service types
            var srvTask = GetServiceTypesAsync();

            var cs = new CompanySpecifics();

            // for previous and current financial year
            //var currentFY = DatesUtilities.GetCurrentFinancialYear();
            DateTime beginDt = new DateTime(currentFY - 1, 2, 1);
            DateTime endDt = new DateTime(currentFY + 2, 1, 31);

            //var validStatuses = cs.BookingStageCodes[bookingStage.received];
            var excludedSL = cs.ServiceLinesCodes[ServiceLineType.Unconfirmed];
            //  get BSL joined with BSD
            var sadList = await GetServicesAndCostsForSAD(beginDt, endDt, validStatuses, excludedSL);

            //  async calls following

            //      get OPT joined with CRM 
            var listOpt = sadList.Select(sad => sad.OPT_ID).Distinct();
            Task<List<SuppliersAnalysisData>> taskOpt = GetSuppliersFromOptsAsync(listOpt);


            // await all the tasks and finish filling sadList
            sadList = sadList.Join(await taskOpt, sad1 => sad1.OPT_ID, sad2 => sad2.OPT_ID,
                (sad1, sad2) =>
                {
                    sad1.SUPPLIER_CODE = sad2.SUPPLIER_CODE;
                    sad1.SUPPLIER_NAME = sad2.SUPPLIER_NAME;
                    sad1.CHAIN_CODE = sad2.CHAIN_CODE;
                    sad1.Service_Type_Name = sad2.Service_Type_Name;
                    return sad1;
                })
                .ToList();


            var dictSuppliersChains = await taskSuppliersChains;
            var locs = await locTask;
            var bdCsl = await bdCslTask;
            var srv = await srvTask;
            sadList = sadList.Select(sad =>
            {

                if (sad.CHAIN_CODE.Trim() == "")
                    sad.CHAIN_NAME = "No chain";
                else
                {
                    sad.CHAIN_NAME = dictSuppliersChains.FirstOrDefault(kvp => kvp.Key == sad.CHAIN_CODE).Value;

                    // sometimes the chain is not defined as a chain (master_supplier =="N")
                    if (sad.CHAIN_NAME == null)
                        sad.CHAIN_NAME = sadList.FirstOrDefault(sad2 => sad2.SUPPLIER_CODE == sad.CHAIN_CODE).SUPPLIER_NAME;

                }


                sad.LOCATION_NAME = locs.FirstOrDefault(l => l.CODE == sad.LOCATION_CODE).NAME;
                sad.BDConsultant_NAME = bdCsl.FirstOrDefault(b => b.CODE == sad.BDConsultant_CODE).DESCRIPTION;
                sad.Service_Type_Name = srv.FirstOrDefault(s => s.CODE == sad.Service_Type_Code).NAME;
                sad.DEPARTMENT = cs.RetrieveDepartment(sad.FULL_REFERENCE);

                // room-night calculation to disregard anything non accommodation
                sad.Rooms_Nights = sad.Service_Type_Name.Contains("Accommodation") ? sad.ROOMCOUNT * sad.SCU_QTY : 0;

                return sad;
            })
            .ToList();


            return sadList;

        }

    }


}