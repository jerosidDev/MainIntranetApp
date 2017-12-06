// initially
$('#All').prop('checked', true);

SetPanelTitlesBasedOnSelection();



$(document).ready(function () {


    //  toggle the display of the booking statuses
    $('.All,.Confirmed,.Cancelled,.Unconfirmed').hover(function () {
        var ul = $(this).children('ul');
        ul.toggle();
    });

    // the checkbox "All"  and "Unconfirmed" "Confirmed" "Cancelled" are mutually excluding on the page
    $("[type='checkbox']").change(function () {
        if (this.checked) {
            switch (this.id) {
                case "All":
                    $('#Unconfirmed').prop('checked', false);
                    $('#Confirmed').prop('checked', false);
                    $('#Cancelled').prop('checked', false);
                    break;
                case "Unconfirmed":
                case "Confirmed":
                case "Cancelled":
                    $('#All').prop('checked', false);
                    break;
            }
        }

    });





    $('#narrowBy select').change(function () {

        var objFilter = {};
        objFilter[this.id] = this.value;

        UpDateFilterDropdown(objFilter);

    });




    // click on reset filter
    $('#resetFilter').click(function () {
        $('#narrowBy select').prop('value', 'All');
        UpDateFilterDropdown({});
    });

    $('#EvaluationType').change(function () {
        SetPanelTitlesBasedOnSelection();
    });

    $('#radioWeekly,#radioMonthly').click(function () {
        SetPanelTitlesBasedOnSelection();
    });


});



function UpDateFilterDropdown(objFilter) {



    jQuery.post(
    "/SuppliersAnalysis/GetDropdownsValuesFromFilter",
    objFilter,
    function (dropDownValues) {

        // generate the dropdowns values for each items of narrow by
        for (var filterId in dropDownValues) {


            var filterSelector = '#' + filterId;
            var selectedValue = $(filterSelector)[0].value;

            $(filterSelector).empty();
            $(filterSelector).append($('<option>', { value: "All", text: "All" }));


            //      reselect the selected value
            for (var code in dropDownValues[filterId]) {
                $(filterSelector).append($('<option>', { value: code, text: dropDownValues[filterId][code], selected: selectedValue === code }));
            }

        }

    })
    .fail(function () {
        alert("error loading dropdowns");
    });
}

function SetPanelTitlesBasedOnSelection() {


    let periodView = $('#radioMonthly').prop('checked') ? "Monthly" : "Weekly";
    let evalText = $('#EvaluationType option[value=' + $('#EvaluationType').val() + ']').text();

    //  first panel
    let title = periodView + " total " + evalText;
    $('#title1').text(title);

    // second panel
    title = periodView + " view of the Cumulative " + evalText;
    $('#title2').text(title);

    // third panel
    periodView = periodView.replace("ly", "");
    title = "(Totals are calculated from the beginning of the current financial year until the end of the current " + periodView + ")";
    $('#title3').text(title);


}


