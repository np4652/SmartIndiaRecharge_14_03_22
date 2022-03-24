"use strict";
var page = '/';
var errorMsg = $('#errorMsg');
var LoginStatus = { FAILED: -1, SUCCESS: 1, OTP: 2, GOOGLEAUTHENABLED: 3 };
var AlertStatus = { RED: 0, GREEN: 1, BLUE: 2 };

var alertNormal = {
    title: '',
    content: '',
    color: { green: 'alert-success', red: 'alert-danger', blue: 'alert-info', warning: 'alert-warning' },
    linkClass: 'alert-link',
    iclass: { failed: 'fas fa-times-circle', warning: 'fas fa-exclamation-triangle', success: 'fas fa-check-circle', info: 'fas fa-info-circle' },
    type: { failed: -1, warning: 0, success: 1, info: 2 },
    parent: $('#alertmsg'),
    id: 'alert',
    div: '<div id={id} class="alert {color} alert-dismissible fade position-fixed alert-custom r-t" role="alert">'
        + '<strong > <i class="{iclass}"></i> {title}!</strong> {content}'
        + '<button type="button" class= "close pr-2" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button ></div>',
    alert: function (type) {
        var cls = this.color.blue;
        if (type === this.type.success) cls = this.color.green;
        else if (type === this.type.failed) cls = this.color.red;
        else if (type === this.type.warning) cls = this.color.warning;
        var icls = this.iclass.info;
        if (type === this.type.success) icls = this.iclass.success;
        else if (type === this.type.failed) icls = this.iclass.failed;
        else if (type === this.type.warning) icls = this.iclass.warning;
        this.parent.html(this.div.replace('{id}', this.id).replace('{title}', this.title).replace('{content}', this.content).replace('{color}', cls).replace('{iclass}', icls));
        this.show();
        if (this.autoClose > 0) {
            setTimeout(function () {
                alertNormal.close();
            }, this.autoClose * 1000);
        }
    },
    close: function () {
        $('#' + this.id).removeClass('show');
    },
    show: function () {
        $('#' + this.id).addClass('show');
    },
    autoClose: 0,
    remove: function () {
        $('#' + this.id).remove();
    }
};

var modalAlert = {
    title: '',
    content: '',
    parent: $('body'),
    id: 'mymodal',
    size: { small: 'modal-sm', large: 'modal-lg', xlarge: 'modal-xl', xxlarge: 'modal-xxl', default: '' },
    div: '<div class="modal fade" id={id} tabindex="-1" role="dialog" aria-labelledby="exampleModalCenterTitle" aria-hidden="true">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document"><div class="modal-content"><div class="modal-header">'
        + '<h5 class="modal-title"></h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button></div><div class="modal-body"></div><div class="modal-footer">'
        + '<button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>'
        + '<button type="button" class="btn btn-primary">Save changes</button></div></div></div></div >',
    divAlert: '<div class="modal fade" id={id} tabindex="-1" role="dialog" aria-hidden="true">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document">'
        + '<div class="modal-content"><div class="modal-body"></div></div></div></div >',
    show: function (size, pos) {
        var mdlId = this.id;
        this.parent.append(this.div.replace('{id}', mdlId));
        $('#' + mdlId + ' .modal-title').html(this.title);
        $('#' + mdlId + ' .modal-body').html(this.content);
        $('#' + mdlId + ' .modal-dialog').addClass(size);
        $('#' + mdlId).modal(this.options);
    },
    alert: function (size) {
        var mdlId = this.id;
        this.parent.append(this.divAlert.replace('{id}', mdlId));
        $('#' + mdlId + ' .modal-body').html(this.content);
        $('#' + mdlId + ' .modal-dialog').addClass(size);
        $('#' + mdlId).modal(this.options);
    },
    options: { backdrop: true, keyboard: true, focus: true, show: true },
    dispose: function (f) {
        var mdlId = this.id;
        $('#' + mdlId + ' .modal-content').animate({ opacity: 0 }, 500, function () {
            $('#' + mdlId + ',.modal-backdrop').remove();
            $('body').removeClass('modal-open').removeAttr('style');
            if (f !== undefined)
                f();
        });
    },
    anim: function (ms) {
        $('#' + this.id + ' .modal-content').animate({ opacity: 0 }, ms);
        $('#' + this.id + ' .modal-content').animate({ opacity: 1 }, ms);
    }
};

var an = alertNormal;

var mdlA = modalAlert;

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
    let LoginDetail = {
        LoginMobile: UserID,
        LoginTypeID: LoginTypeID,
        Password: Password,
        OTP: o.val() != undefined ? o.val() : ''
    };
    var URL = LoginDetail.OTP === '' ? 'Login' : 'Login/OTP';
    an.id = "Loginalert";
    an.autoClose = 5;
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
            }
            else if (result.statuscode === LoginStatus.SUCCESS) {
                an.title = "Wow";
                an.content = result.msg;
                an.alert(an.type.success);
                location.href = page + result.path;
            }
            else if (result.statuscode === LoginStatus.OTP) {
                let _html = `<div class="form-group"><input id="txtOTP" class="form-control" placeholder="Please Enter OTP"/></div>`;
                P.parent('.input-group').after(_html);
                $('#btnResendOTP').html('<a href="javascript:void(0)">Resend OTP</a>')
                    .css({ 'position': 'relative', 'top': '-16px', 'right': '-182px' })
                    .click(function () {
                        _ReSendOTP();
                    });
                Alerts('Enter OTP', AlertStatus.BLUE);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
            }
            else if (result.statuscode === LoginStatus.GOOGLEAUTHENABLED) {
                let _html = `<div class="form-group"><input id="txtGooglePin" class="form-control" placeholder="${result.msg}"/></div>`;
                P.parent('.input-group').after(_html);
                //Alerts(result.msg, AlertStatus.BLUE);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
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
    $.post('/forgetPopUp', {}, function (result) {
        $('#' + an.id).remove();
        mdlA.id = 'mdlForgetPass';
        mdlA.content = result;
        mdlA.options.backdrop = 'static';
        mdlA.options.keyboard = false;
        mdlA.alert(mdlA.size.default);
        $('button.close span,#mdlCancel').click(function () {
            mdlA.dispose();
        });
    }).catch(function () {
        console.clear();
    }).fail(function (xhr) {
        $(_this).attr('checked', Is === false);
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
};

var BeforeLoginPopUp = function () {
    preloader.load();
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
            mdlA.dispose();
        });
    }).catch(function () {
        console.clear();
    }).fail(function (xhr) {
        $(_this).attr('checked', Is === false);
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
};

var Forget = function () {

    btnLdr.addClass = 'btn-dark';
    btnLdr.removeClass = 'btn-outline-dark';
    btnLdr.Start($('#btnfoget'), 'Requesting');

    var U = $('#txtFUser'), T = $('#ddlFLType');
    U.removeClass('is-invalid');
    var UserID = U.val(), LoginTypeID = T.val();
    if (UserID === "") {
        U.addClass('is-invalid');
        U.focus();
        Alerts('Enter User ID', AlertStatus.RED);
        btnLdr.Stop($('#btnfoget'));
        return false;
    }
    U.addClass('is-valid');
    var LoginDetail = {};
    LoginDetail.LoginMobile = UserID;
    LoginDetail.LoginTypeID = LoginTypeID;
    an.id = "myalert";
    an.autoClose = 5;
    preloader.load();
    $.ajax({
        type: 'POST',
        url: '/forget',
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {
            console.log(result);
            if (result.statuscode === an.type.failed) {
                an.title = "Oops";
                an.content = result.msg;
                an.alert(result.statuscode);
            }
            else if (result.statuscode === an.type.success) {
                an.title = "Well done";
                an.content = result.msg;
                an.alert(result.statuscode);
                mdlA.dispose();
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

$(document).ready(function () {
    setTimeout(function () { }, 100);
    BeforeLoginPopUp();
    $('#btnLogin').click(function () {
        Login();
    });
    an.id = 'myalert';
    $(document).keypress(function (event) {
        var keycode = event.keyCode ? event.keyCode : event.which;
        if (keycode === 13) {
            $("#btnLogin").click();
        }
    });
});