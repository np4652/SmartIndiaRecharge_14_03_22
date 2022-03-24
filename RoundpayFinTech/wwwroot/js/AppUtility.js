"use strict";
var ChangePass = function (isMandate) {
    preloader.load();
    $.post('/Change-Password', { IsMandate: isMandate }, function (result) {
        resultReload(result);
        $('#' + an.id).remove();
        mdlA.id = 'myalert';
        mdlA.content = result;
        mdlA.options.backdrop = 'static';
        if (isMandate === true) {
            mdlA.options.keyboard = false;
        }
        mdlA.alert(mdlA.size.small);
        $('button.close span,#mdlCancel').click(() => mdlA.dispose());
        $("#btnChangePass").click(function () {
            let $v = $validator;
            let ct = {
                oldp: $("#txtOldPassword"), newp: $('#txtNewPassword'), confirmp: $("#txtConfirmPassword")
            };
            let IsE = { oldp: (ct.oldp.val().trim() === ''), newp: (ct.newp.val().trim() === ''), confirmp: (ct.confirmp.val().trim() === ''), samep: (ct.newp.val().trim() === ct.oldp.val().trim()), notsameconfirm: (ct.newp.val().trim() !== ct.confirmp.val().trim()) };
            $v.showErrorFor(ct.oldp, 'Enter Old Password', IsE.oldp);
            $v.showErrorFor(ct.newp, 'Enter New Password', IsE.newp);
            $v.showErrorFor(ct.confirmp, 'Enter Confirm Password', IsE.confirmp);


            IsE.err = IsE.oldp || IsE.newp || IsE.confirmp;
            if (IsE.err) {
                return false;
            }
            $v.showErrorFor(ct.newp, 'New Password Cannot Be Same As Old Password', IsE.samep);
            IsE.err = IsE.samep;
            if (IsE.err) {
                return false;
            }
            $v.showErrorFor(ct.confirmp, 'Confirm Password Do Not Match', IsE.notsameconfirm);
            IsE.err = IsE.notsameconfirm
            if (IsE.err) {
                return false;
            }
            var UserData = {
                UserId: "",
                OldPassword: ct.oldp.val().trim(),
                NewPassword: ct.newp.val().trim()
            };
            $.ajax({
                type: 'POST',
                url: '/ChangePassword',
                dataType: 'json',
                contentType: 'application/json',
                data: JSON.stringify(UserData),
                success: function (result) {
                    resultReload(result);
                    an.title = result.statuscode === an.type.success ? "Well done" : "Oops";
                    an.content = result.msg;
                    an.alert(result.statuscode);
                    if (result.statuscode === an.type.success) {
                        mdlA.dispose();
                    }
                },
                error: function (xhr, result) {
                    an.title = "Oops! Error";
                    an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
                    an.alert(an.type.failed);
                    if (result === 'parsererror') {
                        reload();
                    }
                },
                complete: () => btnLdr.Stop(ct.btn)
            });
        });
    }).fail(xhr => {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(() => preloader.remove());
};

var ChangePin = function (isMandate) {
    preloader.load();
    $.post('/Change-Pin', { IsMandate: isMandate }, function (result) {
        resultReload(result);
        $('#' + an.id).remove();
        mdlA.id = 'myalert';
        mdlA.content = result;
        mdlA.options.backdrop = 'static';
        if (isMandate === true) {
            mdlA.options.keyboard = false;
        }
        mdlA.alert(mdlA.size.small);
        $('button.close span,#mdlCancel').click(() => mdlA.dispose());

        $("#btnChangePass").click(function () {
            var $v = $validator;
            var ct = {
                oldp: $("#txtOldPassword"), newp: $('#txtNewPassword'), confirmp: $("#txtConfirmPassword")
            };
            var IsE = { oldp: (ct.oldp.val().trim() === ''), newp: (ct.newp.val().trim() === ''), confirmp: (ct.confirmp.val().trim() === ''), samep: (ct.newp.val().trim() === ct.oldp.val().trim()), notsameconfirm: (ct.newp.val().trim() !== ct.confirmp.val().trim()) }
            $v.showErrorFor(ct.oldp, 'Enter Password', IsE.oldp);
            $v.showErrorFor(ct.newp, 'Enter New Pin', IsE.newp);
            $v.showErrorFor(ct.confirmp, 'Enter Confirm Pin', IsE.confirmp);

            IsE.err = IsE.oldp || IsE.newp || IsE.confirmp;
            if (IsE.err) {
                return false;
            }
            $v.showErrorFor(ct.newp, 'New Pin Cannot Be Same As Old Password', IsE.samep);
            IsE.err = IsE.samep;
            if (IsE.err) {
                return false;
            }
            $v.showErrorFor(ct.confirmp, 'Confirm Pin Do Not Match', IsE.notsameconfirm);
            IsE.err = IsE.notsameconfirm;
            if (IsE.err) {
                return false;
            }
            var UserData = {
                UserId: "",
                OldPassword: ct.oldp.val().trim(),
                NewPassword: ct.newp.val().trim()
            };
            $.ajax({
                type: 'POST',
                url: '/ChangePin',
                dataType: 'json',
                contentType: 'application/json',
                data: JSON.stringify(UserData),
                success: function (result) {
                    resultReload(result);
                    an.title = result.statuscode === an.type.success ? 'Well done' : 'Oops';
                    an.content = result.msg;
                    an.alert(result.statuscode);
                    if (result.statuscode === an.type.success) {
                        mdlA.dispose();
                    }
                },
                error: function (xhr, result) {
                    an.title = "Oops! Error";
                    an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
                    an.alert(an.type.failed);
                    if (result === 'parsererror') {
                        reload();
                    }
                },
                complete: () => btnLdr.Stop(ct.btn)
            });
        });
    }).fail(xhr => {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(() => preloader.remove());
};

var recentLoginActivity = () => {
    $.post('/recent-login-activity', { TopRow: 100, outputInjson: true }, function (result) {
        let _html = `<div class="table-responsive"><table class="table" id="recentLoginActivity">
                        <thead><tr><th>#</th><th>UserId</th><th>IP</th><th>Remark</th><th>Date</th><th>Longitude</th><th>Latitude</th><th></th></tr><thead>
                        <tbody>{{tr}}</tbody></table></div>`;
        let tr = '';
        for (let i = 0; i < result.length; i++) {
            tr = tr + `<tr>
                            <td>${i + 1} ${result[i].isActive ? '<div class="led-green led-sm float-right">' : ''}</td>
                            <td>${result[i].loginMobile !== '' ? result[i].loginMobile : result[i].prefix + result[i].loginID}</td>
                            <td>${result[i].requestIP}</td>
                            <td>${result[i].commonStr}</td>
                            <td>${result[i].commonStr2}</td>
                            <td>${result[i].longitude}</td>
                            <td>${result[i].latitude}</td>
                            <td>${result[i].latitude !== 0 && result[i].longitude !== 0 ? '<a href="https://www.google.com/maps/@' + result[i].latitude + ',' + result[i].longitude + ',15z" target="_blank"><i class="fas fa-map text-info"></i></a>' : ''} </td>
                       </tr>`;
        }
        mdlA.title = 'Recent Login Activity';
        mdlA.id = 'recentLoginActivityWin';
        mdlA.content = _html.replace('{{tr}}', tr);
        mdlA.options.backdrop = 'static';
        mdlA.modal(mdlA.size.xlarge);
        $('#recentLoginActivity').fixTableHeader();
    }).fail(xhr => {
        an.title = 'Oops';
        an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        an.alert(an.type.failed);
    }).always(() => preloader.remove());
};

var viewBrand = () => {
    $.post('/Brand')
        .done(result => {
            mdlA.id = "brandView";
            mdlA.content = result;
            mdlA.title = "Brand";
            mdlA.modal(mdlA.size.default);
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });
};

var showEmailAndSocial = () => {

    var nowDate = new Date();

    var date = nowDate.getFullYear() + '' + (nowDate.getMonth() + 1) + '' + nowDate.getDate();

    var chkDT = localStorage.getItem("SAlertTime", date);

    if (date !== chkDT) {
        $.post('/_ShowSocialPopup').done(result => {
            if (result === false) {
                console.log(result);
            }
            else {
                mdlA.id = "ShowSocialPopup";
                mdlA.title = '';
                mdlA.headerClass = 'h3 text-danger text-center text-monospace';
                mdlA.content = result;
                mdlA.modal(mdlA.size.default, () => { localStorage.setItem("SAlertTime", date); });
            }
        })
    }
}

(() => {

    $.ajax({
        type: 'POST',
        url: '/LoginInfo',
        dataType: 'json',
        contentType: 'application/json',
        success: function (result) {
            if (result.name === undefined)
                reload();
            else {
                setInfo(result);
                $('body').removeAttr('class');
            }
        },
        error: function (xhr, result) {
            an.title = "Oops! Error";
            an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
            an.alert(an.type.failed);
            if (result === 'parsererror') {
                reload();
            }
        }
    });

    $.post('/mybal')
        .done(result => {

            if (result.isShowLBA === true) {
                var _html = `<div class="col-md-12">
                                    <div class="col-md-12 text-center">
                                        <img src="/Image/iconMsg/9.png">
                                        <div class="">You Have Low Balance</div>
                    <div><small>Your Current Balance : ${result.balance}</small></div>
                    <button class="btn btn-outline-success btn-lg" id="btnRedirectToFund">Fund Request</button>
                    ${result.isAddMoneyEnable ? `<button class="btn btn-outline-info btn-lg" id="btnRedirectToAddMoney">Add Money</button>` : `<span></span>`}
                    </div>`;
                mdlA.id = "lowBalanceAlert";
                mdlA.title = 'Low Balance Alert';
                mdlA.headerClass = 'h3 text-danger text-center text-monospace';
                mdlA.content = _html;
                mdlA.modal(mdlA.size.large);
                $('#btnRedirectToFund').click(() => {
                    sessionStorage.setItem('IsAlert', 1);
                    window.location.href = "/fund-request";
                });
                $('#btnRedirectToAddMoney').click(() => {
                    sessionStorage.setItem('IsAlert', 1);
                    window.location.href = "/addmoney";
                });
            }
            else {
                showEmailAndSocial();
            }


            $('.custom.bal').html(`<i class="fas fa-wallet text-warning"></i> ${result.sb}`);
            if (result.isP) {
                ChangePass(true);
            }
            else if (result.isPN) {
                ChangePin(true);
            }
        })
        .fail(xhr => {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        });

    $.post('/IsVendor').done(result => {
        if (result.statuscode === an.type.success && result.commonBool) {
            $(`ul.navbar-nav`).append(`<li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="dwnShopping" data-toggle="dropdown">Shopping</a>
                        <div class="dropdown-menu" area-labelledby="dwnShopping">
                            <a href="javascript:viewBrand()" class="dropdown-item">Brand</a>
                            <a href="/AddProduct" class="dropdown-item">Add Product</a>
                            <a href="/AllProduct" class="dropdown-item">All Product</a>
                            <a href="/shopping/OrderList" class="dropdown-item">Orders</a>
                            <a href="/shopping/OrderReport" class="dropdown-item">Orders Report</a>
                        </div>
                    </li>`);
        }
    })
})();

var closeNotification = (id, userID, EntryDate, IsHover = false) => {
    $.post('/CloseNotification', { id: id, userID: userID, EntryDate: EntryDate }).done(result => {
        if (result.statuscode === 1) {
            if (!IsHover) {
                $(`.notification-${id}`).remove();
                $('div.notification').length === 0 ? $('.markRead').remove() : '';
            }
        }
    });
};

(($) => {
    if ($$IsWebNotify == 'True') {
        $.loadNotification = $.loadNotification || {}
        var NotificationList, AllNotification, IsAutoClicked = false;
        $.loadNotification = (isShowAll) => {
            $.post('/WebNotification', { IsShowAll: isShowAll }).done(result => {
                if (result.isWebNotification) {
                    isShowAll === false ? NotificationList = result.notification : AllNotification = result.notification;
                    if (isShowAll === false) {
                        if ($('#IconNotification').length === 0) {
                            $('.top-nav li:last').before(`<li>
                                                        <a class="nav-link text-white font-bold" id="IconNotification">
                                                            <i class="fa fa-bell text-warning"></i><sup style="top:-1.5em">${NotificationList.length}</sup>
                                                        </a>
                                                      </li>`);
                        }
                        else {
                            $('#IconNotification').closest('li').find('sup').text(NotificationList.length)
                            //$('#NotificationSection').remove();
                        }
                        if ($('#NotificationSection').length < 1) {
                            $.when($('body').append(`<div id="NotificationSection" class="position-fixed notification-div r-b"></div>`))
                                .then(() => {
                                    if (NotificationList.length > 0) {
                                        $('#NotificationSection').append(`<div class="markRead">
                                                                    <span class="form-control"><input type="checkbox" id="chkRead"> <label>Mark all as read</label></span>
                                                                  </div>`);
                                        $('#chkRead').click(e => {
                                            if ($(e.currentTarget).is(':checked')) {
                                                $.post('/markallread').done(result => {
                                                    $('#NotificationSection').remove();
                                                });
                                            }
                                        });
                                    }
                                    for (let i = 0; i < NotificationList.length; i++) {
                                        let m = NotificationList[i];
                                        $('#NotificationSection').append(`<div class="alert-dismissible p-2 mt-1 notification notification-${m.id}"  onmouseover="closeNotification('${m.id}', '${m.userID}','${m.entryDate}',true)">
                                                        <span class="float-right close" onclick="closeNotification('${m.id}', '${m.userID}','${m.entryDate}')">
                                                            <i class="fa fa-times"></i>
                                                        </span>
                                                        <strong class="alert-heading">${m.title}!</strong>                                                        
                                                        <div class="col-sm-12 text-center ${m.img === '' ? 'd-none' : ''}">
                                                            <img src="/Image/Webnotification/${m.img}" style="height:250px;object-fit:cover"/>
                                                        </div>
                                                        <p style="font-family:cursive">${m.notification}<br/>
                                                            <small class="float-right"><i class="fa fa-clock pr-1"></i>${m.entryDate}</small></p>
                                                     </div>`);
                                    }
                                });
                        }
                        else {
                            if (NotificationList.length > 0) {
                                $('#NotificationSection').append(`<div class="markRead">
                                                                    <span class="form-control"><input type="checkbox" id="chkRead"> <label>Mark all as read</label></span>
                                                                  </div>`);
                                $('#chkRead').click(e => {
                                    if ($(e.currentTarget).is(':checked')) {
                                        $.post('/markallread').done(result => {
                                            $('#NotificationSection').remove();
                                        });
                                    }
                                });
                            }
                            for (let i = 0; i < NotificationList.length; i++) {
                                let m = NotificationList[i];
                                $('#NotificationSection').append(`<div class="alert-dismissible p-2 mt-1 notification notification-${m.id}"  onmouseover="closeNotification('${m.id}', '${m.userID}','${m.entryDate}',true)">
                                                        <span class="float-right close" onclick="closeNotification('${m.id}', '${m.userID}','${m.entryDate}')">
                                                            <i class="fa fa-times"></i>
                                                        </span>
                                                        <strong class="alert-heading">${m.title}!</strong>                                                        
                                                        <div class="col-sm-12 text-center ${m.img === '' ? 'd-none' : ''}">
                                                            <img src="/Image/Webnotification/${m.img}" style="height:250px;object-fit:cover"/>
                                                        </div>
                                                        <p style="font-family:cursive">${m.notification}<br/>
                                                            <small class="float-right"><i class="fa fa-clock pr-1"></i>${m.entryDate}</small></p>
                                                     </div>`);
                            }
                        }
                        $('#IconNotification').click(e => {
                            if (AllNotification !== undefined && AllNotification.length > 0) {
                                if ($(e.currentTarget).parent('li').find('div').length > 0) {
                                    $(e.currentTarget).parent('li').find('div').remove();
                                }
                                else {
                                    let _html = '';
                                    for (let i = 0; i < AllNotification.length; i++) {
                                        let m = AllNotification[i];
                                        let read = AllNotification[i].isSeen ? '<i class="fa fa-check-circle text-success"></i>' : "";
                                        _html += `<li><strong>${m.title}</strong><p><span>${read}</span> ${m.notification}</br><small class="float-right"><i class="fa fa-clock pr-1"></i>${m.entryDate}</small></p></li>`;
                                    }
                                    $(e.currentTarget).parent('li').append(`<div class="IconNotification"><ul>${_html}</ul></div>`);
                                }
                            }
                            else {
                                if ($(e.currentTarget).parent('li').find('ul').length === 0) {
                                    if (IsAutoClicked === false) {
                                        $.loadNotification(true);
                                        IsAutoClicked = true
                                    }
                                }
                                else {
                                    $(e.currentTarget).find('ul').remove();
                                }
                            }
                        });
                    }
                    else {
                        $('#IconNotification').click();
                    }
                }
            });
        };
        $.loadNotification(false);
        setInterval(() => $.loadNotification(false), 60000);
    }
})($);