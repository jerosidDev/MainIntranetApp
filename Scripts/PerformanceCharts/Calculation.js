



function CalculateSuccessRate(viewParam, listSentEnquiries) {

    // calculate the success rate of the consultant/department in meeting the deadline for quoting/contracting

    //      viewParam { Department: "French" or Quoter: "VL" , TargetType: "Quoting" or "Contracting" , FromDate: "2017-05-01" , ToDate: "2017-05-11" } 
    //          --> should give enough information about the monitored item(dpt or csl) and the type of evaluation(quoting or contracting)



    // case viewParam = { Department: "French", TargetType: "Quoting", FromDate: "2017-05-01", ToDate: "2017-05-11" }

    if (viewParam["Department"] != undefined) {

        // filter by the department and the time frame
        var FromDate = new Date(viewParam["FromDate"]);
        var ToDate = new Date(viewParam["ToDate"]);
        var filteredSentEnquiries = jQuery.grep(listSentEnquiries, function (sentEnquiry) {

            var dateSent = new Date(sentEnquiry["JsonDateSent"]);

            if (sentEnquiry["Department"] === viewParam["Department"])
                if (FromDate <= dateSent && dateSent <= ToDate)
                    return true;

            return false;

        });


        // calculate the success rate and the list of references per consultant

        var returnedObject = { success_rate: 0, Consultants: {} } // Consultants: { AJ:  ["ref1","ref2" ] , VL: [ "refi"] }


        var totalSuccess = 0;
        filteredSentEnquiries.forEach(function (sentEnquiry) {

            //  Success evaluation
            //      quoting or contracting?
            if (viewParam["TargetType"] === "Quoting") {

                if (sentEnquiry["nbDaysQuoting"] <= sentEnquiry["maxNbDaysQuoting"])
                    totalSuccess++;


                // add the consultant(s) associated with the sent enquiry and the booking reference
                // todo : need to test with 2 quoting consultants associated with 1 enquiry
                var cslList = sentEnquiry["dictQuoting"];
                for (var cslInitial in cslList) {

                    if (returnedObject.Consultants[cslInitial] == undefined) {
                        returnedObject.Consultants[cslInitial] = [];
                    }

                    returnedObject.Consultants[cslInitial].push(sentEnquiry["FullReference"]);
                }



            }

        });

        // final success rate
        if (filteredSentEnquiries.length != 0)
            returnedObject["success_rate"] = totalSuccess / filteredSentEnquiries.length;


    };

    return returnedObject;

}








