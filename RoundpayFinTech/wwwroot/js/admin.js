var viewVendor = () => {
    $.post('/Vendor')
        .done(result => {
            mdlA.id = "VendorView";
            mdlA.content = result;
            mdlA.title = "Vendor";
            mdlA.modal(mdlA.size.default);
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
};
$(function () {
    $('#wallet').click(function () {
        preloader.load();
        $.post('/Add-Wallet', {}, function (result) {
            resultReload(result);
            $('#' + an.id).remove();
            mdlA.id = 'myalert';
            mdlA.content = result;
            mdlA.options.backdrop = 'static';
            mdlA.alert(mdlA.size.large);
            $('button.close span,#mdlCancel').click(function () {
                mdlA.dispose();
            });
            $('[name="options"]').on('change', function () {
                var bType = $(this).data().btype;
                var lbltxt = $(this).data().text;
                $($('#txtcbal').siblings()[0]).text(lbltxt + ' Balance');
                $($('#txtAmount').siblings()[0]).text('Enter '+lbltxt + ' Balance');
                $('#txtcbal').val($(this).data().balance);
                $('#btnBType').val(bType);
            });
            var amt = $('#txtAmount');
            var remk = $('#txtRemark');
            var am = 0;
            amt.on('keyup change', function () {
                if (!$v.$IsNum(amt.val())) {
                    amt.val(0);
                }
                am = parseInt(amt.val());
                amt.val(am);
            });
            $('#bAddW').click(function () {
                var WalletType = 1;
                WalletType = $('#btnBType').val();
                am = parseInt(amt.val());
                if (am < 1) {
                    an.title = 'Info';
                    an.content = 'Enter amount!';
                    an.alert(an.type.info);
                    return false;
                }
                var fp = { Amount: am, Remark: remk.val(), WalletType };
                preloader.load();
                $.ajax({
                    type: 'POST',
                    url: '/AW',
                    dataType: 'json',
                    contentType: 'application/json',
                    data: JSON.stringify(fp),
                    success: function (result) {
                        resultReload(result);
                        if (result.statuscode === -1) {
                            an.title = 'Oops';
                            an.content = result.msg;
                            an.alert(an.type.failed);
                        } else {
                            an.title = 'Success';
                            an.content = result.msg;
                            an.alert(an.type.success);                            
                            mdlA.dispose(function () {
                                checkBal();
                            });
                        }
                    },
                    statusCode: {
                        500: function () {
                            an.title = 'Oops';
                            an.content = 'Server error';
                            an.alert(an.type.failed);
                        },
                        0: function () {
                            an.title = 'Oops';
                            an.content = 'Internet Connection was broken';
                            an.alert(an.type.failed);
                        }
                    },
                    error: function (result) {
                        an.title = 'Oops';
                        an.content = 'An error occured!';
                        an.alert(an.type.failed);
                    },
                    complete: function () {
                        preloader.remove();
                    }
                });
            });
        }).fail(function (xhr) {
            if (xhr.status === 500) {
                an.title = 'Oops';
                an.content = 'Server error';
                an.alert(an.type.failed);
            }
            if (xhr.status === 0) {
                an.title = 'Oops';
                an.content = 'Internet Connection was broken';
                an.alert(an.type.failed);
            }
        }).always(function () {
            preloader.remove();
        });
    });

    $.post('/FundCount')
        .done(result => $('#a_Fund').html('(' + result + ')'))
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
    $.post('/PendingViews')
        .done(result => $('#txtPendingViews').text("(" + result.msg + ")"))
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
    $.post('/callMeCount')
        .done(result => $('#callMeCount').html('(' + result + ')'))
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });

    $.post('/rpadcount')
        .done(result => {
            $('#spTotalDisputCount').html('(' + result.disputeCount + ')')
            $('#spTotalPendingCount').html('(' + result.pCount + ')')
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
    $.post('/BirthdayWishAlert')
        .done()
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
    $('[data-toggle="tooltip"]').tooltip();
});