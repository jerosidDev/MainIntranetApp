$(document).ready(function () {

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




