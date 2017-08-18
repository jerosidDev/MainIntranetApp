using Reporting_application.Models;
using Reporting_application.ReportingModels;
using Reporting_application.Repository;
using Reporting_application.Repository.ThirdpartyDB;
using Reporting_application.Services.Performance;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;

namespace Reporting_application.Controllers
{
    [Authorize]
    public class BkgsOverviewController : Controller
    {


        private ThirdpartyDBContext db = new ThirdpartyDBContext();

        public static BookingsStagesAnalysis bAnalysis;


        public BkgsOverviewController(IPerformance _perfRepo, ICompanyDBRepository _compDbRepo, IThirdpartyDBrepository _tpRepo)
        {
            if (bAnalysis == null)
            {
                bAnalysis = new BookingsStagesAnalysis(db);
                bAnalysis.perfRepo = _perfRepo;
                bAnalysis.compDbRepo = _compDbRepo;
                bAnalysis.tpRepo = _tpRepo;
            }
        }



        [HttpGet]
        public ActionResult Index()
        {


            // initialisation of ViewData to create the initial charts
            ViewData["BeginningTF"] = DateTime.Today.AddDays(-7).ToShortDateString();
            ViewData["EndTF"] = DateTime.Today.AddDays(-1).ToShortDateString();
            ViewData["DeptSelected"] = "All";
            ViewData["bkgTypeSelected"] = "All";


            // generate the charts 
            bAnalysis.UpdateChartsList(ViewData);


            return View(bAnalysis);

        }




        [HttpPost]
        public ActionResult Index(string dptSelected)
        {

            // department selected on the form
            ViewData["DeptSelected"] = Request.Form["Dpt"];
            ViewData["bkgTypeSelected"] = Request.Form["bkgsTypes"];
            // TimeFrame selected
            ViewData["BeginningTF"] = Request.Form["BeginningTF"];
            ViewData["EndTF"] = Request.Form["EndTF"];


            // generate the charts 
            bAnalysis.UpdateChartsList(ViewData);

            return View(bAnalysis);
        }



        public ActionResult DisplayChart4()
        {
            return View();
        }



        public ActionResult RenderChartImage(int? ChartId)
        {


            MemoryStream imageStream = new MemoryStream();

            ChartDisplayed cd = bAnalysis.ListChartsCreated.Where(c => c.ChartId == ChartId).FirstOrDefault();


            // save the chart as an image
            cd.ChartCreated.SaveImage(imageStream, ChartImageFormat.Png);

            Response.ContentType = "image/png";
            imageStream.WriteTo(Response.OutputStream);

            return null;

        }


        public ActionResult ExportToExcel()
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook wb = xlApp.Workbooks.Add();
            Excel.Worksheet ws = wb.Worksheets.Add();

            // write the BkgMinInfo class's properties on the first row
            System.Reflection.PropertyInfo[] propInfo = typeof(BkgAnalysisInfo).GetProperties();
            int row = 1; int col = 1;
            foreach (var p in propInfo)
            {
                ws.Cells[row, col] = p.Name;
                col++;
            }

            // write the data
            row++;
            foreach (BkgAnalysisInfo b in bAnalysis.BkgsSelectedInView2)
            {
                col = 1;
                foreach (var p in propInfo)
                {
                    if (p.Name == "DateConfirmed" && p.GetValue(b).ToString() != "")
                    {
                        // attention date confirmed is converted to date with the wrong culture if done directly to excel
                        DateTime dc = DateTime.Parse(p.GetValue(b).ToString(), bAnalysis.culture);
                        ws.Cells[row, col] = dc;
                    }
                    else
                    {
                        ws.Cells[row, col] = p.GetValue(b);
                    }
                    col++;
                }
                row++;
            }


            xlApp.Visible = true;
            return null;
        }

        public ActionResult ExportToExcel2()
        {
            // action used to download the data on the client and export them to excel
            // using microsoft automation on the server like it was done for the first version is bad practice and doesn't work anyway


            //  creation of the webgrid from pcModel.BkgsSelectedInView2 
            System.Web.Helpers.WebGrid _wb = new System.Web.Helpers.WebGrid(bAnalysis.BkgsSelectedInView2, rowsPerPage: 10000, canPage: false, canSort: false);



            string gridData = _wb.GetHtml().ToString();


            Response.ClearContent();
            Response.AddHeader("Content-Disposition", "attachment; filename = BookingsData.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.Write(gridData);
            Response.End();
            return null;
        }


    }
}
