
let cl: SupplierAnalysis.ChartsLoader;

// initially 
google.charts.load('current', { packages: ['corechart', 'line', 'bar', 'table'] });
google.charts.setOnLoadCallback(function () {
    cl = new SupplierAnalysis.ChartsLoader();
});


$(document).ready(function () {


    $('#ParametersPanel select,[type=checkbox],[type=radio]').change(function () {
        cl.LoadUnderlyingDataFromController();
    });

    $('#resetFilter').click(function () {
        cl.LoadUnderlyingDataFromController();
    });


});




namespace SupplierAnalysis {




    export class ChartsLoader {


        constructor() {



            // load the core data in the controller
            this.LoadUnderlyingDataFromController();



        }


        yearOpacity = { currentFY: 1, previousFY: 0.5  , nextFY:0.75};


        DataCalculatedFromSelection: any;

        LoadUnderlyingDataFromController(): void {
            let urlPost: string = "/SuppliersAnalysis/CalculateAllDataBasedOnSelection";
            let objParam = this.getParameters();

            // on call back build the chart
            jQuery.post(urlPost,
                objParam,
                function (calculatedData: any) {
                    cl.DataCalculatedFromSelection = calculatedData;

                    cl.LoadColumnChart();
                    cl.LoadCumulativeLineChart();
                    cl.LoadTableChart();
                }
            );
        }



        getParameters(): any {

            let objParam = {};


            // build the parameter selection object sent to the controller

            objParam["EvaluationType"] = $('#EvaluationType').val();



            $('#narrowBy select').map(function (i, e) {
                objParam[e.id] = $(e).val();

            });



            $('.affix [type=checkbox]').map(function (i, e) {
                objParam[e.id] = $(e).prop('checked');
            });

            $('.affix [type=radio]').map(function (i, e) {
                objParam[e.id] = $(e).prop('checked');
            });

            return objParam;

        };


        LoadColumnChart(): void {



            let dataTab = cl.GenerateDataTableColChart();


            let chartOptions = {
                animation: { duration: 1500, easing: 'out' },
                legend: 'none',
                height: '600',
                point: { visible: 'true' },
                pointSize: 5,
                chartArea: { left: '40', width: '95%', height: '85%' },
                hAxis: { format: 'MMM yy', gridlines: { count: '12' } },
                tooltip: { isHtml: true, trigger: 'selection' },
                vAxis: { textPosition: 'in' }
            };



            let chart = new google.visualization.ColumnChart(document.getElementById('Column_Period_Only'));

            chart.setAction({
                id: 'exportToExcel',
                text: 'Export the period\'s data to Excel',
                action: function () {
                    let selection = chart.getSelection();
                    let dtSelected: Date = dataTab.getValue(selection[0].row, 0);
                    cl.ExportDataToExcel(dtSelected);
                }
            });



            if ($('#EvaluationType').val() == "evalCost") {
                let currencyFormatter = new google.visualization.NumberFormat({ prefix: '£', pattern: '###,###,###' });
                // get the total number of columns and apply the formatter to all of them except the date
                let nbCol = dataTab.getNumberOfColumns();
                for (let numCol = 1; numCol < nbCol; numCol++) {
                    currencyFormatter.format(dataTab, numCol);
                }

                let vAxisOption = { format: '£###,###,###', textPosition: 'in' };
                chartOptions["vAxis"] = vAxisOption;
            }

     


            chart.draw(dataTab, chartOptions);


        };



        ExportDataToExcel(projectedDate: Date): void {
            let urlPost: string = "/SuppliersAnalysis/GetDataToBeExportedToExcel";

            let strDate = projectedDate.toISOString();

            let objDate = { dated: strDate };

            projectedDate
            jQuery.post(urlPost,
                objDate,
                function (importedData: any) {
                    cl.GenerateCSVfile(importedData, projectedDate);
                }
            );


        };


        GenerateCSVfile(jsonData: any, projectedDate: Date): void {

            let csvString: string = "";

            for (let fy in jsonData) {

                // write the FY
                csvString += fy + '\r\n';

                for (let bs in jsonData[fy]) {

                    // write the booking stage
                    csvString += bs + '\r\n';

                    // case when no data
                    if (jsonData[fy][bs].length == 0)
                        break;


                    // write the headers
                    let firstObj = jsonData[fy][bs][0];

                    for (let p in firstObj) {
                        csvString += p + ',';
                    }

                    csvString += '\r\n';


                    // write the data
                    for (let obj of jsonData[fy][bs]) {

                        for (let p in obj) {

                            if (typeof obj[p] === 'string')
                                csvString += obj[p].replace(/,/g, '-').replace(/é/g, 'e').replace(/(\r\n|\n|\r)/gm, "") + ',';
                            else
                                csvString += obj[p] + ',';


                        }

                        csvString += '\r\n';

                    }

                    csvString += '\r\n';



                }





            }

            let allData = "data:text/csv;charset=utf-8," + csvString;

            let encodedURI = encodeURI(allData);

            let link = document.createElement("a");
            link.setAttribute("href", encodedURI);

            let periodComm = projectedDate.getDate() + "/" + (projectedDate.getMonth() + 1) + "/" + projectedDate.getFullYear();

            let tableTitle = "Data for period commencing " + periodComm;
            tableTitle = tableTitle.replace(/ /g, '_') + ".csv";



            link.setAttribute("download", tableTitle);
            document.body.appendChild(link); // Required for FF
            link.click(); // This will download the data file named tableTitle


        }

        LoadCumulativeLineChart(): void {


            let dataTab = cl.GenerateDataTableCumulativeChart();


            let ChartOptions = {
                animation: { duration: 1500, easing: 'out' },
                legend: 'none',
                height: '600',
                point: { visible: 'true' },
                pointSize: 5,
                chartArea: { left: '40', width: '95%', height: '85%' },
                hAxis: { format: 'MMM yy', gridlines: { count: '12' } },
                tooltip: { isHtml: true, trigger: 'selection' },
                vAxis: { textPosition: 'in' },
                crosshair: { trigger: 'selection', orientation: 'vertical', color: 'red ' },
                explorer: { actions: ['dragToZoom', 'rightClickToReset'] }
            };




            let chart = new google.visualization.LineChart(document.getElementById('Line_Cumulative'));




            if ($('#EvaluationType').val() == "evalCost") {

                let vAxisOption = { format: '£###,###,###', textPosition: 'in' };
                ChartOptions["vAxis"] = vAxisOption;
            }

       


            chart.draw(dataTab, ChartOptions);


            // set selection on the chart date just before today
            let today: Date = new Date();
            let rowSelected: number = 0;
            for (let numRow: number = 0; numRow < dataTab.getNumberOfRows(); numRow++) {
                let currentDate: Date = dataTab.getValue(numRow, 0);
                if (currentDate <= today)
                    rowSelected = numRow;
                else
                    break;

            }


            chart.setSelection([{ row: rowSelected, column: 1 }]);




        }


        GenerateDataTableCumulativeChart(): google.visualization.DataTable {


            let colData: any = cl.DataCalculatedFromSelection["Cumulative"];



            //let periodView: string = $('#radioMonthly').prop('checked') ? "Month" : "Week";
            let evalText: string = $('#EvaluationType option[value=' + $('#EvaluationType').val() + ']').text();
            let comparabilityPreviousCurrentFYs: boolean = $('#currentFY').prop('checked') && $('#previousFY').prop('checked');
            let IsCostEval: boolean = $('#EvaluationType').val() == "evalCost";




            let returnedTable = new google.visualization.DataTable();
            returnedTable.addColumn("date", "Date commencing");

            // define the column structure
            //  get the first object to define the columns structure
            let firstObj: any;
            for (let dt in colData) {
                firstObj = colData[dt];
                if (true) break;
            }

            for (let fy in firstObj) {
                for (let bs in firstObj[fy]) {
                    let title: string = bs;
                    returnedTable.addColumn("number", title);
                    returnedTable.addColumn({ 'type': 'string', 'role': 'tooltip', 'p': { 'html': true } });
                    returnedTable.addColumn({ type: 'string', role: 'style' });
                    if (fy == "currentFY" || fy =="nextFY")
                        returnedTable.addColumn({ type: 'boolean', role: 'certainty' });
                }
            }


            // fill the data
            let today: Date = new Date();
            for (let dt in colData) {

                let row = [];
                let currentDate = new Date(dt);

                row.push(currentDate);

                for (let fy in colData[dt]) {
                    for (let bs in colData[dt][fy]) {

                        let valNumber: number = colData[dt][fy][bs];
                        row.push(valNumber);

                        let valString: string = valNumber.toLocaleString();
                        if (IsCostEval)
                            valString = "£ " + valString;

                        // tooltip
                        let dtPattern = $('#radioMonthly').prop('checked') ? "MMMM" : "Do MMMM";
                        let nbDaysToAdd: number = $('#radioMonthly').prop('checked') ? 0 : 6;

                        let strDate = moment(dt).add("days", nbDaysToAdd).format(dtPattern);


                        let titleFY = $('#' + fy).attr('charttitle').replace(/_/g, ' ');
                        let opacityFY = cl.yearOpacity[fy];


                        let ttvariation: string = ``;
                        if (comparabilityPreviousCurrentFYs && fy == "currentFY") {
                            let valCurr: number = valNumber;
                            let valPrev: number = colData[dt]["previousFY"][bs];
                            let variationString: string;
                            if (valPrev != 0) {
                                let variation = 100 * (valCurr - valPrev) / valPrev;
                                variationString = variation.toFixed() + "%"
                            }
                            else
                                variationString = "+infinite %";

                            ttvariation = `
        <br />
        Variation from the previous financial year:
        <strong>`+ variationString + `</strong>
`;

                        }


                        let endofStr:string = $('#radioMonthly').prop('checked') ? 'end of ' : '';
                        let tt: string = `
    <div class="`+ bs + `" style="opacity: ` + opacityFY + `;">
        Until `+ endofStr +`<strong>` + strDate + `</strong>
        <br />
        <strong>`+ titleFY + `</strong>
        <br />
        <strong>`+ bs + ` bookings</strong>
        <br />
        Cumulative `+ evalText + `: <strong>` + valString + `</strong>` + ttvariation + `
    </div>
`;

                        row.push(tt);



                        // get the class color:
                        let classColor = $('.' + bs).css('background-color')
                        row.push('color: ' + classColor + '; opacity: ' + opacityFY + ';');

                        //  add the certainty boolean
                        if (fy == "currentFY")
                            row.push(currentDate <= today);

                        if (fy == "nextFY")
                            row.push(false);

                    }
                }

                returnedTable.addRow(row);

            }


            //let dtPattern: string = $('#radioMonthly').prop('checked') ? 'MMMM' : 'd MMMM';
            //let dateFormatter = new google.visualization.DateFormat({ pattern: dtPattern });
            //dateFormatter.format(returnedTable, 0);


            return returnedTable;


        }







        GenerateDataTableColChart(): google.visualization.DataTable {

            let colData: any = cl.DataCalculatedFromSelection["Period_Only"];

            let periodView: string = $('#radioMonthly').prop('checked') ? "Month" : "Week";


            let returnedTable = new google.visualization.DataTable();
            returnedTable.addColumn("date", "Date commencing");

            // define the column structure
            //  get the first object to define the columns structure
            let firstObj: any;
            for (let dt in colData) {
                firstObj = colData[dt];
                if (true) break;
            }

            for (let fy in firstObj) {
                for (let bs in firstObj[fy]) {
                    let title: string = $('#' + fy).attr('charttitle').replace(/_/g, ' ') + " " + bs + ' bookings for the ' + periodView;
                    returnedTable.addColumn("number", title);
                    returnedTable.addColumn({ type: 'string', role: 'style' });
                }
            }


            // fill the data
            for (let dt in colData) {
                let row = [];
                row.push(new Date(dt));

                for (let fy in colData[dt]) {
                    for (let bs in colData[dt][fy]) {
                        row.push(colData[dt][fy][bs]);

                        // get the class color:
                        let classColor = $('.' + bs).css('background-color')
                        let opacity = cl.yearOpacity[fy];
                        row.push('color: ' + classColor + '; opacity: ' + opacity + ';');

                    }
                }
                returnedTable.addRow(row);
            }


            let dtPattern: string = $('#radioMonthly').prop('checked') ? 'MMMM' : 'd MMMM';
            let dateFormatter = new google.visualization.DateFormat({ pattern: dtPattern });
            dateFormatter.format(returnedTable, 0);


            return returnedTable;
        };





        LoadTableChart(): void {


            let dataTab = cl.GenerateVariationsTable();


            let table = new google.visualization.Table(document.getElementById('Variation_Table'));



            // set the styling for the table elements
            var cssClassNames = {
                'headerCell': 'centering_text table-background',
                'tableCell': 'centering_text'
            };

            let chartOptions = { width: '100%', height: '700px', allowHtml: true,
                cssClassNames: cssClassNames};



            table.draw(dataTab, chartOptions);



        };


        GenerateVariationsTable(): google.visualization.DataTable {


            let suppData: any = cl.DataCalculatedFromSelection["Variations_Table"];

            let evalText: string = $('#EvaluationType option[value=' + $('#EvaluationType').val() + ']').text();
            let IsCostEval: boolean = $('#EvaluationType').val() == "evalCost";



            let dataTab = new google.visualization.DataTable();
            // add the columns
            dataTab.addColumn('string', 'Supplier or Chain');
            dataTab.addColumn('number', 'Total ' + evalText);
            dataTab.addColumn('number', 'YOY Variation');
            dataTab.addColumn('number', 'Total Confirmed bookings');
            dataTab.addColumn('number', 'YOY Variation');
            dataTab.addColumn('number', 'Cancelled bookings current year');
            dataTab.addColumn('number', 'Cancelled bookings previous year');



            // add the rows
            for (let suppItem of suppData) {
                let objData = suppItem["Value"];
                let tableRow = [];

                // add name
                tableRow.push(objData["currentFY"]["SupplierOrChainName"]);

                // add total
                tableRow.push(objData["currentFY"]["Total_Evaluated"]);

                // add variation
                let variation: number = (objData["currentFY"]["Total_Evaluated"] / objData["previousFY"]["Total_Evaluated"] - 1);
                tableRow.push(variation);

                // add confirmed bookings
                tableRow.push(objData["currentFY"]["nb_Bookings_Confirmed"]);

                // add variation
                variation = (objData["currentFY"]["nb_Bookings_Confirmed"] / objData["previousFY"]["nb_Bookings_Confirmed"] - 1);
                tableRow.push(variation);



                // add cancelled bookings current and previous year
                let ratioCancelled = (objData["currentFY"]["nb_Bookings_Cancelled"] / (objData["currentFY"]["nb_Bookings_Confirmed"] + objData["currentFY"]["nb_Bookings_Cancelled"]));
                tableRow.push(ratioCancelled);



                // add variation
                let ratioCancelledPreviousFY = (objData["previousFY"]["nb_Bookings_Cancelled"] / (objData["previousFY"]["nb_Bookings_Confirmed"] + objData["previousFY"]["nb_Bookings_Cancelled"]));

                tableRow.push(ratioCancelledPreviousFY);


                // add the row to the data table
                dataTab.addRow(tableRow);


            }


            // format the different columns
            let arrowFormatter = new google.visualization.ArrowFormat({ base: 0 });
            arrowFormatter.format(dataTab, 2);
            arrowFormatter.format(dataTab, 4);
            //arrowFormatter.format(dataTab, 6);

            let numberFormatter = new google.visualization.NumberFormat({ pattern: '# %' });
            numberFormatter.format(dataTab, 2);
            numberFormatter.format(dataTab, 4);
            numberFormatter.format(dataTab, 5);
            numberFormatter.format(dataTab, 6);


            if (IsCostEval) {
                let currencyFormatter = new google.visualization.NumberFormat({ prefix: '£', pattern: '###,###,###' });
                currencyFormatter.format(dataTab, 1);
            }


            return dataTab;
        };

    }

}









