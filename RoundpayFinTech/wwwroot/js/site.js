///<reference path="../lib/tinymce/tinymce.min.js" />
"use strict";
var page = '/';
var MessageBoxType = { SUCCESS: 1, FAILED: 2, ACCEPT: 3, REJECT: 4, INCOMPLETE: 5, UNAUTHORISED: 6, UNDERSCREENING: 7, SERVICEDOWN: 8 };
var UPIName = { apl: "Amazon Pay App", allbank: "BHIM ALLBANK UPI", axisbank: "Axis Pay", axl: "Phone-Pe", BARODAMPAY: "Baroda Pay", citi: "Citi Mobile APP", citigold: "Citi Mobile APP", dbs: "DigiBank - DBS APP", federal: "BHIM Lotza UPI", freecharge: "Freecharge", hsbc: "HSBC Simply Pay", ibl: "Phone-Pe", icici: "iMobile - ICICI Bank", idfcfirst: "IDFC First", indus: "IndusPay", kotak: "Kotak Mobile Banking App", okaxis: "Google Pay", okhdfcbank: "Google Pay", okicici: "Google Pay", oksbi: "Google Pay", paytm: "Paytm App", rbl: "RBL Pay", sbi: "SBIPay", sib: "SIB Mirror", upi: "BHIM APP", ybl: "Phone-Pe", yesbank: "YES PAY" };
var LogoutKey = 'redirectToLogin';
var btnLoadingClass = '<i class="fas fa-circle-notch fa-spin"></i> ';
var Logout = function (u, uid, st) {
    $.post('/Logout', { ULT: u, UserID: uid, SType: st }, function (result) {
        resultReload(result);
        if (result.statuscode === an.type.success) {
            sessionStorage.clear();
            reload();
        }
        else {
            an.title = 'Oops';
            an.content = result.msg;
            an.alert(an.type.failed);
        }
    }).fail(xhr => {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(() => preloader.remove());
};

var larr = location.href.split('/');

var loc = location.href.split('#')[0];

var reload = () => location.href = loc;

var resultReload = function (result) {
    try {
        if (result !== undefined) {
            if (result.indexOf('login.js') > -1 || result.indexOf(LogoutKey) > -1) {
                reload();
                return true;
            }
        }
    } catch (e) {
        //console.log(e);
    }
    return false;
};

var setInfo = function (res) {
    let profileImg = `/Image/Profile/${res.userID}.png`;
    $('#profileImg,.btnProfileSetting').load(profileImg, function (response, status, xhr) {
        if (status == "error") {
            $('#btnProfileSetting,.btnProfileSetting').html(`<span>${res.name.match(/\b([A-Z])/g)?.join('')}</span>`);
        }
        else {
            $(this).attr('src', profileImg);
            $('.btnProfileSetting').html(`<img class="profileImg" src="${profileImg}"/>`);
        }
    });
    $('#fullName').html(res.name);
    $('#UINFO span.dropdown-item-text').html(res.outletName);
    $('#UINFO span.dropdown-item-text').attr("data-item-mobile", res.mobileNo);
}

function isUrlExists(url, cb) {
    $.ajax({
        url: url,
        dataType: 'text',
        type: 'GET',
        complete: function (xhr) {
            if (typeof cb === 'function')
                cb.apply(this, [xhr.status]);
        }
    });
}

var btnLdr = {
    removeClass: '',
    addClass: '',
    StartWithAnyText: function (btn, btnText, isOriginal) {
        if (isOriginal === true) {
            btn.attr('original-text', btn.html());
        }
        btn.html(btnLoadingClass + btnText);
        btn.removeClass(this.removeClass).addClass(this.addClass);
    },
    StopWithText: function (btn, btnText) {
        btn.html(btnText);
        btn.removeClass(this.addClass).addClass(this.removeClass);
    },
    Start: function (btn, btnText) {
        btn.attr('original-text', btn.html());
        btn.html(btnLoadingClass + btnText);
        btn.removeClass(this.removeClass).addClass(this.addClass);
    },
    Stop: function (btn) {
        btn.html(btn.attr('original-text'));
        btn.removeClass(this.addClass).addClass(this.removeClass);
    }
};

var alertContent = {
    title: '',
    content: '',
    color: { green: 'alert-success', red: 'alert-danger', blue: 'alert-info', warning: 'alert-warning' },
    linkClass: 'alert-link',
    type: { failed: -1, warning: 0, success: 1, info: 2 },
    parent: $('#alertmsg'),
    id: 'alert',
    div: `<div id={id} class="alert {color} alert-dismissible fade" role="alert">
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">&times;</span></button>
            <h4 class="alert-heading">{title}</h4 >
            <p>{content}</p>
          </div>`,
    alert: function (type) {
        var cls = this.color.blue;
        if (type === this.type.success)
            cls = this.color.green;
        else if (type === this.type.failed)
            cls = this.color.red;
        else if (type === this.type.warning)
            cls = this.color.warning;
        this.parent.html(this.div.replace('{id}', this.id).replace('{title}', this.title).replace('{content}', this.content).replace('{color}', cls));
        this.show();
    },
    close: function () {
        $('#' + this.id).removeClass('show');
    },
    show: function () {
        $('#' + this.id).addClass('show');
    }
};

var alertNormal = {
    title: '',
    content: '',
    color: { green: 'alert-success', red: 'alert-danger', blue: 'alert-info', warning: 'alert-warning' },
    tcolor: { green: 'text-success', red: 'text-danger', blue: 'text-info', warning: 'text-warning' },
    linkClass: 'alert-link',
    iclass: { failed: 'fas fa-times-circle', warning: 'fas fa-exclamation-triangle', success: 'fas fa-check-circle', info: 'fas fa-info-circle' },
    type: { failed: -1, warning: 0, success: 1, info: 2 },
    rtype: { rechPend: 1, rechSucc: 2, rechFail: 3, rechRef: 4 },
    parent: $('#alertmsg'),
    id: 'alert',
    div: `<div id={id} class="alert {color} alert-dismissible fade position-fixed alert-custom r-t" role="alert">
            <strong><i class="{iclass}"></i> {title}!</strong> {content}
            <button type="button" class= "close pr-2" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button >
          </div>`,
    alert: function (type) {
        var cls = this.color.blue;
        if (type === this.type.success || type === this.type.rechSucc) cls = this.color.green;
        else if (type === this.type.failed || type === this.type.rechFail) cls = this.color.red;
        else if (type === this.type.warning || type === this.type.rechPend) cls = this.color.warning;
        var icls = this.iclass.info;
        if (type === this.type.success || type === this.type.rechSucc) icls = this.iclass.success;
        else if (type === this.type.failed || type === this.type.rechFail) icls = this.iclass.failed;
        else if (type === this.type.warning || type === this.type.rechPend) icls = this.iclass.warning;
        this.parent.html(this.div.replace('{id}', this.id).replace('{title}', this.title).replace('{content}', this.content).replace('{color}', cls).replace('{iclass}', icls));
        this.show();
        let _this = this;
        if (_this.autoClose > 0) {
            setTimeout(function () {
                _this.remove();
            }, _this.autoClose * 1000);
        }
    },
    ralert: function (type) {
        var cls = this.color.blue;
        if (type === this.type.rechSucc) cls = this.color.green;
        else if (type === this.type.rechFail) cls = this.color.red;
        else if (type === this.type.rechPend) cls = this.color.warning;
        var icls = this.iclass.info;
        if (type === this.type.rechSucc) icls = this.iclass.success;
        else if (type === this.type.rechFail) icls = this.iclass.failed;
        else if (type === this.type.rechPend) icls = this.iclass.warning;
        this.parent.html(this.div.replace('{id}', this.id).replace('{title}', this.title).replace('{content}', this.content).replace('{color}', cls).replace('{iclass}', icls));
        this.show();
        if (this.autoClose > 0) {
            setTimeout(function () {
                alertNormal.close();
            }, this.autoClose * 1000);
        }
    },
    getColor: function (type) {
        var cls = this.color.blue;
        if (type === this.rtype.rechSucc) cls = this.color.green;
        else if (type === this.rtype.rechFail) cls = this.color.red;
        else if (type === this.rtype.rechPend) cls = this.color.warning;
        return cls;
    },
    getTColor: function (type) {
        var cls = this.color.blue;
        if (type === this.rtype.rechSucc) cls = this.tcolor.green;
        else if (type === this.rtype.rechFail) cls = this.tcolor.red;
        else if (type === this.rtype.rechPend) cls = this.tcolor.warning;
        return cls;
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
    confirmContent: '<h5>Are you sure?</h5>',
    parent: $('body'),
    id: 'mymodal',
    size: { small: 'modal-sm', large: 'modal-lg', xlarge: 'modal-xl', xxlarge: 'modal-xxl', xxlargeM: 'modal-xxl-m', auto: 'modal-auto', default: '' },
    bodyCls: '',
    div: `<div class="modal fade" id={id} tabindex="-1" role="dialog" aria-labelledby="exampleModalCenterTitle" aria-hidden="true">
            <div class= "modal-dialog modal-dialog-centered" role="document"><div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"></h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                </div>
                <div class="modal-body"></div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                    <button type="button" class="btn btn-primary">Save changes</button>
                </div>
                </div>
            </div>
          </div>`,
    divAlert: '<div class="modal fade" id={id} tabindex="-1" role="dialog" aria-hidden="true">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document">'
        + '<div class="modal-content"><div class="modal-body {bodyCls}"></div></div></div></div >',
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
        this.parent.append(this.divAlert.replace('{id}', mdlId).replace('{bodyCls}', this.bodyCls));
        $('#' + mdlId + ' .modal-body').html(this.content);
        $('#' + mdlId + ' .modal-dialog').addClass(size);
        $('#' + mdlId).modal(this.options);
    },
    isHeaderBorder: true,
    headerClass: 'h5',
    callBack: '',
    modal: function (size, callBack) {
        var _html = `<div class="modal fade" id="${this.id}" tabindex="-1" role="dialog" aria-labelledby="exampleModalCenterTitle" aria-hidden="true">
            <div class= "modal-dialog modal-dialog-centered ${size}" role="document">
                <div class="modal-content">
                    <div class="${this.isHeaderBorder ? 'modal-header' : 'pl-3 pr-3 mt-2'} custome">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h3 class="${this.headerClass} modal-title">${this.title === "" ? 'Alert' : this.title}</h3> 
                    </div>
                    <div class="modal-body">${this.content}</div>
                </div>
            </div>
          </div>`
        this.parent.append(_html);
        $(`#${this.id}`).modal(this.options);
        $(`#${this.id}`).find('button.close').unbind().click(() => {
            mdlA.dispose();
            if (callBack !== undefined) {
                callBack();
            }
        });
    },
    reset: function () {
        this.isHeaderBorder = true;
        this.title = "";
        this.headerClass = "h5";
        this.bodyCls = '';
    },
    options: { backdrop: 'static', keyboard: true, focus: true, show: true },
    dispose: function (f) {
        this.reset();
        //this.bodyCls = '';
        var mdlId = this.id;
        $('#' + mdlId + ',.modal-backdrop:last').fadeOut('slow', function () {
            $(this).remove();
        });
        $('#' + mdlId + ' .modal-content:last').animate({ opacity: 0 }, 500, function (i) {
            $('body').removeClass('modal-open').removeAttr('style');
            if (f !== undefined)
                f();
        });
    },
    anim: function (ms) {
        $('#' + this.id + ' .modal-content').animate({ opacity: 0 }, ms);
        $('#' + this.id + ' .modal-content').animate({ opacity: 1 }, ms);
    },
    confirm: function () {
        return `<div class="col-md-12" id="dvpopup">
                    <button type = "button" class="close" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                    ${this.confirmContent}
                    <div class="form-group">
                        <button class="btn btn-outline-success mr-2" id="btnOK">Yes</button>
                        <button class="btn btn-outline-danger" id="mdlCancel">No</button>
                    </div>
                </div>`;
    }
};

var modalStack = {
    title: '',
    content: '',
    confirmContent: '',
    parent: $('body'),
    id: 'mystack',
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
    disposeConfirm: function (f) {
        var mdlId = this.id;
        $('#' + mdlId + ' .modal-content').animate({ opacity: 0 }, 500, function () {
            $('#' + mdlId).remove();
        });
    },
    anim: function (ms) {
        $('#' + this.id + ' .modal-content').animate({ opacity: 0 }, ms);
        $('#' + this.id + ' .modal-content').animate({ opacity: 1 }, ms);
    },
    confirm: function (confirmContent) {
        return `<div class="col-md-12" id="dvpopup">
            <button type = "button" class="close" aria-label="Close">
            <span aria-hidden="true">&times;</span></button>
                ${confirmContent}
            <div class="form-group"></div> <button class="btn btn-outline-success mr-2" id="btnOK">Yes</button>
            <button class="btn btn-outline-danger" id="mdlCancel">No</button></div>`
    },
    confirmStack: function (id, confirmCont) {
        return `<div class="col-md-12" id="${id}">
                     ${confirmCont}
                <div class="form-group"></div>
                <button class="btn btn-outline-success mr-2" id="btnOK">Yes</button>
                <button class="btn btn-outline-danger" id="mdlCancel${id}">No</button></div>`
    }
}

function copyToClipboard(str) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(str).select();
    document.execCommand("copy");
    $temp.remove();
}

function pasteAtControl(ctrl, txtToAdd) {
    var caretPos = ctrl[0].selectionStart;
    var textAreaTxt = ctrl.val();
    ctrl.val(textAreaTxt.substring(0, caretPos) + txtToAdd + textAreaTxt.substring(caretPos));
    ctrl.focus();
}

var rel = function (seconds) {
    setTimeout(function () {
        reload();
    }, seconds * 1000);
};

var preloader = {
    load: () => $('body').append('<div class="loading">Loading&#8230;</div>'),
    remove: () => $('.loading').remove()
};

var $v = $validator;
var ac = alertContent;
var an = alertNormal;
var mdlA = modalAlert;
var mdlStk = modalStack;



$(document).ready(function () {
    an.autoClose = 5;
    $('.dropdown-toggle,[data-toggle="dropdown"]').dropdown();
    an.id = 'myalert';
    //UserDetail();
    $('#logout,#logoutAll').click(function () {
        var s = $(this).attr("id") === 'logoutAll' ? 3 : 1;
        Logout(0, 0, s);
    });

    $('#doctypemaster_minus1,#doctypemaster_plus1').click(function () {
        let isotlt = 0;
        isotlt = this.id.indexOf('_minus1') > -1 ? false : true;
        preloader.load();
        $.post('/DocTypeMaster', { f: isotlt })
            .done(result => {
                resultReload(result);
                $('#' + an.id).remove();
                mdlA.id = 'docalert';
                mdlA.content = result;
                mdlA.options.backdrop = 'static';
                mdlA.alert(mdlA.size.xlarge);
                $('button.close span,#mdlCancel').click(() => mdlA.dispose());
                $('[data-toggle="tooltip"]').tooltip();
                $("[id^=isactservice_],[id^=txtTaxRemark_]").change(function () {
                    an.autoClose = 5;
                    let currid = $(this).closest("tr").data().itemId;
                    let ind = this.id.split('_')[1];

                    let isoptional = $('#isactservice_' + ind).prop("checked");
                    let taxremark = $("#txtTaxRemark_" + ind).val();
                    let IsOutlet = isotlt ? 1 : -1;
                    let cte = { ID: currid, DocName: "", IsOptional: isoptional, Remark: taxremark, ModifyDate: "", UserId: 0, StatusCode: 0, Description: "", IsOutlet: IsOutlet };

                    $.ajax({
                        type: 'POST',
                        url: '/UpdateDocTypeMaster',
                        dataType: 'json',
                        contentType: 'application/json',
                        data: JSON.stringify(cte),
                        success: function (result) {
                            resultReload(result);
                            an.title = result.statuscode === an.type.success ? 'Success' : 'Oops';
                            an.content = result.msg;
                            an.alert(result.statuscode);
                            if (result.statuscode === an.type.success) {
                                $('[data-item-id="' + currid + '"] [data-toggle="tooltip"]').attr('data-original-title', 'Last Modified: Just Now!');
                                mdlA.anim(300);
                            }
                        },
                        error: function (xhr, result) {
                            checkError(result);
                            an.title = "Oops! Error";
                            an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
                            an.alert(an.type.failed);
                        },
                        complete: () => preloader.remove()
                    });
                });
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    });

    $('#invoiceSet').click(function () {
        preloader.load();
        $.post('/InvoiceSetting')
            .done(result => {
                resultReload(result);
                $('#' + an.id).remove();
                mdlA.id = 'invoiceAlert';
                mdlA.content = result;
                mdlA.options.backdrop = 'static';
                mdlA.alert(mdlA.size.default);
                $('button.close span,#mdlCancel').click(() => mdlA.dispose());
                $('[data-toggle="tooltip"]').tooltip();
                $("[id^=isactinvoice_]").change(function () {
                    an.close = 5;
                    var currid = $(this).closest("tr").data().itemId;
                    var ind = this.id.split('_')[1];

                    var isdisable = $('#isactinvoice_' + ind).prop("checked");
                    preloader.load();
                    $.post('/update-invoice-status', { s: isdisable, i: currid })
                        .done(result => {
                            an.title = result.statuscode === an.type.success ? 'Success' : 'Oops';
                            an.content = result.msg;
                            an.alert(result.statuscode);
                            if (result.statuscode === an.type.success) {
                                $('[data-item-id="' + currid + '"] [data-toggle="tooltip"]').attr('data-original-title', 'Last Modified: Just Now!');
                                mdlA.anim(100);
                            }
                        }).fail(xhr => {
                            an.title = 'Oops';
                            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                            an.alert(an.type.failed);

                        }).always(() => preloader.remove());
                });
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    });

    $('#lnkVchEntry').on('click', function () {
        preloader.load();
        $.post('/vch-Entry')
            .done(function (result) {
                resultReload(result);
                mdlA.id = 'mymodel';
                mdlA.content = result;
                mdlA.options.backdrop = 'static';
                mdlA.alert(mdlA.size.default);
                $('button.close span,#mdlCancel').unbind().click(function () {
                    mdlA.dispose();
                });
            }).catch(function (xhr, ex, message) {
                an.title = 'Oops';
                an.content = message;
                an.alert(an.type.failed);
                an.autoClose = 2;
            }).fail(function (xhr) {
                if (xhr.status == 500) {
                    an.title = 'Oops';
                    an.content = 'Server error';
                    an.alert(an.type.failed);
                }
                if (xhr.status == 0) {
                    an.title = 'Oops';
                    an.content = 'Internet Connection was broken';
                    an.alert(an.type.failed);
                }
            }).always(function () {
                preloader.remove();
            });
    })
    bottomIcons();
});

var darkAnimBtn = function (btn) {
    var tglIntvl = setInterval(function () {
        btn.toggleClass('btn-light btn-outline-light');
    }, 2000);
    setTimeout(function () {
        clearInterval(tglIntvl);
    }, 12000);
};

var checkError = function (result) {
    if (result.responseText.indexOf('login.js') > -1) {
        rel(0);
        return false;
    }
};
var UserMyWallet = function () {
    $.post('/my-bal')
        .done(result => {
            resultReload(result);
            mdlA.id = 'mdlMybal';
            mdlA.title = 'My Balance';
            mdlA.content = result;
            mdlA.options.backdrop = 'static';
            mdlA.modal(mdlA.size.large);

        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
};
var PackageLimitBal = function () {
    $.post('PackageLimitbalance')
        .done(result => {
            resultReload(result);
            mdlA.id = 'mymodal';
            mdlA.content = result;
            mdlA.options.backdrop = 'static';
            mdlA.options.keyboard = false;
            mdlA.alert(mdlA.size.default);
            $('button.close span,#mdlCancel').click(() => mdlA.dispose());

        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })
};

$('#changepass').click(() => ChangePass(false));
$('#changepin').click(() => ChangePin(false));
$('#myBalance').click(() => UserMyWallet());
$('#PackageLimitbalance').click(() => PackageLimitBal());
$('#wTowft').click(() => WalletToWalletFT());
var WalletToWalletFT = function () {
    preloader.load();
    $.post('/w-2-w')
        .done(result => {
            resultReload(result);
            mdlA.id = 'mdlMybal';
            mdlA.content = result;
            mdlA.options.backdrop = 'static';
            mdlA.alert(mdlA.size.default);
            $('button.close span,#mdlCancel').click(() => mdlA.dispose());
        }).fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        }).always(() => {
            preloader.remove();
        });
};

var AEPSStatusText = { PENDING: 1, SUCCESS: 2, FAILED: 3, ERROR: 0 };
function getKeyByValue(object, value) {
    return Object.keys(object).find(key => object[key] === value);
}

function getQueryString() {
    let queries = {};
    let url = document.location.search;
    if (url.trim() !== '') {
        $.each(document.location.search.substr(1).split('&'), function (c, q) {
            let i = q.split('=');
            queries[i[0]] = i[1];
        });
    }
    return queries;
}

var loadDownloasWin = function () {
    $.post('/download-pdf').done(result => {
        mdlA.id = "dWin";
        mdlA.content = result;
        mdlA.alert(mdlA.size.large);
        $('button.close').click(() => mdlA.dispose());
    })
};

(function ($) {
    $.FormValidation = $.FormValidation || {};
    $.FormValidation.IsFormValid = function (IsModelPopUp = false) {
        let IsValid = false;
        if (!IsModelPopUp) {
            $('[required="required"]').each(function () {
                let _tag = $(this).prop('tagName'), _ele = $(this);
                if (_ele.parent('div').html().indexOf('text-invalid') > -1) {
                    _ele.removeClass('invalid');
                    _ele.parent('div').find('span.text-invalid').remove();
                }
                if ((_tag === 'SELECT' || _tag === 'INPUT') && (_ele.val() === undefined || _ele.val() === '') || (_tag === 'SELECT' && _ele.val() === "0")) {
                    _ele.addClass('invalid').before(' <span class="text-danger text-monospace text-invalid"><small>(This is mendetory field)</span></small>').focus();
                    IsValid = false;
                    return false;
                }
                else {
                    IsValid = true;
                }
            })
        }
        else {
            $('.modal [required="required"]').each(function () {
                let _tag = $(this).prop('tagName'), _ele = $(this);
                if (_ele.parent('div').html().indexOf('text-invalid') > -1) {
                    _ele.removeClass('invalid');
                    _ele.parent('div').find('span.text-invalid').remove();
                }
                if ((_tag === 'SELECT' || _tag === 'INPUT') && (_ele.val() === undefined || _ele.val() === '') || (_tag === 'SELECT' && _ele.val() === "0")) {
                    _ele.addClass('invalid').before(' <span class="text-danger text-monospace text-invalid"><small>(This is mendetory field)</span></small>').focus();
                    IsValid = false;
                    return false;
                }
                else {
                    IsValid = true;
                }
            })
        }
        return IsValid;
    };
})($);

(function ($) {
    $.fn.numeric = function (options) {
        let settings = $.extend({
            numericType: "number",
            maxLength: 0
        }, options);
        $(this).keypress((e) => {
            if (settings.maxLength !== 0 && $(e.currentTarget).val().length > settings.maxLength)
                return false
            let keycode = (e.keyCode ? e.keyCode : e.which);
            if (settings.numericType === "number") {
                if ((keycode >= 48 && keycode <= 57))
                    return true;
                return false;
            }
            else if (settings.numericType === "decimal") {
                if ((keycode >= 48 && keycode <= 57) || keycode === 46 && $(e.currentTarget).val().indexOf('.') === -1)
                    return true;
                return false;
            }
        });
    };
}($));
//Signed By NT@Roundpay
(function ($) {
    $.fn.fixTableHeader = function () {
        let scrollTop = $(window).scrollTop(),
            elementOffset = $('table:last').offset()?.top,
            filtersHeight = $('.filters')?.height(),
            distance = (elementOffset - scrollTop - isNaN(filtersHeight) ? 0 : filtersHeight),
            other = isNaN(filtersHeight) ? 231 : 93,
            footer = $('footer').height();
        let _style = `<style>.calcHeight{height: calc(100vh - ${distance}px - ${other}px); }.fixedHeader th {background: #dcdbc1!important; position: sticky;top: -1px; z-index:9;padding:10px;}</style>`
        $('head').append(_style);
        $(this).addClass('fixedHeader');
        $(this).closest('div').addClass('calcHeight');
    };
}($));

var previewImage = id => {
    let _src = URL.createObjectURL(event.target.files[0]);
    $('#' + id).html(`<img src="${_src}" class="img-thumbnail" style="max-height:120px"/>`);
}

$('[data-widget="collapse"]').unbind().click(e => {
    $(e.currentTarget).find('i').toggleClass('fa-minus fa-plus');
    $(e.currentTarget).parents('.card').find('.card-body').slideToggle();
});


$('[data-widget="remove"]').unbind().click(e => {
    $(e.currentTarget).find('i').toggleClass('fa-minus fa-plus');
    $(e.currentTarget).parents('.form-group').remove();
});

var geoLoactionDetail = {
    Latitude: '',
    Longitute: ''
};


var Q;
(Q => {
    Q.removeEditor = () => {
        if (tinymce.editors.length > 0) {
            tinymce.remove('textarea');
            tinymce.execCommand('mceRemoveEditor', true, 'textarea');
        }
    };

    Q.initEditor = (options) => {
        let settings = $.extend({
            customeDropdown: {
                enable: false,
                dropdownOption: [],
                onselect: function (e) {
                }
            },

        }, options);
        /* Custome Drodown in Editor*/
        var customeDropDown = function (editor) {
            editor.addButton('customeDropdown', {
                type: 'listbox',
                text: 'Select',
                icon: false,
                onselect: settings.customeDropdown.onselect,
                values: settings.customeDropdown.dropdownOption,
                /*onselect: function (e) {
                    tinyMCE.execCommand(this.value());
                    tinyMCE.execCommand('mceInsertContent', false, this.value());
                },
                 onPostRender: function() {
                    //Select the firts item by default
                    this.value('JustifyLeft');
                 }*/
            });
        };
        /*  End */

        var initImageUpload = function (editor) {
            var inp = $('<input id="tinymce-uploader" type="file" name="pic" accept="image/*" style="display:none">');
            $(editor.getElement()).parent().append(inp);
            editor.addButton('imageupload', {
                text: '',
                icon: 'image',
                onclick: function (e) {
                    inp.trigger('click');
                }
            });
            inp.on("change", function (e) {
                uploadFile($(this), editor);
            });
        };

        var uploadFile = function (inp, editor) {
            if (inp.val() !== undefined && inp.val() !== '') {
                var input = inp.get(0);
                var data = new FormData();
                data.append('file', input.files[0]);
                $.ajax({
                    url: '/uploadTinyMCEImage',
                    type: 'POST',
                    data: data,
                    processData: false,
                    contentType: false,
                    success: function (data) {
                        editor.insertContent('<img class="content-img" src="' + data + '"/>');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        if (jqXHR.responseText) {
                            errors = JSON.parse(jqXHR.responseText).errors
                            alert('Error uploading image: ' + errors.join(", ") + '. Make sure the file is an image and has extension jpg/jpeg/png.');
                        }
                    }
                });
            }
        };

        if (tinymce.editors.length > 0) {
            tinymce.remove('textarea');
        }
        tinymce.init({
            selector: 'textarea',
            height: 400,
            theme: 'modern',
            //plugins: ['advlist autolink lists link image charmap print preview hr anchor pagebreak importcss',
            //    'searchreplace wordcount visualblocks visualchars code codesample fullscreen',
            //    'insertdatetime media nonbreaking save table contextmenu directionality',
            //    'emoticons template paste textcolor colorpicker textpattern imagetools'
            //],
            plugins: ['print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap emoticons'],
            toolbar: 'insertfile undo redo  |fontselect  fontsizeselect forecolor backcolor bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent table imageupload | preview code | customeDropdown',
            setup: function (editor) {
                initImageUpload(editor);
                if (settings.customeDropdown.enable)
                    customeDropDown(editor);
            },
            image_advtab: true,
            image_caption: true,
            //quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
            noneditable_noneditable_class: "mceNonEditable",
            toolbar_mode: 'sliding',
            contextmenu: "link image imagetools table",
            templates: [
                { title: 'Test template 1', content: 'Test 1' },
                { title: 'Test template 2', content: 'Test 2' }
            ],
            content_css: ['//www.tinymce.com/css/codepen.min.css'
            ]
        });
    };

    Q.htmlEditor = (options) => {
        let settings = $.extend({
            CustomeDropdown: {
                enable: false,
                DropdownOption: [],
                onselect: function (e) {
                }
            },
        }, options);

        import('/lib/TinyMCE/tinymce.min.js')
            .then(obj => {
                Q.initEditor(settings);
                $(document).on('focusin', function (e) {
                    if ($(e.target).closest(".mce-window").length) {
                        e.stopImmediatePropagation();
                    }
                    if ($(e.target).closest(".mce-window, .moxman-window").length) {
                        e.stopImmediatePropagation();
                    }
                });
            })
            .catch(err => console.log('loading error, no such module exists/n', err));
    };

    Q.pagination = (callBack, isPageNavClicked) => {
        let c = RowCount / $('#ddlTop').val();
        if (parseInt(c) > 0) {
            if (!isPageNavClicked) {
                if ($('#stylePageination').index() === -1) {
                    $('head').append(`<style id="stylePageination">
                                            .pagination { display: flex; max-width: 50%;float: right; margin-top: 4px;}
                                            .pagination a {color: black;float: left;padding: 3px 10px;text-decoration:none;transition: background-color .3s;}
                                            .pagination a.active {background-color: #4CAF50; color: white; border: 1px solid #4CAF50;}
                                            .pagination a:hover:not(.active) {background-color: #ddd;}
                                            .pgButtons {max-height:35px;max-width:411.5px;display:inline-block;overflow:hidden;}
                                            #prePage,#nextPage{border: 1px solid #dee2e6;padding-top: 1px;font-size: 20px;line-height: 20px;}
                                            .page-link{line-height: 1.5;width:40px;text-align:center}</style>`);
                    $('table').wrap('<div class="table-responsive" id="tblResponsive" style="max-height: calc(100vh - 263.5px);overflow:auto"</div>');
                    $('#tblResponsive').wrap('<div id="content_P0"><div>');
                    $('#content_P0').append('<div class="pagination"><a href = "#" class="page-link page-item" id="prePage">&laquo;</a><div class="pgButtons"></div><a href="#" class="page-link page-item" id="nextPage">&raquo;</a></div>');
                }
                let _btns = '';
                for (var k = 0; k < c; k++) {
                    let display = k >= 10 ? 'd-none' : '';
                    _btns += `<a class="page-link ${display} page-item ${k === 0 ? 'active' : ''}" href="#">${k + 1}</a>`;
                }
                //if (parseInt(c) > 0) {
                //    for (var k = 0; k < c; k++) {
                //        _btns += `<a class="page-link page-item ${k === 0 ? 'active' : ''}" href="#">${k + 1}</a>`
                //    }
                //}
                $('.pagination .pgButtons').html(_btns);
                $('.page-item').unbind().click(e => {
                    if (!isNaN($(e.currentTarget).text())) {
                        $('.page-item').removeClass('active');
                        $(e.currentTarget).addClass('active');
                        callBack($(e.currentTarget).text(), true);
                    }
                    else {
                        let currentActive = $('.page-item.active');
                        let _id = $(e.currentTarget).attr('id');
                        if (_id === "nextPage") {
                            let nextItem = currentActive.next('.page-item');
                            if (nextItem.hasClass('d-none')) {
                                let currentText = nextItem.text();
                                currentText = currentText - 11;
                                $('.pgButtons .page-item:eq(' + currentText + ')').addClass('d-none');
                                nextItem.removeClass('d-none');
                            }
                            nextItem.click()
                        }
                        if (_id === "prePage") {
                            let currentText = currentActive.text();
                            if (currentText >= 10) {
                                currentText = currentText - 10 - 1;
                                if (currentText > -1) {
                                    $('.pgButtons .page-item:eq(' + currentText + ')').removeClass('d-none');
                                    currentActive.addClass('d-none');
                                }
                            }
                            currentActive.prev('.page-item').click()
                        }
                    }
                });
            }
        }
        $('table').fixTableHeader();
    };

    Q.geoLoaction = () => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(geoSuccess);
        }
        function geoSuccess(position) {
            geoLoactionDetail.Latitude = position.coords.latitude;
            geoLoactionDetail.Longitute = position.coords.longitude;
        }
    }

    Q.getFormData = ($form) => {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};
        $.map(unindexed_array, function (n) {
            indexed_array[n['name']] = n['value'] === 'on' ? true : n['value'];
        });
        return indexed_array;
    };

    Q.IsFormValid = (option) => new Promise((resolve, reject) => {
        if (!option) {
            option = {
                Selector: '',
                callBack: function () {
                }
            }
        }
        let IsValid = true;
        let element = option.Selector === '' ? '[required="required"]' : option.Selector + ' [required="required"]';
        let totalRequiredTag = $(element).length;
        $('.validation-error').text('').removeClass('text-danger text-monospace error');
        $(element).removeClass('invalid');
        $(element).each(function (i) {
            let _tag = $(this).prop('tagName'), _ele = $(this);
            if (_ele.parent('div').html().indexOf('text-invalid') > -1) {
                _ele.parent('div').find('span.text-invalid').remove();
            }
            if (((_tag === 'SELECT' || _tag === 'INPUT' || _tag === 'TEXTAREA') && (_ele.val() === undefined || _ele.val() === '')) || (_tag === 'SELECT' && (_ele.val() === "0" || _ele.find('option:selected').attr('value') === undefined))) {
                let errorMsg = _ele.attr('data-error');
                let areaDescribe = _ele.attr('aria-describedby');
                if (errorMsg === undefined || errorMsg === '') {
                    errorMsg = 'This is mendetory field';
                }
                if (areaDescribe === undefined) {
                    _ele.addClass('invalid').after('<span class="p-absolute text-danger text-monospace text-invalid"><small>' + errorMsg + '</small></span>');
                } else {
                    _ele.addClass('invalid');
                    $('#' + areaDescribe).text(errorMsg).addClass('text-danger text-monospace validation-error');
                }
                _ele.focus();
                IsValid = false;
            }
            if ((i + 1) === totalRequiredTag) {
                if (IsValid) {
                    resolve(IsValid);
                }
                else {
                    reject('Form is not valid');
                }
            }
        });
    });

    Q.formatUnformattedKey = (unformattedKey) => {
        let result = unformattedKey.match(/.{1,4}/g);
        return result.join(' ');
    };

    Q.base64ToArrayBuffer = (base64) => {
        var binaryString = window.atob(base64);
        var binaryLen = binaryString.length;
        var bytes = new Uint8Array(binaryLen);
        for (var i = 0; i < binaryLen; i++) {
            var ascii = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    };
    Q.numberToWords = (number) => {
        var digit = ['zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine'];
        var elevenSeries = ['ten', 'eleven', 'twelve', 'thirteen', 'fourteen', 'fifteen', 'sixteen', 'seventeen', 'eighteen', 'nineteen'];
        var countingByTens = ['twenty', 'thirty', 'forty', 'fifty', 'sixty', 'seventy', 'eighty', 'ninety'];
        var shortScale = ['', 'thousand', 'million', 'billion', 'trillion'];

        number = number?.toString(); number = number?.toString().replace(/[\, ]/g, '');
        if (number != parseFloat(number)) return 'not a number';
        var x = number?.toString().indexOf('.');
        if (x == -1) x = number?.toString().length;
        if (x > 15) return 'too big';
        var n = number?.toString().split('');
        var str = '';
        var sk = 0;
        for (var i = 0; i < x; i++) {
            if ((x - i) % 3 == 2) {
                if (n[i] == '1') { str += elevenSeries[Number(n[i + 1])] + ' '; i++; sk = 1; }
                else if (n[i] != 0) { str += countingByTens[n[i] - 2] + ' '; sk = 1; }
            }
            else if (n[i] != 0) {
                str += digit[n[i]] + ' ';
                if ((x - i) % 3 == 0) str += 'hundred '; sk = 1;
            }
            if ((x - i) % 3 == 1) { if (sk) str += shortScale[(x - i - 1) / 3] + ' '; sk = 0; }
        }
        if (x != number.length) {
            var y = number.length; str += 'point ';
            for (var i = x + 1; i < y; i++) str += digit[n[i]] + ' ';
        }
        str = str.replace(/\number+/g, ' ');
        return str.trim() + ".";
    }
})(Q || (Q = {}));

var getXmlAsString = function (xmlDom) {
    return (typeof XMLSerializer !== "undefined") ?
        (new window.XMLSerializer()).serializeToString(xmlDom) :
        xmlDom.xml;
}

class ShowJsTimer {
    constructor(elem, timeInMinutes) {
        this._elem = elem;
        this._timeInMinutes = timeInMinutes === undefined ? 5 : timeInMinutes;
    }
    startTimer() {
        let currentTime = new Date();
        currentTime.setMinutes(currentTime.getMinutes() + this._timeInMinutes);
        let __this = this;

        __this._st = setInterval(function () {
            let now = new Date().getTime();
            let diff = currentTime - now;
            var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((diff % (1000 * 60)) / 1000);
            if (minutes > -1 && seconds > -1) {
                __this._elem.innerHTML = minutes.toString().padStart(2, 0) + ":" + seconds.toString().padStart(2, 0);
            }
            else {
                __this._elem.innerHTML = "00:00";
            }
            if (diff <= 0) {
                clearInterval(__this._st);
            }
        }, 1000);
    }
    stopTimer(f) {
        if (this._st === undefined)
            return true;
        clearInterval(this._st);
        if (f === undefined)
            return true;
        f();
    }
}

const Two_FA_Win = (__this = null) => {
    preloader.load();
    let otp = "";
    if (__this != null)
        otp = $(__this).val();
    $.post('/GoogleAuthenticatorSetup', { otp }).done(result => {
        if (typeof (result) === 'object') {
            if (result.statuscode == 2) {
                let _html = `<div class="row"><div class="col-sm-12"><div class="form-group">
                                <input type="text" class="form-control" placeholder="Please enter otp" name="otp" id="txtOTP"/>
                            </div>
                            <div class="form-group"><button class="btn btn-dark" onclick="Two_FA_Win('#txtOTP')">Submit</button></div>
                            </div></div>`;
                mdlA.id = "otpwin";
                mdlA.title = "OTP";
                mdlA.content = _html;
                mdlA.modal(mdlA.size.small);
                an.title = "Alert";
                an.content = result.msg;
                an.alert(result.statuscode);
            }
            if (result.statuscode == -1) {
                an.title = "Oops!";
                an.title = result.msg;
                an.alert(result.statuscode);
            }
        }
        else {
            mdlA.dispose();
            let el = $('#googleAuthWin');
            if (el[0]) {
                el.find('.modal-body').html(result);
            }
            else {
                mdlA.id = 'googleAuthWin';
                mdlA.title = 'Google Authenticator';
                mdlA.content = result;
                mdlA.modal(mdlA.size.large)
            }
        }
    }).fail(xhr => {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(() => preloader.remove());
};

var bottomIcons = () => {
    $('body').append(`<div id="divbottomIcons" style="position: fixed;bottom:10px;right:-12px;max-width:100px;overflow:hidden;z-index:1;text-align:center"></div>`).delay(5000, function () {
        if (serverSetting.applicationSetting.IsCallbackAlert && serverSetting.roleName.toLowerCase() != 'admin') {
            $('#divbottomIcons').append(`<a class="callMe"><img src="/images/CallMe.gif" class="img-fluid rounded-circle cus-image" onclick="callMe()"/></a>`);
        }
        if (serverSetting.applicationSetting.IsSocialAlert && serverSetting.roleName.toLowerCase() != 'admin') {
            $.post('/GetWISection', {}, function (result) {
                $('#divbottomIcons').append(result);
            }).catch(function (err) {
                console.log(err);
            }).fail(function (xhr) {
                an.title = 'Oops';
                an.content = xhr.status == 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            });
        }
    })
};

var callMe = () => {
    $('.callMe').unbind().click(function () {
        mdlA.id = 'usrContactmodel';
        mdlA.content = `<div class="col-md-12">
                                <button type="button" class="close" aria-label="Close">
                                <span aria-hidden="true">×</span></button>
                                <h5 class="text text-info">Call Me Request</h5><hr/>
                                <div class="form-group">
                                    <input type="text" placeholder="EnterMobile" class="form-control" id="txtUserContactNo" maxlength="10"/>
                                </div>
                                <button class="btn btn-outline-dark mr-2" id="btnSubmitCallMeReq">Submit</button>
                                <button class="btn btn-dark" id="mdlCancel">Cancel</button>
                            </div>`;
        mdlA.options.backdrop = 'static';
        mdlA.alert(mdlA.size.small);
        let mobile = $('#UINFO span.dropdown-item-text').data().itemMobile;
        $('#txtUserContactNo').val(mobile);
        $('button.close span,#mdlCancel').unbind().click(() => mdlA.dispose());
        $('#btnSubmitCallMeReq').unbind().click(() => {
            let userContactNo = $('#txtUserContactNo').val();
            preloader.load();
            $.post('/callme', { uMob: userContactNo })
                .done(result => {
                    an.title = result.statuscode == an.type.success ? 'Success' : 'Oops';
                    an.content = result.msg;
                    an.alert(an.type.success);
                    mdlA.dispose();
                })
                .always(() => preloader.remove());
        });
    });
};

$(document).on('keypress', $('button:last'), function (event) {
    var keycode = event.keyCode ? event.keyCode : event.which;
    if (keycode === 13) {
        let lastButton = $("button:last");
        if (lastButton.text().toLowerCase() !== 'delete') {
            $("button:last").click();
        }
    }
});