"use strict";
var btnLoadingClass = '<i class="fas fa-circle-notch fa-spin"></i> ';
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
    size: { small: 'modal-sm', large: 'modal-lg', xlarge: 'modal-xl', xxlarge: 'modal-xxl', xxlargeM: 'modal-xxl-m', default: '' },
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
        $('button.close').unbind().click(() => {
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
var preloader = {
    load: () => $('body').append('<div class="loading">Loading&#8230;</div>'),
    remove: () => $('.loading').remove()
};
var $v = $validator;
var an = alertNormal;
var mdlA = modalAlert;
$(document).ready(function () {
    an.autoClose = 10;
    $(".dropdown-toggle").dropdown();
    an.id = 'myalert';
});
var getXmlAsString = function (xmlDom) {
    return (typeof XMLSerializer !== "undefined") ?
        (new window.XMLSerializer()).serializeToString(xmlDom) :
        xmlDom.xml;
} 
var AEPSStatusText = { PENDING: 1, SUCCESS: 2, FAILED: 3, ERROR: 0 };
function getKeyByValue(object, value) {
    return Object.keys(object).find(key => object[key] === value);
}