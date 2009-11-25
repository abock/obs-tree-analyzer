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
});
