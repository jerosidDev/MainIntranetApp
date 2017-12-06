/// <reference path="../typings/jquery/jquery.d.ts" />


$(document).ready(function () {

    $('#ShowLocationPanel+.panel .btn.btn-primary').click(function () {
        SendAjaxPost(this);
    });

});

function SendAjaxPost(button: any) {

    // get the preceding select
    let select: any = $(button).prev('select');
    let selectOption: any = select.find(':selected');

    // get the preceding label
    let row = $(button).parent();
    let label: any = row.find('label');

    let tbaLoc: TBAitems.TBALocation = { CODE: label.attr('value'), NAME: label.text(), Csl_Assigned_INITIAL: selectOption.val() };
    let cc: TBAitems.ContractConsultant = { INITIALS: selectOption.val(), NAME: selectOption.text(), LocationsAssigned: [tbaLoc] };

    if (cc.INITIALS == undefined) return;

    let urlAction: string = "/PerformanceOverview/PostTBAsInformation";
    let jqxhr: JQueryXHR = $.post(urlAction, cc);
    jqxhr.done(function () {
        // if change successful, change the color to green and the btn text to "Saved"
        select.css({ "color": "black", "background-color": "white" });
        $(button).text("Saved");
    });


}


declare namespace TBAitems {

class ContractConsultant {
    INITIALS: string;
    NAME: string;
    LocationsAssigned: TBALocation[];
}


class TBALocation {
    CODE: string;
    NAME: string;
    Csl_Assigned_INITIAL: string;
}

}

