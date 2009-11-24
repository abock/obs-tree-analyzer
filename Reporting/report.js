$(document).ready (function () {
    $(".package").click (function () {
        var package = this;
        $("#" + $(package).attr ("id") + "-details").toggle (0, function () {
            if ($(this).is (":visible")) {
                $(package).addClass ("selected");
            } else {
                $(package).removeClass ("selected");
            }
        });
    });
});