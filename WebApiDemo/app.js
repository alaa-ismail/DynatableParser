$(function () {

    $('#my-ajax-table').dynatable({
        dataset: {
            ajax: true,
            ajaxUrl: '/api/data',
            ajaxOnLoad: true,
            records: []
        }
    });

});