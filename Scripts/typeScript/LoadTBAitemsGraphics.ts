/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/google.visualization/google.visualization.d.ts" />



$(document).ready(function () {

    let tgl: TBAChartsLoader;

    let urlJSON: string = "/PerformanceOverview/RetrieveTBAsInfo";
    $.getJSON(urlJSON, function (data: any) {
        tgl = new TBAChartsLoader(data);
        google.charts.load('current', { packages: ['corechart', 'gauge'] });
        google.charts.setOnLoadCallback(function () {
            tgl.InitiateDrawing();
            tgl.DrawGaugeCharts();
        });
    });

    // reload TBA related gauge charts
    $('#contractCsl').change(function () {
        //alert("hello TBA");
        tgl.DrawContractCslGauge();
    });
});




class TBAChartsLoader {



    constructor(private TBAdata: any) {
    }


    AssignedTBA: TBAcharting.IConsultantsTBA[] = [];
    totalTBA: number = 0;


    InitiateDrawing(): void {


        let dataTableColChart = this.GenerateDataTableColChart();

        let chart = new google.visualization.ColumnChart(document.getElementById('colChart5_div'));

        // for zooming option
        //var explo = {
        //    actions: ['dragToZoom', 'rightClickToReset'],
        //    axis: 'vertical',
        //    keepInBounds: true,
        //    maxZoomIn: 0.01
        //};

        //optionColChart["explorer"] = explo;


        let optionColChart: google.visualization.ColumnChartOptions = {
            title: "To be advised services per consultant", legend: { position: 'none' }, height: 600,
            animation: { duration: 1500, easing: 'out' },
            chartArea: { left: '40', width: '90%' },
            tooltip: { isHtml: true, trigger: 'selection' },
            isStacked: true,

        };


        chart.draw(dataTableColChart, optionColChart);


        this.AssignedTBA = this.CalculateTBAworkload();

    };


    GenerateDataTableColChart = () => {

        let returnedTable = new google.visualization.DataTable();

        // columns definition
        returnedTable.addColumn('string', 'Consultant');
        returnedTable.addColumn('number', 'To be advised assignments');

        // had to add  "p?: any;" in "DataTableColumnDescription" of google.visualization.d.ts
        returnedTable.addColumn({ type: 'string', role: 'tooltip', 'p': { 'html': true } });
        returnedTable.addColumn({ type: 'string', role: 'style' });

        for (let k in this.TBAdata) {
            let dataList: TBAcharting.IContractTBAsInfo[] = this.TBAdata[k];
            let htmlString: string = this.GenerateToolTipTable(dataList);
            returnedTable.addRow([k, dataList.length, htmlString, ""]);
        }

        return returnedTable;
    };


    GenerateToolTipTable = (_dataList: TBAcharting.IContractTBAsInfo[]) => {

        let bgColor = 'blue';
        let captionTitle: string = 'to be advised services assigned';
        let cslName: string = _dataList[0]._contractConsultant.NAME;

        var returnedHTML = '<style> #customers { border-collapse: collapse; width: 100%; table-layout:fixed; width:780px; } #customers td, #customers th { border: 1px solid #ddd; padding: 8px;  word-wrap:break-word; } #customers tr:nth-child(even) {background-color: #f2f2f2; } #customers tr:hover {background-color: #ddd; } #customers th { padding-top: 12px; padding-bottom: 12px; text-align: left; } </style><table id="customers">';

        // add the caption:
        var captionStyle = 'style="text-align: center; font-weight: bold; color: white; background-color: '
            + bgColor + ';"'
        returnedHTML += '<caption ' + captionStyle + ' title="Click to export the table to Excel">' + captionTitle + " to " + cslName + '</caption>';

        // generate the headers
        var firstObj = _dataList[0];
        returnedHTML += '<tr>';
        for (var val in firstObj) {
            if (typeof firstObj[val] === "string") returnedHTML += '<th>' + val + '</th>';
        }
        returnedHTML += '</tr>';

        // generate the data
        for (let objData of _dataList) {
            returnedHTML += '<tr>';
            for (let val in objData) {
                if (typeof objData[val] === "string") returnedHTML += '<td>' + objData[val] + '</td>';
            }
            returnedHTML += '</tr>';
        }



        returnedHTML += '</table>';

        return returnedHTML;
    }

    CalculateTBAworkload(): TBAcharting.IConsultantsTBA[] {

        let listTBAworkload: TBAcharting.IConsultantsTBA[] = [];


        for (let cslName in this.TBAdata) {
            let firstInfo: TBAcharting.IContractTBAsInfo = this.TBAdata[cslName][0];

            let cslWorkload: TBAcharting.IConsultantsTBA = { INITIALS: firstInfo._contractConsultant.INITIALS, Workload: this.TBAdata[cslName].length };
            listTBAworkload.push(cslWorkload);
        }

        return listTBAworkload;
    }

    DrawGaugeCharts() {


        //  gauge for all contract department
        //      get the total number of TBAs
        for (let aTBA of this.AssignedTBA) {
            this.totalTBA += aTBA.Workload;
        }

        let data = google.visualization.arrayToDataTable([
            ['Label', 'Value'],
            ['Total TBAs', this.totalTBA],
        ]);

        let optionsContractDpt = {
            yellowFrom: 0, yellowTo: this.totalTBA,
            minorTicks: 5, max: this.totalTBA
        };

        let chart = new google.visualization.Gauge(document.getElementById('gauge9_div'));

        chart.draw(data, optionsContractDpt);

        this.DrawContractCslGauge();



    }

    DrawContractCslGauge() {

        // gauge for the contract consultant

        //     get the consultant selected
        let cslCode: string = $('#contractCsl').val();

        //     find the consultant object , manage the case non found
        let cslTBA: TBAcharting.IConsultantsTBA[] = this.AssignedTBA.filter(function (item) {
            return item.INITIALS == cslCode;
        });

        let cslTotal = cslTBA.length != 0 ? cslTBA[0].Workload : 0;

        let dataCsl = google.visualization.arrayToDataTable([
            ['Label', 'Value'],
            ['TBAs assigned', cslTotal * 100 / this.totalTBA],
        ]);

        let optionsCsl = {
            yellowFrom: 0, yellowTo: 100,
            minorTicks: 5, max: 100
        };

        let formatter = new google.visualization.NumberFormat({ suffix: '%', fractionDigits: 0 });

        formatter.format(dataCsl, 1); // Apply formatter to second column
        let chartCsl = new google.visualization.Gauge(document.getElementById('gauge10_div'));
        chartCsl.draw(dataCsl, optionsCsl);

    }

}

declare namespace google.visualization {

    export class Gauge extends CoreChartBase {
        draw(data: DataTable, options: any): void;
    }
}


declare namespace TBAcharting {


    interface IContractTBAsInfo {
        Service_Date: string;
        FULL_REFERENCE: string;
        Booking_Name: string;
        Supplier_Name: string;
        Location_Name: string;
        Option_Name: string;

        _contractConsultant: IContractConsultant;

    }

    interface IContractConsultant {
        INITIALS: string;
        NAME: string;
        LocationsAssigned: any;
    }

    interface IConsultantsTBA {
        INITIALS: string;
        Workload: number;
    }

}

