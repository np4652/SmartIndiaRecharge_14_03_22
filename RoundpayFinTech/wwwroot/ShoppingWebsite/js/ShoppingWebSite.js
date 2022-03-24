
$(document).ready(function () {
    HeaderMenu();
    ShoppingFooter();
    ProfileInfo();
    CartDetails();
    an.autoClose = 3;

    
});
$("#dvdash a").click(function () {
    var type = $(this).attr("data-id");
    if (type == "W") {
        getallwishlist();
    }
    else if (type == "O") {
        getOrders();
    }
});

var preloader = {
    load: () => $('body').append(`<div id="img_Loader" class="loadingLarge"><img src="/ShoppingWebsite/images/loader/shopping_loader.gif"/></div>`),
    remove: () => $('.loadingLarge').remove()
};
var preloaderSmall = {
    load: () => $('body').append(`<div id="img_Loader" class="loadingSmall"><img src="/ShoppingWebsite/images/loader/loader_small.gif"/></div>`),
    remove: () => $('.loadingSmall').remove()
};

var HeaderMenu = () => {
    preloader.load();
    $.post('/HeaderMenu', {})
        .done(function (result) {
            $('#HeaderMenu').html(result);
            preloader.remove();
        }).fail(xhr => console.log(xhr.responseText))
}
var ShoppingFooter = () => {
    preloader.load();
    $.post('/ShoppingFooter', {})
        .done(function (result) {
            $('#footer').html(result);
            preloader.remove();
        }).fail(xhr => console.log(xhr.responseText))
}
var CartDetails = () => {
    preloader.load();
    $.post('/CartDetails', {})
        .done(function (result) {
            $('#cartDetails').html(result);
            preloader.remove();
        }).fail(xhr => console.log(xhr.responseText))
}
var ProfileInfo = () => {
    preloader.load();
    $.post('/ProfileInfo', {})
        .done(function (result) {
            $('#ProfileInfo').html(result);
            preloader.remove();
        }).fail(xhr => console.log(xhr.responseText))

}
var removeitemfromcart = (id, productdetailid, removeall = false) => {
    preloaderSmall.load();
    $.post('/removecart', { ID: id, ProductDetailID: productdetailid, RemoveAll: removeall })
        .done(function (result) {
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
            ShoppinCartDetails();
            preloaderSmall.remove();
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}
var AddWishListProduct = (productdetailid) => {
    preloaderSmall.load();
    $.post('/addwishlist', { ProductDetailID: productdetailid })
        .done(function (result) {
            preloaderSmall.remove();
            if (result.statuscode == -2) {
                window.location.href = result.msg;
                return;
            }
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}
var loadProduct = (webSiteID = 1, mainCategory, categoryId = 0, selector = '0') => {
    selector = selector == '0' ? "#allAcc" + mainCategory : selector;
    $.post('/CategoryProduct', { wsid: webSiteID, mcatid: mainCategory, catid: categoryId })
        .done(function (result) {
            $(selector).html(result);
        })
}

var WishListToCart = (productdetailid, id) => {
    preloaderSmall.load();
    $.post('/WishListToCart', { ID: id, posdetid: productdetailid })
        .done(function (result) {
            preloaderSmall.remove();
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
            getallwishlist();
            CartDetails();
        }).catch(function (xhr, e, msg) {
            reject(xhr);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })

}


var AddToCart = (productid, productdetailid, quantity) => {
    preloaderSmall.load();
    $.post('/AddToCartProd', { posid: productid, posdetid: productdetailid, Quantity: quantity })
        .done(function (result) {
            preloaderSmall.remove();
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
            CartDetails();
        }).catch(function (xhr, e, msg) {
            reject(xhr);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })

}
var ChangeQuantity = (productdetailid, quantity, type) => {
    preloaderSmall.load();
    if (quantity > 5) {
        an.title = "Message";
        an.content = "Max 5 Quntity Allowed";
        an.alert(-1);
        return;
    }
    if (quantity == 0 || quantity == '') {
        an.title = "Message";
        an.content = "Invalid Quntity";
        an.alert(-1);
        return;
    }
    $.post('/ChangeItemQunatity', { ProductDetailID: productdetailid, Quantity: quantity, Type: type })
        .done(function (result) {
            preloaderSmall.remove();
            ShoppinCartDetails();
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
        })

}
var ShoppinCartDetails = () => {
    preloaderSmall.load();
    $.post('/shoppingcart', {})
        .done(function (result) {
            $('#ShoppingCart').html(result);
            preloaderSmall.remove();
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}
var BuyNow = (productdetailid) => {
    let quantity = $('#txtquannum').val();
    if (quantity == null || quantity == '' || quantity < 1) {
        quantity = 1;
    }
    preloaderSmall.load();
    $.post('/buynow', { ProductDetailID: productdetailid, Quantity: quantity })
        .done(function (result) {
            preloaderSmall.remove();
            if (result.statuscode == -2) {
                window.location.href = result.msg;
                return;
            }
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
            if (result.statuscode == 1) {
                window.location.href = "/shoppingcart";
            }
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}

var quickView = (webSiteID = "1", posid) => {
    var classs = ".loadersmall" + posid;
    $(classs).show();
    $.post('/QuickView', { wsid: webSiteID, PosID: posid })
        .done(function (result) {
            $(classs).hide();
            mdlA.id = "Quickwin";
            mdlA.title = "Quick View";
            mdlA.content = result;
            mdlA.modal(mdlA.size.large);

        })
}
var getallwishlist = (type, filterlist = "") => {
    preloaderSmall.load();
    $.post('/WishList', {})
        .done(function (result) {
            preloaderSmall.remove();
            $('#MyDetails').html(result);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}

var getOrders = (type, filterlist = "") => {
    preloaderSmall.load();
    $.post('/OrderReport', {})
        .done(function (result) {
            $('#MyDetails').html(result);
            preloaderSmall.remove();
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}

var removeItemFromWishList = (POSId, id, removeall = false) => {
    preloaderSmall.load();
    $.post('/removewishlist', { POSId: POSId, ID: id, RemoveAll: removeall })
        .done(function (result) {
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
            getallwishlist();
            preloaderSmall.remove();
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
}