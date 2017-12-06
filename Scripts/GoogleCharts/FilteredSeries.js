// travel data sorted by travel date
var TravelData = { weekly: {}, monthly: {} };
// travel data form the previous year which will be compared in the current year (year on year)
var TravelDataYOY = { weekly: {}, monthly: {} };

// Enquiries sorted by date entered
var EnquiriesEntered = { weekly: {}, monthly: {} };
// Enquiries entered data from the previous year which will be compared in the current year (year on year)
var EnquiriesEnteredYOY = { weekly: {}, monthly: {} };


// Enquiries finalised sorted by date confirmed
var EnquiriesFinalised = { weekly: {}, monthly: {} };
// Enquiries finalised data from the previous year which will be compared in the current year (year on year)
var EnquiriesFinalisedYOY = { weekly: {}, monthly: {} };


// Missing information sorted by travel date
var MissingData = { weekly: {}, monthly: {} };
// Enquiries finalised data from the previous year which will be compared in the current year (year on year)
var MissingDataYOY = { weekly: {}, monthly: {} };



// all column charts on the view and their options
var MainChart;
var EnquiriesChart;
var FinalisedChart;
var MissingdataChart;




// load the data from the server asynchronously
var MaxAjaxCalls = 0;
var ActualAjaxCalls = 0;

MaxAjaxCalls++;
$.getJSON('/GroupsTravellingOverview/ReturnJSONMissingInformation', function (data4) {
    MissingData["weekly"] = data4;
    ActualAjaxCalls++;
    if (MaxAjaxCalls === ActualAjaxCalls)
        ExecuteAfterJSONloading();
});

MaxAjaxCalls++;
$.getJSON('/GroupsTravellingOverview/ReturnJSONEnquiriesFinalised', function (data3) {
    EnquiriesFinalised["weekly"] = data3;
    ActualAjaxCalls++;
    if (MaxAjaxCalls === ActualAjaxCalls)
        ExecuteAfterJSONloading();
});

MaxAjaxCalls++;
$.getJSON('/GroupsTravellingOverview/ReturnJSONEnquiriesEntered', function (data2) {
    EnquiriesEntered["weekly"] = data2;
    ActualAjaxCalls++;
    if (MaxAjaxCalls === ActualAjaxCalls)
        ExecuteAfterJSONloading();
});

MaxAjaxCalls++;
$.getJSON('/GroupsTravellingOverview/ReturnJSONTravelData', function (data) {
    TravelData["weekly"] = data;
    ActualAjaxCalls++;
    if (MaxAjaxCalls === ActualAjaxCalls)
        ExecuteAfterJSONloading();
});



function ExecuteAfterJSONloading() {
    GenerateAllDataYOY();
    GenerateAllDataMonthly();
    LoadgooglePackage();
}


// load the google chart package once the travel data are here
function LoadgooglePackage() {

    // load the google package for the linechart
    google.charts.load('current', { 'packages': ['corechart', 'table'] });
    google.charts.setOnLoadCallback(drawAllCharts);

}



function drawAllCharts() {

    // draw the line chart
    drawColumnCharts();

    // draw the table
    drawTableChart();

}




// create the table chart  under the line chart
function drawTableChart() {


    // set the styling for the table elements
    var cssClassNames = {
        'headerCell': 'centering_text white_on_blue',
        'tableCell': 'centering_text'
    };


    //if (TableOptions == undefined) {
    TableOptions = {
        allowHtml: true,/* width: '100%',*/ height: '500px', sortAscending: false, sortColumn: '1', 'cssClassNames': cssClassNames
    };
    //}


    // handle to the table_div
    //if (TableChart == undefined) {
    //    TableChart = new google.visualization.Table(document.getElementById('table_div'));
    //}
    var TableChart = new google.visualization.Table(document.getElementById('table_div'));

    var dataTable = dataTableCalculation();





    // formatting for the variation column
    var colVariation = 3;
    var formatter = new google.visualization.ArrowFormat({ base: '0' });
    formatter.format(dataTable, colVariation); // Apply formatter to second column

    var formatter2 = new google.visualization.NumberFormat({ pattern: '# %' });
    formatter2.format(dataTable, colVariation); // Apply formatter to second column

    var formatter3 = new google.visualization.ColorFormat();
    formatter3.addRange(null, -0.2, 'red');
    formatter3.addRange(-0.2, -0.01, 'orange');
    formatter3.addRange(0, null, 'green');
    formatter3.format(dataTable, colVariation); // Apply formatter to second column


    // draw the table chart
    TableChart.draw(dataTable, TableOptions);

    // on selection of a row will select a client on the parameters panel
    google.visualization.events.addListener(TableChart, 'select', highlightClient);

    function highlightClient() {

        var selection = TableChart.getSelection();
        if (selection.length != 0) {

            // find the client code in the property of cell 1
            var clientCode = dataTable.getRowProperties(selection[0].row)['value'];
            //$('#' + clientCode).prop('selected', true);
            $('[value=' + clientCode + ']').prop('selected', true);
            $('[name="AgentCode"]').trigger("change");
            $(window).scrollTop(0);
        }
    }


    //$(".dp").change(function () {
    //    drawTableChart();
    //    var scrollPos = $("#travelPeriodPanel").prop('offsetTop');
    //    $(window).scrollTop(scrollPos);
    //});


    $(".dp").on("change", function () {
        drawTableChart();
        var scrollPos = $("#travelPeriodPanel").prop('offsetTop');
        $(window).scrollTop(scrollPos);
    });


}





// calculate the dataTable based on the parameters selection and the travel period selected
function dataTableCalculation() {

    var dataTable = new google.visualization.DataTable();

    dataTable.addColumn('string', 'Client');
    dataTable.addColumn('number', 'Total enquiries previous year');
    dataTable.addColumn('number', 'Total enquiries');
    dataTable.addColumn('number', 'Variation');

    var dataTravel = {};      // each property is an a AgentCode : [ AgentName , number of bookings prev year ,number of bookings curr year ]


    // get the select which don't have the txt "All" and which doesn't have the name "AgentCode"
    var listSelect = $('select[name!=AgentCode]').filter(function () {
        return this.value != "All";
    });


    var beginDate = ConvertStringToDate($('#beginDate').val());
    var endDate = ConvertStringToDate($('#endDate').val());
    var beginDatePY = ConvertStringToDate($('#beginDatePY').text());
    var endDatePY = ConvertStringToDate($('#endDatePY').text());;
    for (wc in TravelData["weekly"]) {

        if (TravelData["weekly"][wc].length != 0) {



            // filter bkgsList based on the selection
            var bkgsList = TravelData["weekly"][wc].filter(function (objBkg) {
                if (listSelect.length != 0) {
                    for (var s = 0 ; s < listSelect.length ; s++) {
                        var filterName = listSelect[s].name;
                        var filterValue = listSelect[s].value;
                        if (objBkg[filterName].trim() != filterValue.trim()) return false;
                    }
                }
                return true;
            });


            if (bkgsList.length != 0) {

                for (var numObj = 0; numObj < bkgsList.length; numObj++) {

                    var TravelDate = ConvertStringToDate(bkgsList[numObj]["TravelDate"]);
                    var agentCode = bkgsList[numObj]["AgentCode"];

                    // current travel period tested
                    if (TravelDate >= beginDate && TravelDate <= endDate) {
                        // initialisation if non-existent agent
                        if (dataTravel[agentCode] == undefined) {
                            dataTravel[agentCode] = [bkgsList[numObj]["AgentName"], 0, 0];
                        }
                        // increment for current year
                        dataTravel[agentCode][2]++;
                    }

                    // previous year travel period tested
                    if (TravelDate >= beginDatePY && TravelDate <= endDatePY) {
                        // initialisation if non-existent agent
                        if (dataTravel[agentCode] == undefined) {
                            dataTravel[agentCode] = [bkgsList[numObj]["AgentName"], 0, 0];
                        }
                        // increment for previous year
                        dataTravel[agentCode][1]++;
                    }


                }

            }



        }

    }


    // fill the data table 
    for (agentCode in dataTravel) {

        var dataRow = dataTravel[agentCode];

        // variation which will be displayed as %
        var variation = (dataRow[2] - dataRow[1]) / dataRow[1];
        dataRow.push(variation);


        //// to add style to a cells: 
        //var numColName = 0;
        ////var tableCell = { v: dataRow[numColName], f: agentCode, p: { style: 'border: 10px solid green;' } };
        //dataRow[numColName] = tableCell;



        var numRow = dataTable.addRow(dataRow);

        // add the property id to identify the agent on selection of a line
        dataTable.setRowProperties(numRow, { 'value': agentCode })

    }

    // sort the clients by total bookings current year + previous year

    return dataTable;


}

// used to draw the column charts
function drawColumnCharts() {


    ChartOptions = {
        animation: { duration: 1500, easing: 'out' }, legend: 'none',
        /*legend: { position: 'top', alignment: 'center', maxLines: '2' },*/ height: '600', point: { visible: 'true' }, pointSize: 5, chartArea: { left: '40', width: '90%' }, hAxis: { format: 'MMM yy', gridlines: { count: '12' } }, tooltip: { isHtml: true, trigger: 'selection' }, /* title: 'Enquiries by Travel date', */ isStacked: true
    };



    //  main chart drawing
    if (MainChart == undefined) {
        // MainChart has to be kept global if I want to keep the animation
        MainChart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
    }
    ChartOptions.title = 'Enquiries by Travel date';
    var dataViewFiltered = GenerateDataFromSelection6("TravelDate");
    MainChart.draw(dataViewFiltered, ChartOptions);


    // enquiries entered chart drawing
    if (EnquiriesChart == undefined) {
        // MainChart has to be kept global if I want to keep the animation
        EnquiriesChart = new google.visualization.ColumnChart(document.getElementById('enquiriesEntered_div'));
    }
    ChartOptions.title = 'Enquiries by date Entered';
    var dataTableEnquiriesEntered = GenerateDataFromSelection6("DateEntered");
    EnquiriesChart.draw(dataTableEnquiriesEntered, ChartOptions);



    // enquiries finalised chart drawing
    if (FinalisedChart == undefined) {
        // MainChart has to be kept global if I want to keep the animation
        FinalisedChart = new google.visualization.ColumnChart(document.getElementById('enquiriesFinalised_div'));
    }
    ChartOptions.title = 'Enquiries by Finalised date for sent,confirmed and cancelled enquiries, the travel date is taken as default if the finalised date is not available';
    var dataTableEnquiriesFinalised = GenerateDataFromSelection6("DateFinalised");
    FinalisedChart.draw(dataTableEnquiriesFinalised, ChartOptions);


    // missing data chart drawing
    if (MissingdataChart == undefined) {
        // MainChart has to be kept global if I want to keep the animation
        MissingdataChart = new google.visualization.ColumnChart(document.getElementById('missingData_div'));
    }
    ChartOptions.title = 'Missing data ordered by travel dates: where the finalised date or the series reference is missing';
    var dataTableMissingData = GenerateDataFromSelection6("MissingData");
    MissingdataChart.draw(dataTableMissingData, ChartOptions);





}


// generate a datatable based on the user's selection , add the html elements on the chart
// now it will also show the tooltips for the previous year
// the previous and current years will have their own columns stacked:
//      the trick is to use a different day (or time) for the previous year and hide the rows if unselected 
// the columns are not hidden anymore but generated if needed
//  added style roles for columns
// the checkbox "All"  and "Unconfirmed" "Confirmed" "Cancelled" are mutually excluding on the page
// the source data can be :
//      by travel dates
//      by enquiry date entered
function GenerateDataFromSelection6(AnalysisDate) {

    "use strict";


    var today = new Date();
    var currentFY = today.getMonth() === 0 ? today.getFullYear() - 1 : today.getFullYear();
    var firstDayFY = new Date(currentFY, 1, 1); // 1st february of the current financial year
    var todayLastYear = today;
    todayLastYear.setFullYear(today.getFullYear() - 1);


    var periodSelected = $('#radioMonthly').prop("checked") ? "monthly" : "weekly";


    // set up of the datasource and the week commencing :
    var dataSourceCurrY;   // current financial year
    var dataSourcePrevY;   // previous financial year
    var StartDate;          // date from which the chart will start
    var DateDisplayed;      // date type which will be displayed on the chart
    var posChart;           // the position of the chart from top to bottom
    switch (AnalysisDate) {
        case "TravelDate":
            dataSourceCurrY = TravelData[periodSelected];
            dataSourcePrevY = TravelDataYOY[periodSelected];
            StartDate = firstDayFY;
            DateDisplayed = "TravelDate";
            posChart = 0;
            break;
        case "DateEntered":
            dataSourceCurrY = EnquiriesEntered[periodSelected];
            dataSourcePrevY = EnquiriesEnteredYOY[periodSelected];
            StartDate = firstDayFY;
            DateDisplayed = "DateEntered";
            posChart = 1;
            break;
        case "DateFinalised":
            dataSourceCurrY = EnquiriesFinalised[periodSelected];
            dataSourcePrevY = EnquiriesFinalisedYOY[periodSelected];
            StartDate = firstDayFY;
            DateDisplayed = "DateFinalised";
            posChart = 2;
            break;
        case "MissingData":
            dataSourceCurrY = MissingData[periodSelected];
            dataSourcePrevY = MissingDataYOY[periodSelected];
            StartDate = firstDayFY;
            DateDisplayed = "TravelDate";
            posChart = 3;
            break;
    }



    // fill the data table
    var InitDataRow = [];   //  template used to fill each row of the data table







    // define the columns for the data table, the role of the column defines its colour
    var indexList = {};
    var dataTab = new google.visualization.DataTable();
    dataTab.addColumn('date', 'week commencing');
    InitDataRow.push(StartDate);

    var yearOpacity;

    // current financial year
    if ($('#currentYear')[0].checked) {

        //var styleYear = 'stroke-width: 1;stroke-color: #01a0ff';
        //var styleYear = '';
        yearOpacity = 1;

        if ($('#cbAll')[0].checked) {
            if (indexList["currentYear"] == undefined) indexList["currentYear"] = {};
            indexList["currentYear"]["All"] = dataTab.addColumn('number', 'All current year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(0, 0, 139); opacity: ' + yearOpacity + ';');
        }


        if ($('#cbCancelled')[0].checked) {
            if (indexList["currentYear"] == undefined) indexList["currentYear"] = {};
            indexList["currentYear"]["Cancelled"] = dataTab.addColumn('number', 'Cancelled current year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(255, 0, 0); opacity: ' + yearOpacity + ';');
        }


        if ($('#cbConfirmed')[0].checked) {
            if (indexList["currentYear"] == undefined) indexList["currentYear"] = {};
            indexList["currentYear"]["Confirmed"] = dataTab.addColumn('number', 'Confirmed current year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(0, 128, 0); opacity: ' + yearOpacity + ';');
        }

        if ($('#cbUnconfirmed')[0].checked) {
            if (indexList["currentYear"] == undefined) indexList["currentYear"] = {};
            indexList["currentYear"]["Unconfirmed"] = dataTab.addColumn('number', 'Unconfirmed current year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(255, 140, 0); opacity: ' + yearOpacity + ';');
        }




    }


    // previous financial year
    if ($('#previousYear')[0].checked) {

        //var styleYear = 'stroke-width: 5;stroke-color: #cf001d';
        //var styleYear = 'opacity: ' + opacityPrevYear + ';';
        yearOpacity = 0.5;

        if ($('#cbAll')[0].checked) {
            if (indexList["previousYear"] == undefined) indexList["previousYear"] = {};
            indexList["previousYear"]["All"] = dataTab.addColumn('number', 'All previous year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(0, 0, 139); opacity: ' + yearOpacity + ';');
        }



        if ($('#cbCancelled')[0].checked) {
            if (indexList["previousYear"] == undefined) indexList["previousYear"] = {};
            indexList["previousYear"]["Cancelled"] = dataTab.addColumn('number', 'Cancelled previous year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(255, 0, 0); opacity: ' + yearOpacity + ';');
        }


        if ($('#cbConfirmed')[0].checked) {
            if (indexList["previousYear"] == undefined) indexList["previousYear"] = {};
            indexList["previousYear"]["Confirmed"] = dataTab.addColumn('number', 'Confirmed previous year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(0, 128, 0); opacity: ' + yearOpacity + ';');
        }

        if ($('#cbUnconfirmed')[0].checked) {
            if (indexList["previousYear"] == undefined) indexList["previousYear"] = {};
            indexList["previousYear"]["Unconfirmed"] = dataTab.addColumn('number', 'Unconfirmed previous year');
            InitDataRow.push(0);
            dataTab.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
            InitDataRow.push('');
            dataTab.addColumn({ type: 'string', role: 'style' });
            InitDataRow.push('color: rgb(255, 140, 0); opacity: ' + yearOpacity + ';');
        }




    }



    // search the first week commencing and then count for the next 52weeks

    // get the select which don't have the txt "All"
    var listSelect = $('select').filter(function () {
        return this.value != "All";
    });

    var periodNumber = 0;
    for (var pc in dataSourceCurrY) {       // pc period commencing

        // if periodSelected is monthly convert the month to a date being the first of the month

        var pcDate;
        var colXOffset;
        var maxPeriodNumber;
        var strDate;  //  used for the HTML tooltips
        var strPeriodType;
        if (periodSelected === "monthly") {
            // pc is the name of a month
            var numMonth = monthNumber(pc);
            var y = numMonth === 0 ? currentFY + 1 : currentFY;
            pcDate = new Date(y, numMonth, 1)
            maxPeriodNumber = 12;
            colXOffset = 10;
            strDate = pc;
            strPeriodType = "Month:";
        }


        if (periodSelected === "weekly") {
            pcDate = new Date(pc);
            maxPeriodNumber = 52;
            colXOffset = 2;
            strDate = pcDate.getDate() + "/" + (pcDate.getMonth() + 1) + "/" + pcDate.getFullYear();
            strPeriodType = "Week commencing:";
        }



        if (pcDate > StartDate || periodSelected === "monthly") {

            for (var p in indexList) {
                if ($('#' + p)[0].checked) addRowFromYearType(p);
            }


            //  create a separate row depending on _yearType
            function addRowFromYearType(_yearType) {

                // initialise dataRow 
                var dataRow = InitDataRow.slice();
                dataRow[0] = pcDate;


                var bkgsList = _yearType == "currentYear" ? dataSourceCurrY[pc] : dataSourcePrevY[pc];

                // will define for col 5 and 6 which property of each data object to select
                var col5ObjectProp = AnalysisDate == "MissingData" ? "DateMissing" : "EstimatedTurnover";
                var col6ObjectProp = AnalysisDate == "MissingData" ? "SeriesReferenceMissing" : _yearType == 'previousYear' ? "Pax" : "SalesUpdate";


                // filter bkgsList based on the selection
                bkgsList = bkgsList.filter(function (objBkg) {
                    if (listSelect.length != 0) {
                        for (var s = 0 ; s < listSelect.length ; s++) {
                            var filterName = listSelect[s].name;
                            var filterValue = listSelect[s].value;
                            if (objBkg[filterName].trim() != filterValue.trim()) return false;
                        }
                    }
                    return true;
                });


                // generate the HTML for the tooltips
                //  modify dataRow to reflect this
                // the tooltip are also generated for the previous year

                var redStyle = 'style="background-color:red;color:white"';

                if (bkgsList.length != 0) {

                    // filter object by object based on the selection
                    for (var numBkg = 0; numBkg < bkgsList.length; numBkg++) {
                        //var bkgQualify = true;
                        var objBkg = bkgsList[numBkg];

                        // add a count to dataRow where appropriate if objBkg qualifies 
                        // update the tooltip only if yearOrder is currentYear

                        // booking stage is "All" by default
                        var iStage = indexList[_yearType]["All"];
                        if (iStage == undefined) iStage = indexList[_yearType][objBkg["BookingStage"]];   // booking stage applicable

                        if (iStage != undefined) {
                            // increment for the booking stage applicable


                            dataRow[iStage]++;


                            //// update the tooltip for "All"and the booking stage applicable only if it is currentYear
                            //if (yearOrder == "currentYear") {

                            var strHTML = "<tr>";

                            // add reference
                            strHTML = strHTML + "<td> " + objBkg["Full_Reference"] + " </td>";


                            // add Booking name
                            strHTML = strHTML + "<td> " + objBkg["BookingName"] + " </td>";

                            // add status
                            strHTML = strHTML + "<td> " + objBkg["Status"] + " </td>";

                            // add travel date or date entered or date finalised
                            strHTML = strHTML + "<td> " + objBkg[DateDisplayed] + " </td>";

                            // add estimated turnover or missing data

                            //strHTML = strHTML + "<td " + redStyle + "> " + objBkg[col5ObjectProp] + " </td>";

                            if (objBkg[col5ObjectProp] == "Yes") {
                                strHTML = strHTML + "<td " + redStyle + "> " + objBkg[col5ObjectProp] + " </td>";
                            }
                            else
                                strHTML = strHTML + "<td> " + objBkg[col5ObjectProp] + " </td>";


                            // add sales update or pax depending on the year selected or missing data
                            //strHTML += "<td> " + objBkg[col6ObjectProp] + " </td>";

                            if (objBkg[col6ObjectProp] == "Yes") {
                                strHTML = strHTML + "<td " + redStyle + "> " + objBkg[col6ObjectProp] + " </td>";
                            }
                            else
                                strHTML = strHTML + "<td> " + objBkg[col6ObjectProp] + " </td>";


                            // add consultant
                            strHTML = strHTML + "<td> " + objBkg["Consultant"] + " </td>";

                            // close the html line
                            strHTML = strHTML + "</tr>";

                            // save the line of the tooltip
                            dataRow[iStage + 1] += strHTML;


                        }


                    }


                }




                // finish the HTML tooltips


                // for each tooltip
                for (var p in indexList[_yearType]) {
                    var i = indexList[_yearType][p];
                    //}
                    //for (var i = 1 ; i < numberCol ; i = i + 2) {

                    var title = dataTab.getColumnLabel(i);
                    var bkgsTotal = dataRow[i];
                    //var bgColor = dataTab.getColumnId(i);
                    var iBlankSpace = dataRow[i + 2].indexOf(" ");
                    var iSemiColon = dataRow[i + 2].indexOf(";");
                    var bgColor = dataRow[i + 2].substring(iBlankSpace + 1, iSemiColon);
                    var opacity = dataRow[i + 2].substring(iSemiColon + 1).replace("opacity:", "").replace(";", "").trim();

                    // turn the background color into rgba
                    bgColor = bgColor.substring(0, bgColor.lastIndexOf(")")).replace("rgb", "rgba") + "," + opacity + ")";

                    var txtColor = opacity == "1" ? "white" : "black";


                    //var opacityYear = _yearType == "previousYear" ? opacityPrevYear : opacityCurrYear;
                    var titleDate = "Travel Date";
                    var col5Title = "Estimated turnover";

                    // not working with IE
                    //var paxOrSalesUpdate = title.includes('previous') ? 'Total pax' : 'Sales update';
                    var col6Title = _yearType != "previousYear" ? 'Sales update' : 'Total pax';


                    // change titles according to the type of analysis

                    switch (AnalysisDate) {
                        case 'DateFinalised':
                            switch (p) {
                                case "Unconfirmed":
                                    titleDate = "Date Sent";
                                    break;
                                case "Confirmed":
                                    titleDate = "Date Confirmed";
                                    break;
                                case "Cancelled":
                                    titleDate = "Date Cancelled";
                                    break;
                                default:
                                    titleDate = "Date Finalised";
                            }
                            break;
                        case 'DateEntered':
                            titleDate = "Date Entered";
                            break;
                        case 'MissingData':
                            titleDate = "Travel Date";
                            col5Title = "Finalised date missing";
                            col6Title = "Series reference missing";
                            break;
                        default:
                            titleDate = "Travel Date";
                    }


                    var left1 = -400;
                    var left2 = 700;
                    if ($(window).width() < 1500) {
                        left1 = $(window).width();
                        left2 = $(window).width();
                    }

                    var toTheLeft = "<style> div.google-visualization-tooltip {  position:absolute !important; top: 0px !important; left:" + left1 + "px !important; z-index: 10 !important; } </style>";
                    var toTheRight = "<style> div.google-visualization-tooltip {  position:absolute !important; top: 0px  !important; left:" + left2 + "px !important; z-index: 10 !important;  } </style>";



                    var beginningHTML = '<p style="background-color: ' + bgColor + '; color: ' + txtColor + ';" id="titleP">' + strPeriodType + ' <b>' + strDate + '</b> , ' + title + ': <b>' + bkgsTotal + '</b></p><a style="background-color: gold; font-size: x-large;" href="#" id="' + posChart + '" onclick="exportToCSV(' + posChart + ');"> Download table</a><style> #customers , #titleP { font-family: "Trebuchet MS", Arial, Helvetica, sans-serif; border-collapse: collapse; width: 100%; table-layout:fixed; width:680px; } #customers td, #customers th { border: 1px solid #ddd; padding: 8px;  word-wrap:break-word; } #customers tr:nth-child(even) {background-color: #f2f2f2; } #customers tr:hover {background-color: #ddd; } #titleP,#customers th { padding-top: 12px; padding-bottom: 12px; text-align: left; } </style><table id="customers"><tr><th>Reference</th><th>Booking name</th><th>Status</th><th>' + titleDate + '</th><th>' + col5Title + '</th><th>' + col6Title + '</th><th>Consultant</th></tr>';





                    if (periodNumber < (maxPeriodNumber / 2 - 2)) {
                        beginningHTML = toTheRight + beginningHTML;
                    }
                    else {
                        beginningHTML = toTheLeft + beginningHTML;
                    }

                    dataRow[i + 1] = beginningHTML + dataRow[i + 1] + '</table>';

                }

                // add 2 days if previousYear
                if (_yearType == "previousYear") dataRow[0] = addDays(pcDate, colXOffset);
                dataTab.addRow(dataRow);

            }





            periodNumber++;
        }
        if (periodNumber >= maxPeriodNumber) break;
    }


    return dataTab
}



$(document).ready(function () {

    // redraw the line chart : when change of ticks or selection (not date)
    // redraw the table chart : on change on selection or timeframe

    $("select").change(function () {
        drawAllCharts();
    });

    $("[type='checkbox']").change(function () {

        // the checkbox "All"  and "Unconfirmed" "Confirmed" "Cancelled" are mutually excluding on the page
        if (this.checked) {
            switch (this.id) {
                case "cbAll":
                    $('#cbUnconfirmed').prop('checked', false);
                    $('#cbConfirmed').prop('checked', false);
                    $('#cbCancelled').prop('checked', false);
                    break;
                case "cbUnconfirmed":
                case "cbConfirmed":
                case "cbCancelled":
                    $('#cbAll').prop('checked', false);
                    break;
            }
        }

        // draw the charts based on the selection
        drawColumnCharts();
    });


    $("[type='radio']").change(function () {

        // draw the charts based on the selection
        drawColumnCharts();
    });


    //$(".dp").change(function () {
    //    drawTableChart();
    //    var scrollPos = $("#travelPeriodPanel").prop('offsetTop');
    //    $(window).scrollTop(scrollPos);
    //});


});

function GenerateAllDataYOY() {

    // generate a parallel list of object indexed by the current year's weeks commencing but containing previous year's data

    TravelDataYOY["weekly"] = {};

    for (wc in TravelData["weekly"]) {
        for (var numObj = 0; numObj < TravelData["weekly"][wc].length; numObj++) {
            wcYOY = TravelData["weekly"][wc][numObj]["WillBeCompared1YearForwardWith"];

            if (TravelDataYOY["weekly"][wcYOY] === undefined) TravelDataYOY["weekly"][wcYOY] = [];

            TravelDataYOY["weekly"][wcYOY].push(TravelData["weekly"][wc][numObj]);
        }
    }


    // generate a parallel list of object indexed by the current year's weeks commencing but containing previous year's data for the enquiries entered

    EnquiriesEnteredYOY["weekly"] = {};
    for (wc in EnquiriesEntered["weekly"]) {
        for (var numObj = 0; numObj < EnquiriesEntered["weekly"][wc].length; numObj++) {
            wcYOY = EnquiriesEntered["weekly"][wc][numObj]["WillBeCompared1YearForwardWithEnt"];

            if (EnquiriesEnteredYOY["weekly"][wcYOY] === undefined) EnquiriesEnteredYOY["weekly"][wcYOY] = [];

            EnquiriesEnteredYOY["weekly"][wcYOY].push(EnquiriesEntered["weekly"][wc][numObj]);
        }
    }



    // generate a parallel list of object indexed by the current year's weeks commencing but containing previous year's data for the enquiries finalised

    EnquiriesFinalisedYOY["weekly"] = {};
    for (wc in EnquiriesFinalised["weekly"]) {
        for (var numObj = 0; numObj < EnquiriesFinalised["weekly"][wc].length; numObj++) {
            wcYOY = EnquiriesFinalised["weekly"][wc][numObj]["WillBeCompared1YearForwardWithFin"];

            if (EnquiriesFinalisedYOY["weekly"][wcYOY] === undefined) EnquiriesFinalisedYOY["weekly"][wcYOY] = [];

            EnquiriesFinalisedYOY["weekly"][wcYOY].push(EnquiriesFinalised["weekly"][wc][numObj]);
        }
    }



    // generate a parallel list of object indexed by the current year's weeks commencing but containing previous year's bookings for the missing data

    MissingDataYOY["weekly"] = {};
    for (wc in MissingData["weekly"]) {
        for (var numObj = 0; numObj < MissingData["weekly"][wc].length; numObj++) {
            wcYOY = MissingData["weekly"][wc][numObj]["WillBeCompared1YearForwardWithEnt"];

            if (MissingDataYOY["weekly"][wcYOY] === undefined) MissingDataYOY["weekly"][wcYOY] = [];

            MissingDataYOY["weekly"][wcYOY].push(MissingData["weekly"][wc][numObj]);
        }
    }





}


// Group all data per month and create monthly datasource
function GenerateAllDataMonthly() {

    "use strict";


    var today = new Date();
    var currentFY = today.getMonth() === 0 ? today.getFullYear() - 1 : today.getFullYear();
    var firstDayFY = new Date(currentFY, 1, 1); // 1st february of the current financial year
    var lastDayFY = new Date(currentFY + 1, 0, 31); // 31st january 

    var firstDayPrevFY = new Date(currentFY - 1, 1, 1); // 1st february of the previous financial year
    var lastDayPrevFY = new Date(currentFY, 0, 31); // 31st january of the previous financial year



    var dataMapping = [{ stringDateEval: "TravelDate", dataSource: TravelData, dataSourceYOY: TravelDataYOY }, { stringDateEval: "DateEntered", dataSource: EnquiriesEntered, dataSourceYOY: EnquiriesEnteredYOY }, { stringDateEval: "DateFinalised", dataSource: EnquiriesFinalised, dataSourceYOY: EnquiriesFinalisedYOY }, { stringDateEval: "TravelDate", dataSource: MissingData, dataSourceYOY: MissingDataYOY }];




    for (var numDM = 0; numDM < dataMapping.length; numDM++) {

        var objDM = dataMapping[numDM];

        var stringDateEval = objDM["stringDateEval"];
        var dataSource = objDM["dataSource"];
        var dataSourceYOY = objDM["dataSourceYOY"];

        // create a continuous line of month from february to january
        dataSource["monthly"] = {};
        for (var numMonth = 1; numMonth < 12; numMonth++) {
            dataSource["monthly"][monthName(numMonth)] = [];
        }
        dataSource["monthly"][monthName(0)] = [];
        dataSourceYOY["monthly"] = {};
        for (var numMonth = 1; numMonth < 12; numMonth++) {
            dataSourceYOY["monthly"][monthName(numMonth)] = [];
        }
        dataSourceYOY["monthly"][monthName(0)] = [];


        // allocate the objects to the appropriate months if applicable
        for (wc in dataSource["weekly"]) {
            for (var numObj = 0; numObj < dataSource["weekly"][wc].length; numObj++) {
                var dateEval = ConvertStringToDate(dataSource["weekly"][wc][numObj][stringDateEval]);
                var _monthName = monthName(dateEval.getMonth());
                if (firstDayFY <= dateEval && dateEval <= lastDayFY) {
                    // add the object to where it belongs
                    dataSource["monthly"][_monthName].push(dataSource["weekly"][wc][numObj]);
                }
                if (firstDayPrevFY <= dateEval && dateEval <= lastDayPrevFY) {
                    // add the object to where it belongs
                    dataSourceYOY["monthly"][_monthName].push(dataSource["weekly"][wc][numObj]);
                }
            }
        }

    }


}

function ConvertStringToDate(strDate) {
    var parts = strDate.split('/');
    return new Date(parts[2], parts[1] - 1, parts[0]);
}


function addDays(startDate, numberOfDays) {
    return new Date(startDate.getTime() + (numberOfDays * 24 * 60 * 60 * 1000));
}



function monthName(numMonth) {

    var month = new Array();
    month[0] = "January";
    month[1] = "February";
    month[2] = "March";
    month[3] = "April";
    month[4] = "May";
    month[5] = "June";
    month[6] = "July";
    month[7] = "August";
    month[8] = "September";
    month[9] = "October";
    month[10] = "November";
    month[11] = "December";


    return month[numMonth];

}



function monthNumber(monthName) {

    var month = { "January": 0, "February": 1, "March": 2, "April": 3, "May": 4, "June": 5, "July": 6, "August": 7, "September": 8, "October": 9, "November": 10, "December": 11 };


    return month[monthName];

}


function exportToCSV(posChart) {



    // find the table which is the next sibling to <a> with posChart id
    var tab_text = $('#' + posChart).nextAll().last().html();

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


    var data_type = 'data:application/csv';
    $('#' + posChart).attr('href', data_type + ',' + encodeURIComponent(tab_text));
    $('#' + posChart).attr('download', 'CSVexported.csv');

}

