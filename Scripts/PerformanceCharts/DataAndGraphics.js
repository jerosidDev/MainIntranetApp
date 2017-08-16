/// <reference path="Calculation.js" />


// contain all data from which the charts data will be calculated
var JsonPerformanceItems;

// initial to full name dictionary
var dictConsultant;


// full list of departments, containing the consultants
var AllDptCslRelated;


// first will load the google charts
//  then wil proceed to filling the charts
LoadgooglePackage();



// load the google chart package once the data are here
function LoadgooglePackage() {

    // load the google package for the column and gauge chart
    google.charts.load('current', { 'packages': ['corechart', 'gauge'] });

    google.charts.setOnLoadCallback(LoadChartsData);

}


function LoadChartsData() {

    // url example : http://localhost:57014/PerformanceOverview/JsonPerformanceItems

    var urlJSON = "/PerformanceOverview/JsonPerformanceItems";
    $.getJSON(urlJSON, function (data) {
        JsonPerformanceItems = data;
        DrawCharts();
    });

}


function DrawCharts() {

    var departmentSelected = $('#dpt').val();
    var cslSelected = $('#csl').val();
    var contractCslSelected = $('#contractCsl').val();
    var AllContractCsl = $.makeArray($('#contractCsl option').map(function (i, opt) {
        return $(opt).val();
    }));


    //// will be filled in DrawGaugeChart()
    //var dataSourceContractDpt = {};


    DrawGaugeChart();

    function DrawGaugeChart() {

        var formatter = new google.visualization.NumberFormat({ suffix: '%', fractionDigits: 0 });

        var optSent = { redFrom: 0, redTo: 70, yellowFrom: 70, yellowTo: 85, greenFrom: 85, greenTo: 100, minorTicks: 5 };
        var optConv = { redFrom: 0, redTo: 18, yellowFrom: 18, yellowTo: 21, greenFrom: 21, greenTo: 100, minorTicks: 5 };
        var optMiss = { redFrom: 0, redTo: 96, yellowFrom: 96, yellowTo: 99, greenFrom: 99, greenTo: 100, minorTicks: 5 };

        var chartsMapping = {
            Gauge1: { option: optSent, title: "Sent within Deadline", div: "gauge1_div", perfType: "Turnaround", departmental: true, evaluated: departmentSelected },
            Gauge2: { option: optConv, title: "Conversion rate", div: "gauge2_div", perfType: "Conversion", departmental: true, evaluated: departmentSelected },
            Gauge3: { option: optMiss, title: "Valid data", div: "gauge3_div", perfType: "MissingData", departmental: true, evaluated: departmentSelected },
            Gauge4: { option: optSent, title: "Sent within Deadline", div: "gauge4_div", perfType: "Turnaround", departmental: false, evaluated: cslSelected },
            Gauge5: { option: optConv, title: "Conversion rate", div: "gauge5_div", perfType: "Conversion", departmental: false, evaluated: cslSelected },
            Gauge6: { option: optMiss, title: "Valid data", div: "gauge6_div", perfType: "MissingData", departmental: false, evaluated: cslSelected },

            Gauge7: { option: optSent, title: "Sent within Deadline", div: "gauge7_div", perfType: "ContractEnquiries", departmental: false, evaluated: AllContractCsl },

            Gauge8: { option: optSent, title: "Sent within Deadline", div: "gauge8_div", perfType: "ContractEnquiries", departmental: false, evaluated: contractCslSelected }

        };

        for (var g in chartsMapping) {


            function PerformanceFigure(g2) {

                var sourceType = chartsMapping[g2]["perfType"];
                var departmental = chartsMapping[g2]["departmental"];
                var evaluated = chartsMapping[g2].evaluated;

                var source = undefined;
                if (Array.isArray(evaluated)) {

                    // create a common source out of all the consultants
                    for (var i = 0; i < AllContractCsl.length ; i++) {
                        var csl = AllContractCsl[i];
                        var sourceCsl = FindFirstSourceForConsultant(csl);

                        //// will be used for the column chart:
                        //dataSourceContractDpt[csl] = sourceCsl;


                        // add success and unsuccess to the source
                        if (source == undefined) {
                            source = sourceCsl;
                        }
                        else {
                            source["Success"] = source["Success"].concat(sourceCsl["Success"]);
                            source["Unsuccess"] = source["Unsuccess"].concat(sourceCsl["Unsuccess"]);
                        }

                    }


                }
                else if (departmental) {
                    source = JsonPerformanceItems[sourceType][evaluated]["DepartmentOnly"];
                }
                else {
                    source = FindFirstSourceForConsultant(evaluated);
                }

                function FindFirstSourceForConsultant(evaluatedItem) {
                    // find the first source which contains the consultant selected
                    for (var dptName in JsonPerformanceItems[sourceType]) {
                        if (JsonPerformanceItems[sourceType][dptName][evaluatedItem] != undefined)
                            return JsonPerformanceItems[sourceType][dptName][evaluatedItem];
                    }
                }


                var totalSuccess = source != undefined ? source["Success"].length : 0;
                var totalUnsuccess = source != undefined ? source["Unsuccess"].length : 0;



                if (totalSuccess == 0 && totalUnsuccess == 0)
                    return undefined;

                return 100 * totalSuccess / (totalSuccess + totalUnsuccess);
            }


            var evaluation = PerformanceFigure(g);


            // hide the gauge chart if it is not relevant to the consultant (no data)
            var hideGauge = false;
            if (evaluation == undefined) {
                evaluation = 0;
                hideGauge = true;
            }


            var data = google.visualization.arrayToDataTable([['Label', 'Value'], [chartsMapping[g]["title"], evaluation]]);
            formatter.format(data, 1); // Apply formatter to second column
            var chart = new google.visualization.Gauge(document.getElementById(chartsMapping[g]["div"]));
            chart.draw(data, chartsMapping[g]["option"]);

            // hide the gauge chart if it is not relevant to the consultant (no data)
            if (hideGauge)
                $('#' + chartsMapping[g]["div"]).css('visibility', 'hidden');
            else
                $('#' + chartsMapping[g]["div"]).css('visibility', 'visible');


        }


    }


    DrawColumnChart();

    function DrawColumnChart() {

        var colChartsMapping = {
            ColChart1: {
                div: "colChart1_div", title: "Enquiries sent",
                dataSourceDpt: JsonPerformanceItems["Turnaround"][departmentSelected],
                colType: {
                    Success: { title: "Sent Within Deadline", color: "green" },
                    Unsuccess: { title: "Sent Beyond Deadline", color: "red" }
                }
            },
            ColChart2: {
                div: "colChart2_div", title: "Confirmed and unconfirmed enquiries",
                dataSourceDpt: JsonPerformanceItems["Conversion"][departmentSelected],
                colType: {
                    Success: { title: "Confirmed", color: "green" },
                    Unsuccess: { title: "Unconfirmed", color: "red" }
                }
            },
            ColChart3: {
                div: "colChart3_div", title: "Enquiries with missing or invalid data",
                dataSourceDpt: JsonPerformanceItems["MissingData"][departmentSelected],
                colType: {
                    Unsuccess: { title: "Details on missing or invalid data", color: "red" }
                }
            },
            ColChart4: {
                div: "colChart4_div", title: "Contract enquiries",
                dataSourceDpt: JsonPerformanceItems["ContractEnquiries"][departmentSelected],
                colType: {
                    Success: { title: "Confirmed", color: "green" },
                    Unsuccess: { title: "Unconfirmed", color: "red" }
                }
            }
        };


        for (var cc in colChartsMapping) {

            var cmObject = colChartsMapping[cc];

            var dataTableColChart = GenerateDataTableColChart(cmObject);

            function GenerateDataTableColChart(cmObject) {

                var returnedTable = new google.visualization.DataTable();

                // columns definition
                returnedTable.addColumn('string', 'Consultant');
                var colType = cmObject["colType"];
                for (var ct in colType) {
                    returnedTable.addColumn('number', colType[ct]["title"]);
                    returnedTable.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
                    returnedTable.addColumn({ type: 'string', role: 'style' });
                }


                // get the data source associated
                var dataDpt = cmObject.dataSourceDpt;


                for (var csl in dataDpt) {

                    // in case of the consultant does not appear for this type of evaluation
                    var consultantDisplayedinSelect = $('#csl option[value="' + csl + '"]').val() != undefined;

                    if (csl != "DepartmentOnly" && dataDpt[csl] != undefined && consultantDisplayedinSelect) {  

                        var rowToAdd = [];
                        //rowToAdd.push(dictConsultant[csl]);
                        var cslName = AllDptCslRelated[departmentSelected][csl];
                        rowToAdd.push(cslName);

                        for (var ct in colType) {

                            var listColType = dataDpt[csl][ct];
                            if (listColType.length != 0) {
                                rowToAdd.push(listColType.length)

                                var htmlTooltip = GenerateToolTipTable(listColType, colType[ct], cslName);

                                rowToAdd.push(htmlTooltip);

                                // see https://developers.google.com/chart/interactive/docs/roles#stylerole to add different styles
                                var style = "color:" + colType[ct]["color"] + ";";
                                if (csl != cslSelected) style += "opacity: 0.5;";
                                rowToAdd.push(style);

                            }
                            else {
                                rowToAdd.push(null, null, null);
                            }
                        }

                        function GenerateToolTipTable(listToolTipObj, colTypeObj, cslName) {

                            var returnedHTML = '<style> #customers { border-collapse: collapse; width: 100%; table-layout:fixed; width:780px; } #customers td, #customers th { border: 1px solid #ddd; padding: 8px;  word-wrap:break-word; } #customers tr:nth-child(even) {background-color: #f2f2f2; } #customers tr:hover {background-color: #ddd; } #customers th { padding-top: 12px; padding-bottom: 12px; text-align: left; } </style><table id="customers">';

                            // add the caption:
                            var captionStyle = 'style="text-align: center; font-weight: bold; color: white; background-color: '
                                + colTypeObj["color"] + ';"'
                            returnedHTML += '<caption ' + captionStyle + ' title="Click to export the table to Excel">' + colTypeObj["title"] + " for " + cslName + '</caption>';

                            // generate the headers
                            var firstObj = listToolTipObj[0]["_tooltipColChart"];
                            returnedHTML += '<tr>';
                            for (var p in firstObj) {
                                returnedHTML += '<th>' + p + '</th>';
                            }
                            returnedHTML += '</tr>';

                            // generate the data
                            listToolTipObj.forEach(function (itemObj) {
                                returnedHTML += '<tr>';
                                htmlCell = itemObj["_tooltipColChart"];
                                for (var p in htmlCell) {
                                    returnedHTML += '<td>' + htmlCell[p] + '</td>';
                                }
                                returnedHTML += '</tr>';
                            });

                            returnedHTML += '</table>';

                            return returnedHTML;
                        }

                        returnedTable.addRow(rowToAdd);
                    }
                }

                return returnedTable;
            }

            var titleChart = cmObject["title"] != "Contract enquiries" ? departmentSelected + " department: " + cmObject["title"] : "";
            var optionColChart = {
                title: titleChart, legend: 'none', height: '600',
                vAxis: { logscale: true },
                animation: { duration: 1500, easing: 'out' },
                chartArea: { left: '40', width: '90%' },
                tooltip: { isHtml: true, trigger: 'selection' },
                isStacked: true,
            };

            var explo = {
                actions: ['dragToZoom', 'rightClickToReset'],
                axis: 'vertical',
                keepInBounds: true,
                maxZoomIn: 0.01
            };

            optionColChart["explorer"] = explo;

            var colChart = new google.visualization.ColumnChart(document.getElementById(cmObject["div"]));
            colChart.draw(dataTableColChart, optionColChart);

        }



    }






}




// Events
$(document).ready(function () {

    UpdateCsl($('#dpt').val());

    // update the consultant dropdown down based on the department selection
    $('#dpt').change(function () {
        var departmentSelected = $('#dpt').val();
        //  ajax call to update the csl list
        UpdateCsl(departmentSelected);
    });

    function UpdateCsl(departmentSelected) {


        if (AllDptCslRelated == undefined) {
            $.getJSON("/PerformanceOverview/RetrieveAllCslAssociatedToAllDpt", function (data2) {
                AllDptCslRelated = data2;
                //      clear
                $('#csl').empty();
                for (var p in AllDptCslRelated[departmentSelected]) {
                    $('#csl').append($('<option>', { value: p, text: AllDptCslRelated[departmentSelected][p] }));
                }
                if (JsonPerformanceItems != undefined) DrawCharts();

            });
            return;
        }

        //      clear
        $('#csl').empty();
        for (var p in AllDptCslRelated[departmentSelected]) {
            $('#csl').append($('<option>', { value: p, text: AllDptCslRelated[departmentSelected][p] }));
        }
        if (JsonPerformanceItems != undefined) DrawCharts();

    }

    $('#csl,#contractCsl').change(function () {
        if (JsonPerformanceItems != undefined) DrawCharts();
    });

    $(document).on('click', 'caption', function () {
        exportToCSV2(this);
    });

});


function ConvertStringToDate(strDate) {
    var parts = strDate.split('/');
    return new Date(parts[2], parts[1] - 1, parts[0]);
}

function ConvertStringdateToISOstring(strDate) {
    var parts = strDate.split('/');
    return parts[2] + "-" + parts[1] + "-" + parts[0];
}



function exportToCSV2(CaptionElement) {


    var JqueryElement = $(CaptionElement);


    // find the table
    var tab_text = JqueryElement.next().html();

    // replace all ',' by '-' to avoid unwanted split
    tab_text = tab_text.replace(/,/g, '-');
    // issue with é
    tab_text = tab_text.replace(/é/g, 'e');


    tab_text = tab_text.replace(/<tbody>/g, '');
    tab_text = tab_text.replace(/<\/tbody>/g, '');
    tab_text = tab_text.replace(/<th>/g, '');
    tab_text = tab_text.replace(/<tr>/g, '');
    tab_text = tab_text.replace(/<td>/g, '');
    tab_text = tab_text.replace(/<td style="background-color:red;color:white">/g, '');

    tab_text = tab_text.replace(/<\/th>/g, ',');
    tab_text = tab_text.replace(/<\/tr>/g, '\r\n');
    tab_text = tab_text.replace(/<\/td>/g, ',');

    tab_text = tab_text.replace(/\t/g, '');
    tab_text = tab_text.replace(/\n/g, '');


    var allData = "data:text/csv;charset=utf-8," + tab_text;

    var encodedURI = encodeURI(allData);

    var link = document.createElement("a");
    link.setAttribute("href", encodedURI);
    var tableTitle = JqueryElement.text().replace(/ /g, '_') + ".csv";
    link.setAttribute("download", tableTitle);
    document.body.appendChild(link); // Required for FF
    link.click(); // This will download the data file named "my_data.csv".


}


