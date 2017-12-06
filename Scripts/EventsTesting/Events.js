/// <reference path="../jquery-3.1.1.min.js" />
/// <reference path="../moment.min.js" />
/// <reference path="../bootstrap.min.js" />
/// <reference path="../bootstrap-datetimepicker.js" />



$(document).ready(function () {
    $(function () {
        $('.dp').datepicker({
                    format: 'dd/mm/yyyy',
                    autoclose: true,
                    todayBtn: 'linked',
                    startDate: '01/02/2016'
        });
    });
        // detect a change event on datepicker
        $('.dp').datepicker().on('changeDate', function (e) {


            // get the first date
            var d1 = $('#beginDate').val();

            // get the second date
            var d2 = $('#endDate').val();

            // fill the 2 labels associated with the previous year
            var parts = d1.split('/');
            var s1 = parts[0] + '/' + parts[1] + '/' + (parts[2] - 1);

            var parts = d2.split('/');
            var s2 = parts[0] + '/' + parts[1] + '/' + (parts[2] - 1);

            $('#beginDatePY').text(s1);
            $('#endDatePY').text(s2);

        });
});



