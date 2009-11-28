$(document).ready (function () {
    $(".tablesorter").collapsible ("td.collapsible", {
        collapse: true
    }).tablesorter ({
        // sortList: [[2, 0]],
        headers: { 0: { sorter: false } },
        widgets: ['zebra'],
        debug: false
    });

    $(".package").click (function () {
        $(this).children ("td.collapsible").children ("a").click ();
    });

    $("#toggle-all-links").click (function () {
        $("#linked-packages > tbody > tr > td.collapsible > a").click ();
    });

    $("#toggle-all-owned").click (function () {
        $("#owned-packages > tbody > tr > td.collapsible > a").click ();
    });

    $("#toggle-all-requests").click (function () {
        $("#requests > tbody > tr > td.collapsible > a").click ();
    });
});
