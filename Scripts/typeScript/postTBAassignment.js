/// <reference path="../typings/jquery/jquery.d.ts" />
$(document).ready(function () {
    $('#ShowLocationPanel+.panel .btn.btn-primary').click(function () {
        SendAjaxPost(this);
    });
});
function SendAjaxPost(button) {
    // get the preceding select
    let select = $(button).prev('select');
    let selectOption = select.find(':selected');
    // get the preceding label
    let row = $(button).parent();
    let label = row.find('label');
    let tbaLoc = { CODE: label.attr('value'), NAME: label.text(), Csl_Assigned_INITIAL: selectOption.val() };
    let cc = { INITIALS: selectOption.val(), NAME: selectOption.text(), LocationsAssigned: [tbaLoc] };
    if (cc.INITIALS == undefined)
        return;
    let urlAction = "/PerformanceOverview/PostTBAsInformation";
    let jqxhr = $.post(urlAction, cc);
    jqxhr.done(function () {
        // if change successful, change the color to green and the btn text to "Saved"
        select.css({ "color": "black", "background-color": "white" });
        $(button).text("Saved");
    });
}
//# sourceMappingURL=postTBAassignment.js.map