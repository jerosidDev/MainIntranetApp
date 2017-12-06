using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Reporting_application.Services.SuppliersAnalysis
{
    public interface ISuppliersAnalysis
    {
        Task InitialLoadingAsync(int currentFY);


        List<SuppliersAnalysisData> supplierAnalysisData { get; set; }

        Dictionary<string, string> GetEvalItemsForTheView();
        Dictionary<string, string> GetFYsForTheView();
        Dictionary<string, List<string>> GetBookingStatusesForTheView();

        Dictionary<string, Dictionary<string, string>> GenerateFilterValuesForDropDowns(List<SuppliersAnalysisData> _sadList);


        List<SuppliersAnalysisData> FilterDataBasedOnViewSelection(NameValueCollection RequestForm);


        void EvaluateDataFromUserSelection(NameValueCollection RequestForm);


        Dictionary<string, object> DataToBeJSONed { get; set; }


        object GetDataToBeExported(DateTime dateRequested);

    }
}
