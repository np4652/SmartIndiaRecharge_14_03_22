$.require('/js/component.alert.js');
$.require('/js/component.modal.js');
$.require('/js/cookie.js');
"use strict";
var page = '/';
var errorMsg = $('#errorMsg');
var LoginStatus = { FAILED: -1, SUCCESS: 1, OTP: 2, GOOGLEAUTHENABLED: 3 };
var AlertStatus = { RED: 0, GREEN: 1, BLUE: 2 };
var geoLocationDetail = {
    Latitude: '',
    Longitude: ''
};

var preloader = {
    load: function () {
        $('body').append('<div class="loading">Loading&#8230;</div>');
    },
    remove: function () {
        $('.loading').remove();
    }
};

var Alerts = function (m, t) {
    errorMsg.removeClass('text-danger text-success text-info');
    errorMsg.removeClass('hide');
    errorMsg.addClass(t === 0 ? 'text-danger' : t === 1 ? 'text-success' : 'text-info');
    errorMsg.text(m);

};

var geoLocation = function () {
    navigator.permissions.query({ name: 'geolocation' }).then(function (result) {
        if (result.state === 'prompt' || result.state === 'granted') {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(geoSuccess);
            }
            function geoSuccess(position) {
                geoLocationDetail.Latitude = position.coords.latitude;
                geoLocationDetail.Longitude = position.coords.longitude;
            }
        }
        else if (result.state === 'denied') {
            an.title = 'Alert';
            an.content = 'Your location permission is denied.Please allow location first.';
            an.alert(an.type.warning);
        }
    });
};

var Login = function () {
    errorMsg.removeClass('hide');
    var U = $('#txtUser'), P = $('#txtPassword'), T = $('#ddlLType'), o = $('#txtOTP');
    U.removeClass('is-invalid');
    P.removeClass('is-invalid');
    var UserID = U.val(), Password = P.val(), LoginTypeID = T.val();
    if (UserID === "") {
        U.addClass('is-invalid').focus();
        Alerts('Enter User ID', AlertStatus.RED);
        return false;
    }
    U.addClass('is-valid');
    if (Password === "") {
        P.addClass('is-invalid').focus();
        Alerts('Enter Password', AlertStatus.RED);
        return false;
    }
    P.addClass('is-valid');
    if ($('body').html().indexOf('txtOTP') > -1 && $('#txtOTP').val() == '') {
        o.addClass('is-invalid').focus();
        Alerts('Enter OTP', AlertStatus.RED);
        return false;
    }
    if ($('body').html().indexOf('txtGooglePin') > -1 && $('#txtGooglePin').val() == '') {
        o.addClass('is-invalid').focus();
        Alerts('Please Enter Google PIN', AlertStatus.RED);
        return false;
    }

    if (window.location.protocol === 'https:') {
        if (geoLocationDetail.Latitude === '' || geoLocationDetail.Longitude === '') {
            geoLocation();
            return false;
        }
    }

    var LoginDetail = {
        LoginMobile: UserID,
        LoginTypeID: LoginTypeID,
        Password: Password,
        OTP: o.val() != undefined ? o.val() : '',
        GooglePin: $('#txtGooglePin').val(),
        Latitude: geoLocationDetail.Latitude ? geoLocationDetail.Latitude : 0,
        Longitude: geoLocationDetail.Longitude ? geoLocationDetail.Longitude : 0,
    };
    var URL = LoginDetail.OTP === '' ? 'Login' : 'Login/OTP';
    if (LoginDetail.GooglePin !== undefined && LoginDetail.GooglePin !== '')
        URL = '/login/VerifyGoogleAuthenticatorSetup';
    an.id = "Loginalert";
    an.autoClose = 5;
    var isBocked = Q.cookie.get('isBrowserBlock');
    if (isBocked != null && isBocked === 'true') {
        Alerts('Account is blocked.Please contact to support team.', AlertStatus.RED);
        return;
    }
    preloader.load();
    $.ajax({
        type: 'POST',
        url: URL,
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {
            console.log(result);
            if (result.statuscode === undefined || result.statuscode === LoginStatus.FAILED) {
                Alerts(result.msg, AlertStatus.RED);
                an.title = "Oops";
                an.content = result.msg;
                an.alert(an.type.failed);
                if (result.isBrowserBlock) {
                    Q.cookie.set('isBrowserBlock', result.isBrowserBlock, 0.08);
                }
            }
            else if (result.statuscode === LoginStatus.SUCCESS) {
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.success);
                location.href = page + result.path;
            }
            else if (result.statuscode === LoginStatus.OTP) {
                var _html = '<div class="form-group"><input id="txtOTP" class="form-control" placeholder="Please Enter OTP"/></div>';
                if ($('body').html().indexOf('txtOTP') == -1) {
                    P.parent('.input-group').after(_html);
                }
                $('#btnResendOTP').html('<a href="javascript:void(0)">Resend OTP</a>')
                    .css({ 'position': 'relative', 'top': '-16px', 'right': '-182px' })
                    .click(function () {
                        _ReSendOTP();
                    });
                //Alerts('Enter OTP', AlertStatus.BLUE);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
            }
            else if (result.statuscode === LoginStatus.GOOGLEAUTHENABLED) {
                var _html = '<div class="form-group"><input id="txtGooglePin" autocomplete="off" class="form-control" placeholder="' + result.msg + '"/></div>';
                if ($('body').html().indexOf('txtGooglePin') === -1)
                    P.parent('.input-group').after(_html);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
                $('#txtGooglePin').focus();
            }
            else if (result.statuscode === -2) {
                geoLocation();
            }
        }, statusCode: {
            500: function () {
                Alerts('Oops! Server error', AlertStatus.RED);
            },
            0: function () {
                Alerts('Oops! Internet Connection was broken', AlertStatus.RED);
            }
        },
        error: function (xhr, result) {
            Alerts(result, AlertStatus.RED);
        },
        complete: function () {
            preloader.remove();
        }
    });
};

var _ReSendOTP = function () {
    $.post('/ResendOTP')
        .done(function (result) {
            an.title = "Info";
            an.content = result.msg;
            an.alert(an.type.info);
        });
};

var forgetPopUp = function () {
    preloader.load();
    $.post('/forgetPopUp').done(function (result) {
        $('#' + an.id).remove();
        mdlA.id = 'mdlForgetPass';
        mdlA.content = result;
        mdlA.options.backdrop = 'static';
        mdlA.options.keyboard = false;
        mdlA.title = 'Forget Password';
        mdlA.modal(mdlA.size.default);
    }).fail(function (xhr) {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(function () { preloader.remove() });
};

var BeforeLoginPopUp = function () {
    preloader.load();
    var IsBLPShowed = JSON.parse(localStorage.getItem("IsBLPShowed"));
    var todayDate = new Date().getDate();
    if (IsBLPShowed === null || !IsBLPShowed._Value || IsBLPShowed._date !== todayDate) {
        $.post('/BeforeLoginPopup', {}, function (result) {
            if (result !== "") {
                $('#' + an.id).remove();
                mdlA.id = 'mdlForgetPass';
                mdlA.content = result;
                mdlA.options.backdrop = 'static';
                mdlA.options.keyboard = false;
                mdlA.alert(mdlA.size.large);
            }
            $('button.close span,#mdlCancel').click(function () {
                mdlA.dispose(function () {
                    var obj = {
                        _date: new Date().getDate(),
                        _Value: true
                    };
                    localStorage.setItem("IsBLPShowed", JSON.stringify(obj))
                })
            });
        }).fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(function () { preloader.remove() });
    }
};

var Forget = function () {
    btnLdr.addClass = 'btn-dark';
    btnLdr.removeClass = 'btn-outline-dark';
    btnLdr.Start($('#btnfoget'), 'Requesting');
    preloader.load();
    var User = $('#txtFUser'), Type = $('#ddlFLType'), mobileOTP = $('#txtMobileOTP'), emailOTP = $('#txtEmailOTP');
    var LoginDetail = {
        LoginMobile: User.val(),
        LoginTypeID: Type.val(),
        MobileOTP: mobileOTP.val() != undefined ? mobileOTP.val() : '',
        EmailOTP: emailOTP.val() != undefined ? emailOTP.val() : ''
    };
    if (LoginDetail.LoginMobile == undefined || LoginDetail.LoginMobile == '') {
        User.addClass('is-invalid').focus();
        $('#alert').text('Enter User ID').addClass("text-danger");
        btnLdr.Stop($('#btnfoget'));
        preloader.remove();
        return false;
    }
    $.ajax({
        type: 'POST',
        url: '/forget',
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {
            console.log(result);
            if (result.statuscode == 2 || result.msg == "Invalid OTP") {
                an.title = "OTP";
                if (!$('#txtMobileOTP').length && !$('#txtEmailOTP').length) {
                    var _html = '<div class="form-group"><input id="txtMobileOTP" class="form-control" placeholder="Please enter mobile OTP"/></div><div class="form-group"><input id="txtEmailOTP" class="form-control" placeholder="Please enter email OTP"/></div>'
                    $('#txtFUser').parent('.form-group').after(_html);
                }
                $('#btnfoget').text('Submit OTP');
            }
            $('#alert').text(result.msg).addClass(result.statuscode === an.type.success ? "text-success" : "text-danger");
            if (result.statuscode === 1) {
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.success);
                mdlA.dispose();
            }
        }, statusCode: {
            500: function () {
                $('#alert').text('Oops! Server error').addClass("text-danger");
                Alerts('Oops! Server error', AlertStatus.RED);
            },
            0: function () {
                $('#alert').text('Oops! Internet Connection was broken').addClass("text-danger");
                Alerts('Oops! Internet Connection was broken', AlertStatus.RED);
            }
        },
        error: function (xhr, result) {
            Alerts(result, AlertStatus.RED);
        },
        complete: function () {
            btnLdr.Stop($('#btnfoget'));
            preloader.remove();
        }
    });
};

var btnLdr = {
    removeClass: '',
    addClass: '',
    Start: function (btn, btnText) {
        var dataLoadingClass = "<i class='fas fa-circle-notch fa-spin'></i> " + btnText;
        btn.attr('original-text', btn.html());
        btn.html(dataLoadingClass);
        btn.removeClass(this.removeClass).addClass(this.addClass);
    },
    Stop: function (btn) {
        btn.html(btn.attr('original-text'));
        btn.removeClass(this.addClass).addClass(this.removeClass);
    }
};
var Getnews = function () {
    preloader.load();
    $.post('/l-news')
        .done(function (result) { $('#divNews').append(result.newsDetail) })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
        .always(function () { preloader.remove() });
};


$(document).ready(function () {
    BeforeLoginPopUp();
    Getnews();
    $('#btnLogin').click(function () {
        Login()
    });
    an.id = 'myalert';
    if (window.location.protocol === 'https:') {
        geoLocation();
    }
});

$(document).on('keypress', $('button:last'), function (event) {
    var keycode = event.keyCode ? event.keyCode : event.which;
    if (keycode === 13) {
        $("button:last").click();
    }
});
var Unlockme = function () {
    preloader.load();
    $.post('/Unlockme').done(function (result) {
        $('#' + an.id).remove();
        mdlA.id = 'mdlUnlockMe';
        mdlA.content = result;
        mdlA.options.backdrop = 'static';
        mdlA.options.keyboard = false;
        mdlA.title = 'Unlock Me';
        mdlA.modal(mdlA.size.default);
    }).fail(function (xhr) {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(function () {
        preloader.remove()
    });
};

var Unlock = function () {
    btnLdr.addClass = 'btn-dark';
    btnLdr.removeClass = 'btn-outline-dark';
    btnLdr.Start($('#btnUnlock'), 'Requesting');
    preloader.load();
    var User = $('#txtFFUser'), Type = $('#ddlFLType'), googlePin = $('#txtGooglePin'), emailOTP = $('#txtEmailOTP');
    var LoginDetail = {
        LoginMobile: User.val(),
        LoginTypeID: Type.val(),
        GooglePin: googlePin.val() != undefined ? googlePin.val() : ''
    };
    if (LoginDetail.LoginMobile == undefined || LoginDetail.LoginMobile == '') {
        User.addClass('is-invalid').focus();
        $('#alert').text('Enter User ID').addClass("text-danger");
        btnLdr.Stop($('#btnfoget'));
        preloader.remove();
        return false;
    }
    $.ajax({
        type: 'POST',
        url: '/Unlock',
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {

            if (result.statuscode === 1 && !result.commonBool) {
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.success);
                Q.cookie.delete('isBrowserBlock');
                mdlA.dispose();
            }
            if (result.statuscode === 1 && result.commonBool) {
                an.title = "OTP";
                var _html = '<div class="form-group"><input id="txtGooglePin" class="form-control" placeholder="Please Enter Google Pin"/></div>'
                $('#txtFFUser').parent('.input-group').after(_html);

                $('#btnUnlock').text('Submit Pin');
            }
            if (result.statuscode === 2) {
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.success);
                Q.cookie.delete('isBrowserBlock');
                mdlA.dispose();
            }
            if (result.statuscode === -1) {
                $('#alert').text(result.msg).addClass(result.statuscode === an.type.success ? "text-success" : "text-danger");
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.failed);
            }


        }, statusCode: {
            500: function () {
                $('#alert').text('Oops! Server error').addClass("text-danger");
                Alerts('Oops! Server error', AlertStatus.RED);
            },
            0: function () {
                $('#alert').text('Oops! Internet Connection was broken').addClass("text-danger");
                Alerts('Oops! Internet Connection was broken', AlertStatus.RED);
            }
        },
        error: function (xhr, result) {
            Alerts(result, AlertStatus.RED);
        },
        complete: function () {
            btnLdr.Stop($('#btnUnlock'));
            preloader.remove();
        }
    });
};