

google.charts.load('current', { 'packages': ['table'] });
//google.charts.setOnLoadCallback(CallAjax);
google.charts.setOnLoadCallback(CallAjax2);


//function CallAjax() {

//    // ajax calls


//    // PastDeadline
//    $.getJSON("/FollowUpBkgs/FollowUpTableData?FollowUpCategory=" + "PastDeadline",
//            function (DataReceived1) {
//                LoadGoogleTable(DataReceived1, "PastDeadline", 'white', 'red');
//            }
//    );


//    // ToBeDoneToday
//    $.getJSON("/FollowUpBkgs/FollowUpTableData?FollowUpCategory=" + "ToBeDoneToday",
//            function (DataReceived2) {
//                LoadGoogleTable(DataReceived2, "ToBeDoneToday", 'black', 'orange');
//            }
//    );


//    // ToBeDoneWithin7Days
//    $.getJSON("/FollowUpBkgs/FollowUpTableData?FollowUpCategory=" + "ToBeDoneWithin7Days",
//            function (DataReceived3) {
//                LoadGoogleTable(DataReceived3, "ToBeDoneWithin7Days", 'black', 'green');
//            }
//    );


//    // AllFollowUp
//    $.getJSON("/FollowUpBkgs/FollowUpTableData?FollowUpCategory=" + "AllFollowUp",
//            function (DataReceived4) {
//                LoadGoogleTable(DataReceived4, "AllFollowUp", 'white', 'blue');
//            }
//    );



//    function LoadGoogleTable(DataFromAJAX, FollowUpCategory, textColor, cellColor) {



//        var data = new google.visualization.DataTable();




//        // exit if no data : 
//        if (DataFromAJAX.length == 1) return;

//        // extract the first object and its properties to create the columns
//        var firstObj = DataFromAJAX[0];
//        for (var name in firstObj) {
//            // change the Title of the columns to reduce its size
//            var columnTitle = name.replace("Booking", "").replace("Code", "").replace("_", " ");
//            if (firstObj[name].toLowerCase() == 'string') {
//                data.addColumn('string', columnTitle);
//            }
//            else {
//                data.addColumn('date', columnTitle);
//            }
//        }

//        for (var i = 1; i < DataFromAJAX.length; i++) {
//            // creation of the array
//            dataRow = [];
//            objData = DataFromAJAX[i];
//            for (var name in objData) {
//                dataToPush = objData[name];
//                // attention if date : 
//                if (firstObj[name].toLowerCase() != 'string') {
//                    dataToPush = new Date(objData[name]);
//                }
//                dataRow.push(dataToPush);
//            }
//            data.addRow(dataRow);
//        }

//        var table = new google.visualization.Table(document.getElementById(FollowUpCategory));

//        // apply the color to the follow up date column
//        var formatter = new google.visualization.ColorFormat();
//        formatter.addRange(null, null, textColor, cellColor);
//        formatter.format(data, 2);



//        table.draw(data, { allowHtml: true });



//    }

//}

function CallAjax2() {

    // ajax calls
    var catFollowUp = [];
    catFollowUp.push("PastDeadline");
    catFollowUp.push("ToBeDoneToday");
    catFollowUp.push("ToBeDoneWithin7Days");
    catFollowUp.push("AllFollowUp");

    for (var iFollowUp = 0; iFollowUp < catFollowUp.length; iFollowUp++) {
        $.getJSON("/FollowUpBkgs/FollowUpTableData?FollowUpCategory=" + catFollowUp[iFollowUp], jsonCallbackSuccess(catFollowUp[iFollowUp])
        );

    }   

    // closure for typeFollowUp
    function jsonCallbackSuccess(typeFollowUp) {
        return function (jsonData) {
            LoadGoogleTable(jsonData, typeFollowUp);
        }
    }





    function LoadGoogleTable(DataFromAJAX, FollowUpCategory) {



        var data = new google.visualization.DataTable();




        // exit if no data : 
        if (DataFromAJAX.length == 1) return;

        // extract the first object and its properties to create the columns
        var firstObj = DataFromAJAX[0];
        for (var name in firstObj) {
            // change the Title of the columns to reduce its size
            var columnTitle = name.replace("Booking", "").replace("Code", "").replace("_", " ");
            if (firstObj[name].toLowerCase() == 'string') {
                data.addColumn('string', columnTitle);
            }
            else {
                data.addColumn('date', columnTitle);
            }
        }

        for (var i = 1; i < DataFromAJAX.length; i++) {
            // creation of the array
            dataRow = [];
            objData = DataFromAJAX[i];
            for (var name in objData) {
                dataToPush = objData[name];
                // attention if date : 
                if (firstObj[name].toLowerCase() != 'string') {
                    dataToPush = new Date(objData[name]);
                }
                dataRow.push(dataToPush);
            }
            data.addRow(dataRow);
        }

        var table = new google.visualization.Table(document.getElementById(FollowUpCategory));

        // apply the color to the follow up date column
        var textColor, cellColor;
        switch (FollowUpCategory) {
            case "PastDeadline":
                textColor = 'white';
                cellColor = 'red';
                break;
            case "ToBeDoneToday":
                textColor = 'black';
                cellColor = 'orange';
                break;
            case "ToBeDoneWithin7Days":
                textColor = 'black';
                cellColor = 'green';
                break;
            case "PastDeadline":
                textColor = 'white';
                cellColor = 'red';
                break;
            case "AllFollowUp":
                textColor = 'white';
                cellColor = 'blue';
                break;
            default:
                textColor = 'white';
                cellColor = 'black';
                break;
        }
        var formatter = new google.visualization.ColorFormat();
        formatter.addRange(null, null, textColor, cellColor);
        formatter.format(data, 2);



        table.draw(data, { 'allowHtml': true });



    }

}






