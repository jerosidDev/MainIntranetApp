 class PendingEnquiryTableRow {
     Full_Reference:string = ""; 
     Booking_Name: string = "";
     Last_Stage: string = "";      // quoting or contracting
     Days_Before_Deadline: number = 0; 
     Last_Consultant: string =  "";
     BD_Consultant: string = "";
    }


// extract the table of unsent enquiries from the controller
var urlJSON: string = "/PerformanceOverview/RetrieveUnsentEnquiries";
$.getJSON(urlJSON, function (data) {
    GenerateGoogleTable(data)
});


function GenerateGoogleTable(UnsentEnquiriesTable: PendingEnquiryTableRow[]) {

    google.charts.load('current', { 'packages': ['table'] });
    google.charts.setOnLoadCallback(drawTable);

    function drawTable() {
        var data = new google.visualization.DataTable();

        // add the columns
        var RowTest: PendingEnquiryTableRow = new PendingEnquiryTableRow();
        for (var prop in RowTest)
        {
            data.addColumn(typeof RowTest[prop] , prop);
        }

        UnsentEnquiriesTable.forEach(function (dataRow) {
            var d: any = $.map(dataRow, function (value) {
                return value;
            });
            data.addRow(d);
        });



        var table = new google.visualization.Table(document.getElementById('PendingTable_div'));
        var formatter = new google.visualization.ColorFormat();
        formatter.addRange(-200000, 0, 'white', 'red');
        formatter.addRange(2, 200000, 'black', 'green');
        formatter.addRange(-1, 2, 'white', 'orange');
        formatter.format(data, 3);

        table.draw(data, { showRowNumber: true, width: '100%', height: '100%', allowHtml: true });

    }

};




