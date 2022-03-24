
/*const { cookies } = require("modernizr");*/

var page = '/';
var errorMsg = $('#errorMsg');
var LoginStatus = { FAILED: -1, SUCCESS: 1, OTP: 2 };
var AlertStatus = { RED: 0, GREEN: 1, BLUE: 2 };


var alertNormal = {
    title: '',
    content: '',
    color: { green: 'alert-success', red: 'alert-danger', blue: 'alert-info', warning: 'alert-warning' },
    linkClass: 'alert-link',
    iclass: { failed: 'fa fa-times-circle', warning: 'fa fa-exclamation-triangle', success: 'fa fa-check-circle', info: 'fa fa-info-circle' },
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
        $('#alertmsg').addClass('wish-alert');
        $('#alertmsg').html(this.div.replace('{id}', this.id).replace('{title}', this.title).replace('{content}', this.content).replace('{color}', cls).replace('{iclass}', icls));
        this.show();
        if (this.autoClose > 0) {
            $('body').removeClass('modal-open');
            $('body').removeAttr('style');
            setTimeout(function () {
                alertNormal.close();
            }, this.autoClose * 1000);
        }
    },
    close: function () {
        $('#' + this.id).removeClass('show');
        //$('#alertmsg').removeClass('wish-alert');
        $('#alertmsg').html('');
    },
    show: function () {
        $('#' + this.id).addClass('show');
    },
    autoClose: 5,
    remove: function () {
        $('#' + this.id).remove();
    }
};

var modalAlert = {
    title: '',
    content: '',
    parent: $('#ECommBody'),
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
        this.div = this.div.replace('{id}', mdlId);
        $('#alertmsg').html(this.div);
        //this.parent.append(this.div.replace('{id}', mdlId));
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
        $('#' + mdlId).modal('show');
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

var myModal = {
    id: 'mymodal',
    div: '<div class="modal fade" id="{divModalId}" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true" data-keyboard="false" data-backdrop="static">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document" >'
        + '<div class="modal-content">'
        + '<div class="modal-header">'
        + '<h5 class="modal-title" id="exampleModalLabel">Modal title</h5>'
        + '<button type="button" class="close" data-dismiss="modal" aria-label="Close">'
        + '<span aria-hidden="true">&times;</span>'
        + '</button>'
        + '</div>'
        + '<div class="modal-body">'
        + '</div></div></div ></div>',
    div1: '<div class="modal fade" id="{divModalId}" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true" data-keyboard="false" data-backdrop="static">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document" >'
        + '<div class="modal-content">'
        + '<div class="modal-body">'
        + '</div></div></div ></div>',
    show: function (id, size, title, content) {
        if (title == '') {
            this.div = this.div1.replace('{divModalId}', id);
        }
        else {
            this.div = this.div.replace('{divModalId}', id);
        }
        $('#alertmodal').html(this.div);
        $('#' + id + ' .modal-title').html(title);
        $('#' + id + ' .modal-body').html(content);
        $('#' + id + ' .modal-dialog').addClass(size);
        $('#' + id).modal('show');
    },
    dispose: function (f) {
        var mdlId = this.id;
        this.div = this.div.replace(mdlId, '{divModalId}');
        $('#' + mdlId + ' .modal-content').animate({ opacity: 0 }, 500, function () {
            $('#' + mdlId + ',.modal-backdrop').remove();
            $('#alertmodal').html('');
            if (f !== undefined)
                f();
        });
    }
};

var myModalA = {
    id: 'mymodal',
    div: '<div class="modal modalz fade" id="{divModalId}" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true" data-keyboard="false" data-backdrop="static">'
        + '<div class= "modal-dialog modal-dialog-centered" role="document" >'
        + '<div class="modal-content">'
        + '<div class="modal-header">'
        + '<h5 class="modal-title" id="exampleModalLabel">Modal title</h5>'
        + '<button type="button" class="close" data-dismiss="modal" aria-label="Close">'
        + '<span aria-hidden="true">&times;</span>'
        + '</button>'
        + '</div>'
        + '<div class="modal-body">'
        + '</div></div></div ></div>',
    show: function (id, size, title, content) {
        this.div = this.div.replace('{divModalId}', id);
        $('body').append(this.div);
        $('#' + id + ' .modal-title').html(title);
        $('#' + id + ' .modal-body').html(content);
        $('#' + id + ' .modal-dialog').addClass(size);
        $('#' + id).modal('show');
    },
    dispose: function (id) {
        $('#' + id).remove();
    }
};


var anEC = alertNormal;

var mdlE = myModal;
var mdlAp = myModalA;

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

var MyWalletBalance = function () {
    $.post('/ECBal')
        .done(result => {
            console.log(result);
            $('#myBalance').append('<span>&#8377; ' + result.balance + '</span>');
            //mdlE.id = 'mdlBalance';
            //mdlE.show('mdlBalance', 'modal-lg', '', result);
            //$('button.close').click(() => {
            //    mdlE.dispose();
            //});
        }).fail(xhr => {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            anEC.alert(anEC.type.failed);
        })
};

var ECommLogin = (path = '') => {
    jQuery.get('/EcommLogin').done(result => {
        //preloader.load();
        mdlE.id = 'mdlLogin';
        /*mdlE.show('mdlLogin', 'modal-lg', 'Login', result);*/
        $.when(mdlE.show('mdlLogin', 'modal-lg', 'Login', result)).done(function () {
            $('#hdnPath').val(path);
        });

        $('button.close').click(() => {
            mdlE.dispose();
        });
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
        //$('.info_shopping_icon').unbind().mouseover(function () {
        //    shoppingMoouseOver(this);
        //});
    });
};

var Login = function (path = '') {
    errorMsg.removeClass('hide');
    var U = $('#txtUser'), P = $('#txtPassword'); //, T = $('#ddlLType');
    var UserID = U.val(), Password = P.val();//, LoginTypeID = T.val();
    var LoginDetail = {};
    LoginDetail.LoginMobile = UserID;
    LoginDetail.LoginTypeID = 1;
    //if (P.attr('type') === 'text')
    //    LoginDetail.OTP = Password;
    //else
    LoginDetail.Password = Password;
    var URL = 'ECLogin';

   // preloader.load();
    $.ajax({
        type: 'POST',
        url: URL,
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {
            if (result.statuscode === undefined || result.statuscode === LoginStatus.FAILED) {
                anEC.title = "Oops";
                anEC.content = result.msg;
                anEC.alert(anEC.type.failed);
            }
            else if (result.statuscode === LoginStatus.SUCCESS) {
                anEC.title = "Wow";
                anEC.content = result.msg;
                anEC.alert(anEC.type.success);
                mdlE.dispose();
                GetUserInfo();
                //CartCount();
                //WishlistCount();
                if (path != '') {
                    location.href = path;
                }
            }
            else if (result.statuscode === LoginStatus.OTP) {
                P.attr({ placeholder: 'Enter OTP', type: 'text' });
                $('#btnResendOTP').html('<a href="javascript:void(0)">Resend OTP</a>')
                    .css({ 'position': 'relative', 'top': '-16px', 'right': '-182px' })
                    .click(function () {
                        _ReSendOTP();
                    });
                Alerts('Enter OTP', AlertStatus.BLUE);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
                anEC.title = 'Info';
                anEC.content = result.msg;
                anEC.alert(anEC.type.info);
                P.val('');
                P.focus();
            }
        }, statusCode: {
            500: function () {
                anEC.title = 'Oops';
                anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
                anEC.alert(-1);
            },
            0: function () {
                anEC.title = 'Oops';
                anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
                anEC.alert(-1);
            }
        },
        error: function (xhr, result) {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
            anEC.alert(-1);
        },
        complete: function () {
            //preloader.remove();
        }
    });
};

var GetUserInfo = () => {
    jQuery.post('/GetUserInfo').done(result => {
        //preloader.load();
        if (result.statuscode == 1) {
            $.when(SetUserInfo(result)).done(function () {
                MyWalletBalance();
            });
        }
        CartCount();
        WishlistCount();
        $('#logout,#logoutAll').click(function () {
            var s = $(this).attr("id") === 'logoutAll' ? 3 : 1;
            Logout(0, 0, s);
        });
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
    });
};

var SetUserInfo = (res) => {
    $('#liSignIn').html(` <div class="dropdown" id="UINFO">
                                <a class="authorization-link dropdown-toggle" type="button" id="drpdn_LoginInfo" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">${res.name}</a>
                                <div class="dropdown-menu dropdown-menu-right" aria-labelledby="drpdn_LoginInfo">
                                    <span class="dropdown-item-text"></span>
                                    <a class="dropdown-item" href="javascript:void(0)" id="myBalance" onclick="MyWalletBalance();">My Balance: </a>
                                    <a class="dropdown-item" href="/ECommOrderReport">Order Report</a>
                                    <a class="dropdown-item" href="#" id="logout">Logout</a>
                                </div>
                            </div>`);
    //$('#UINFO button.dropdown-toggle').html(res.name);
    //$('#UINFO span.dropdown-item-text').html(res.outletName);
    //$('#UINFO span.dropdown-item-text').attr("data-item-mobile", res.mobileNo);
};

var Logout = function (u, uid, st) {
    $.post('/ECLogout', { ULT: u, UserID: uid, SType: st }, function (result) {
        if (result.statuscode === anEC.type.success) {
            sessionStorage.clear();
            //location.reload();
            location.href('/shopping');
        }
        else {
            anEC.title = 'Oops';
            anEC.content = result.msg;
            anEC.alert(anEC.type.failed);
        }
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        anEC.alert(anEC.type.failed);
    }).always(() => preloader.remove());
};

var GetLocalCart = () => {
    jQuery.post('/GetUserInfo').done(result => {
        //preloader.load();
        anEC.title = result.statuscode == 1 ? 'Wow' : 'Oops';
        anEC.content = result.msg;
        anEC.alert(result.statuscode);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
    });
}

var BindMenu = () => {
    jQuery.post('/ECommMenu').done(result => {
        //preloader.load();
        $('#divECommMenu').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
        //$('.info_shopping_icon').unbind().mouseover(function () {
        //    shoppingMoouseOver(this);
        //});
    });
};

var BindCategoriesForIndex = () => {
    jQuery.post('/BindCatForIndex').done(result => {
        //preloader.load();
        $('#categoryGrid').html(result);
        //$('#divIndexCategories .nav_zero').click();
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
        //$('.info_shopping_icon').unbind().mouseover(function () {
        //    shoppingMoouseOver(this);
        //});
    });
};

var getProductGrid = (mainCatId, subCatId) => {
    $.post('ProductGridForIndex', { CategoryID: mainCatId, SubCategoryID1: subCatId }).done(result => {
        $('#divCatWiseProGrid_' + mainCatId).html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    });
};

var getProductNewArrivalGrid = (mainCatId, catId, subCatId) => {
    let param = {
        CategoryID: mainCatId,
        SubCategoryID1: catId,
        SubCategoryID2: subCatId
    };
    jQuery.post('/ECommNewArrival', param).done(result => {
        $('#catNew').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    });
};

var getProductTrendingGrid = (mainCatId, catId, subCatId) => {
    let param = {
        CategoryID: mainCatId,
        SubCategoryID1: catId,
        SubCategoryID2: subCatId
    };
    jQuery.post('/ECommTrending', param).done(result => {
        $('#trendingGrid').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    });
};

var getProductSimilarGrid = (PId, PdId) => {
    let param = {
        ProductId: PId,
        ProductDetailId: PdId
    };
    jQuery.post('/ECommSimilar', param).done(result => {
        $('#divProductSimilar').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    });
};

var shoppingMoouseOver = function (sender) {
    let _this = sender;
    let pdId = $(_this).parent().parent().data().productdetailId;
    $.post('/GetUserCommission', { pdId })
        .done(result => {
            if (result.status == 1) {
                $(_this).attr('title', 'Comm: ₹' + result.commonStr);
            }
            else {
                $(_this).attr('title', result.msg);
            }
        })
        .fail(xhr => {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
            anEC.alert(-1);
        }).always(function () {
            preloader.remove();
            $('.info_shopping_icon').unbind().mouseover(function () {
                shoppingMoouseOver(this);
            });
        });
}

var loadProduct = (CategoryID, subCategoryID1, subCategoryID2) => {
    let param = {
        CategoryID: CategoryID,
        SubCategoryID1: subCategoryID1,
        SubCategoryID2: subCategoryID2
    };
    $.post('_ProductForIndex', param).done(result => {
        preloader.load();
        $('#renderProducts').append(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server ErrorTest";
        anEC.alert(-1);
    }).always(function () {
        preloader.remove();
        $('.info_shopping_icon').unbind().mouseover(function () {
            shoppingMoouseOver(this);
        });
    });
};

var FilterProduct = (param) => {
    preloader.load();
    $('#divMainRender').html('<div class="row"><div class="col-md-12"><div id="renderProducts"></div></div><div class="col-md-12"><div id="divTrending"></div></div><div class="col-md-12"><div id="divNew"></div></div></div>')
    $.post('/FilteredProduct', param).done(result => {
       // console.log(param);
        $('#renderProducts').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        preloader.remove();
        $('.info_shopping_icon').unbind().mouseover(function () {
            shoppingMoouseOver(this);
        });
    });
};

var loadProductTrending = (CategoryID, subCategoryID1, subCategoryID2) => {
    let param = {
        CategoryID: CategoryID,
        SubCategoryID1: subCategoryID1,
        SubCategoryID2: subCategoryID2
    };
    $.post('_ProductTrending', param).done(result => {
        preloader.load();
        $('#divTrending').append(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        preloader.remove();
        $('.info_shopping_icon').unbind().mouseover(function () {
            shoppingMoouseOver(this);
        });
    });
};

var loadProductNewArrival = (CategoryID, subCategoryID1, subCategoryID2) => {
    let param = {
        CategoryID: CategoryID,
        SubCategoryID1: subCategoryID1,
        SubCategoryID2: subCategoryID2
    };
    $.post('_ProductNewArrival', param).done(result => {
        preloader.load();
        $('#divNew').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        preloader.remove();
        $('.info_shopping_icon').unbind().mouseover(function () {
            shoppingMoouseOver(this);
        });
    });
};

var GetFilters = (CID, SID1, SID2,Filters) => {
    $.post('/List-Filter-Option', { CID: CID, sid: SID1, sid2: SID2, filters: Filters, IsListForm: true })
        .done(result => {
            $('.vm-menu').click();
            $('#FilterGrid').remove();
            $('.content').after(`<div class="" id="FilterGrid" data-category-Id="${CID}" data-subcategory1-Id="${SID1}" data-subcategory2-Id="${SID2}"></div>`);
            $('#FilterGrid').html(result);
        }).fail(xhr => {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            anEC.alert(anEC.type.failed);
        }).always(() => preloader.remove());
};

var ItemInCart = () => {
    $.post('/ItemCountinCart')
        .done(result => {
            if (result.statuscode == anEC.type.success)
                $('#btnCartDeatil>sup').text(result.tQuantity);
        })
        .fail(xhr => {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            anEC.alert(anEC.type.failed);
        });
};

var ProductDetailForUser = e => {
    preloader.load();
    $.post('/ProductDetailForUser', { PID: $(e.currentTarget).data().productId, PdetailId: $(e.currentTarget).data().productdetailId })
        .done(result => {
            mdlA.id = "ProductDetailModal";
            mdlA.title = "Detail";
            mdlA.content = result;
            mdlA.modal(mdlA.size.xxlarge);
        })
        .fail(xhr => {
            anEC.title = 'Oops';
            anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
            anEC.alert(-1);
        }).always(() => preloader.remove());
};

var CartDetail = () => {
    preloader.load();
    $.post('/CartDetail').done(result => {
        mdlA.title = 'Cart Detail';
        mdlA.id = 'CartDetail';
        mdlA.content = result;
        mdlA.modal(mdlA.size.large);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        anEC.alert(anEC.type.failed);
    }).always(() => preloader.remove());
};

var AddToCart = (pdid, qty) => {
    $.post('/ECommAddToCart', { ProductDetailID: pdid, Quantity: qty }).done(result => {
        CartCount();
        anEC.title = result.statuscode == 1 ? 'Success' : "Oops";
        anEC.content = result.msg;
        anEC.alert(result.statuscode);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var CartCount = () => {
    $.post('/CartCount').done(result => {
        $('#ECommCart .counter').html('<span class="total-mini-cart-item">' + result.tQuantity + '</span>');
        $('#ECommCartSticky .counter').html('<span class="total-mini-cart-item">' + result.tQuantity + '</span>');
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var OnCartQuantityChange = (_this, qty) => {
    var _eleTr = $(_this).closest('tr');
    var _sp = _eleTr.find('span.price.sellPrice')[0].innerHTML;
    var input_ = _eleTr.find('#qty');
    $.post('/QuantityChange', { PdId: _eleTr.data().pdid, Quantity: qty }).done(result => {
        CartCount();
        if (result.statuscode == 1) {
            input_.val(qty);
            _eleTr.find('span#lblSubTotal.price')[0].innerHTML = parseFloat(_sp) * qty;
        }
        else {
            anEC.title = "Oops";
            anEC.content = result.msg;
            anEC.alert(result.statuscode);
            input_.val(result.commonStr);
            _eleTr.find('span#lblSubTotal.price')[0].innerHTML = parseFloat(_sp) * parseFloat(result.commonStr);
        }
        CartSummary();
        //anEC.title = result.statuscode == 1 ? 'Success' : "Oops";
        //anEC.content = result.msg;
        //anEC.alert(result.statuscode);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var CartSummary = () => {
    let totalMrp = 0.00;
    let totalDiscount = 0.00;
    let totalPrice = 0.00;
    let _tr = $('tbody tr.item-info');
    $.each(_tr, function (i, item) {
        let _mrp = $(item).find('span.price.mrp')[0].innerHTML;
        let _discount = $(item).find('span.price.discount')[0].innerHTML;
        let _qty = $(item).find('#qty').val();
        totalMrp = totalMrp + (_mrp * _qty);
        totalDiscount = totalDiscount + (_discount * _qty);
        totalPrice = totalPrice + ((_mrp - _discount) * _qty);
    });
    $('tbody.summary td span.price.totalMrp')[0].innerHTML = totalMrp;
    $('tbody.summary td span.price.totalDisc')[0].innerHTML = totalDiscount;
    $('tbody.summary td span.price.totalAmt')[0].innerHTML = totalPrice;
};

var AddToWishlist = (pdid) => {
    $.post('/ECommWishList', { ProductDetailID: pdid }).done(result => {
        WishlistCount();
        anEC.title = result.statuscode == 1 ? 'Success' : "Oops";
        anEC.content = result.msg;
        anEC.alert(result.statuscode);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var WishlistCount = () => {
    $.post('/WishListCount').done(result => {
        $('#ECommWishList .counter').html('(' + result.tQuantity + ')');
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var WishlistDetail = () => {
    preloader.load();
    $.post('/GetECommWishlist').done(result => {
        mdlE.id = 'mdlWishlist';
        mdlE.show('mdlWishlist', 'modal-lg', 'My Wishlist', result);
        $('button.close').click(() => {
            mdlE.dispose();
        });
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        anEC.alert(anEC.type.failed);
    }).always(() => preloader.remove());
};

var Quickview = (pdid) => {
    $.post('/Quickview/' + pdid).done(result => {
        debugger;
        mdlE.id = 'mdlQuickview';
        mdlE.show('mdlQuickview', 'modal-xxl', 'Quickview', result);
        $('button.close').click(() => {
            mdlE.dispose();
        });
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var OnFilterChange = (pid, pdid, filter) => {
    let param = {
        ProductId: pid,
        ProductDetailId: pdid,
        FilterIds: filter
    };
    $.post('/OnFilterChange', param).done(result => {
        if (result.statuscode == 1) {
            location.href = '/Shop/Product/' + result.productDetailID;
        }
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        // preloader.remove();
    });
};

var ProceedToPay = () => {
    //preloader.load();
    $.post('/ECProceedToPay').done(result => {
        if (result.statuscode != undefined && result.statuscode != 1) {
            //location.href = "/Shopping";
            anEC.title = "Oops";
            anEC.content = result.msg;
            anEC.alert(anEC.type.failed);
            if (result.statuscode == -1) {
                ECommLogin('/ECommCart');
            }
        }
        else {
            mdlE.id = 'mdlPay';
            mdlE.show('mdlPay', 'modal-lg', 'Proceed To Pay', result);
            $('button.close').click(() => {
                mdlE.dispose();
            });

            //$.when(ECommLogin()).done(function () {
            //    location.href = "/Shopping";
            //});
        }
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
        anEC.alert(anEC.type.failed);
    }).always(() => preloader.remove());
};

var BindBanners = () => {
    jQuery.post('/GetECommBanners').done(result => {
        //preloader.load();
        $('#divBanners').html(result);
    }).fail(xhr => {
        anEC.title = 'Oops';
        anEC.content = xhr.status === 0 ? "Check your internet connection" : "Server Error";
        anEC.alert(-1);
    }).always(function () {
        //preloader.remove();
        //$('.info_shopping_icon').unbind().mouseover(function () {
        //    shoppingMoouseOver(this);
        //});
    });
};
$(document).ready(() => {
    GetUserInfo();
    BindMenu();
    
});


