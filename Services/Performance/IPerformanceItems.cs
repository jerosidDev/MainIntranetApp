using Reporting_application.Utilities.CompanyDefinition;
using System;
using System.Collections.Generic;

namespace Reporting_application.Services.Performance
{
    public interface IPerformanceItems<T>
    {
        Func<CompanySpecifics, string, bool> IsDepartment { get; set; }
        Func<CompanySpecifics, bool> IsSuccess { get; set; }
        Func<string, bool> IsCsl { get; set; }
        Func<List<T>, List<string>> GetAllCslFromListOfItems { get; set; }
    }
}
