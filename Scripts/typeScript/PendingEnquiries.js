class PendingEnquiryTableRow {
    constructor() {
        this.Full_Reference = "";
        this.Booking_Name = "";
        this.Last_Stage = ""; // quoting or contracting
        this.Days_Before_Deadline = 0;
        this.Last_Consultant = "";
        this.BD_Consultant = "";
    }
}
// extract the table of unsent enquiries from the controller
var urlJSON = "/PerformanceOverview/RetrieveUnsentEnquiries";
$.getJSON(urlJSON, function (data) {
    GenerateGoogleTable(data);
});
function GenerateGoogleTable(UnsentEnquiriesTable) {
    google.charts.load('current', { 'packages': ['table'] });
    google.charts.setOnLoadCallback(drawTable);
    function drawTable() {
        var data = new google.visualization.DataTable();
        // add the columns
        var RowTest = new PendingEnquiryTableRow();
        for (var prop in RowTest) {
            data.addColumn(typeof RowTest[prop], prop);
        }
        UnsentEnquiriesTable.forEach(function (dataRow) {
            var d = $.map(dataRow, function (value) {
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
}
;
//# sourceMappingURL=PendingEnquiries.js.map