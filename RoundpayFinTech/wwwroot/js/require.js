(function ($) {
    $.require = function () {
        $.each(arguments, function (i, script) {
            if ($('head script[src="' + script + '"]').length == 0) {
                document.write('<script type="text/javascript" src="' + script + '"></script>');
            }
        });
    };
})(jQuery);
