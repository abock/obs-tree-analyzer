$(document).ready (function () {
    $(".package").click (function () {
        $("#" + $(this).attr ("id") + "-details").toggle ();
    });
});