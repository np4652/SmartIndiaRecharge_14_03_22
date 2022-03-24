class product {
    IsPageReload = false;
    Option = [];
    newFileList = [];
    constructor() {
        let urlArray = window.location.href.split('/');

        if (!isNaN(urlArray[urlArray.length - 1])) {
            this.bindFilters();
            this.preloadedImages(urlArray[urlArray.length - 1]);
            this.IsPageReload = true
        }

        $('#ddlCategory').focus();

        $('#ddlCategory,#ddlVendor,#ddlLevel1,#ddlLevel2,#ddlProduct,#ddlBrand').select2();

        $('#txtCost,#txtDiscount,#txtCommission').numeric({
            numericType: 'decimal',
            maxLength: 10
        });

        $('#txtQuantity').numeric({
            numericType: 'number',
            maxLength: 5
        });
        Q.initEditor();

        $('#btnNew').click(() => {
            preloader.load();
            $.post('/Add-MasterProduct').done(result => {
                mdlA.id = "addMasterProduct";
                mdlA.title = "Add Master Product";
                mdlA.content = result;
                mdlA.modal(mdlA.size.large);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
        });

        $('#ddlCategory').change(e => { bindCategoryLevel1(e, 'ddlLevel1'); this.bindBrand(e, 'ddlBrand'); });

        $('#ddlLevel1').change(e => bindCategoryLevel2(e, 'ddlLevel2'));

        $('#ddlLevel2').change(() => {
            this.bindProduct(); this.bindFilters();
        });

        $('#btnAddToList').click(() => {
            if ($.FormValidation.IsFormValid()) {
                if (this.newFileList.length <= 0) {
                    this.validateAlert('No Image choosen yet.Please choose product Image');
                    $('#fileImage').click();
                    return false
                }
                //if ($('#tblFilter').index() === -1) {
                //    this.validateAlert('Please select all mendetory dropdowns');
                //    return false;
                //}
                let validFilter = false, arr = [], filiterCount = 0;
                $('#tblFilter tbody tr').each(function (i) {
                    filiterCount = i + 1;
                    if ($(this).find('span.active').data() !== undefined) {
                        if ($(this).attr('class') !== undefined && $(this).attr('class').indexOf('bg-red') > -1) {
                            $(this).removeClass('bg-red');
                        }
                        validFilter = true;
                        arr.push($(this).find('span.active').data().optionId);
                    }
                })
                if (filiterCount === $('#tblFilter tbody tr').length) {//if (validFilter) {
                    this.Option = arr;
                    pro.saveProduct();
                }
            }
        });

        $('#btnReset').click(() => this.resetAll());

        $('#fileImage').change(e => this.previewImage(e, 'imagePreview'));

        $('.DiscountType button').click(e => {
            console.log(e);
            if ($(e.currentTarget).parents('#divB2C').length > 0) {
                $('#divB2C .DiscountType button').removeClass('btn-dark active').addClass('btn-outline-dark');
                $(e.currentTarget).removeClass('btn-outline-dark').addClass('btn-dark active');
                OnSellingPrice({ id: 'B2C' });
            }
            if ($(e.currentTarget).parents('#divB2B').length > 0) {
                $('#divB2B .DiscountType button').removeClass('btn-dark active').addClass('btn-outline-dark');
                $(e.currentTarget).removeClass('btn-outline-dark').addClass('btn-dark active');
                OnSellingPrice({ id: 'B2B' });
            }
        });

        $('.CommType button').click(e => {
            if ($(e.currentTarget).parents('#divB2C').length > 0) {
                $('#divB2C .CommType button').removeClass('btn-dark active').addClass('btn-outline-dark');
                $(e.currentTarget).removeClass('btn-outline-dark').addClass('btn-dark active')
                OnSellingPrice({ id: 'B2C' });
            }
            if ($(e.currentTarget).parents('#divB2B').length > 0) {
                $('#divB2B .CommType button').removeClass('btn-dark active').addClass('btn-outline-dark');
                $(e.currentTarget).removeClass('btn-outline-dark').addClass('btn-dark active')
                OnSellingPrice({ id: 'B2B' });
            }
        });
    }

    bindBrand(e, _id) {
        $.post('/List-Brand', { cid: $(e.currentTarget).val() })
            .done(result => $('#' + _id).empty().append(`<option value="0">:: Choose Brand ::</option>`).append(result.map(m => `<option value="${m.brandId}">${m.brandName}</option>`)).select2())
            .fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    }

    bindProduct() {
        preloader.load();
        let param = {
            CategoryID: $('#ddlCategory').val(),
            SubCategoryID1: $('#ddlLevel1').val(),
            SubCategoryID2: $('#ddlLevel2').val(),
        };
        $.post('/List-MasterProduct', param)
            .done(result => $('#ddlProduct').empty().append(`<option>:: Choose Product ::</option>`).append(result.map(m => `<option value="${m.productID}">${m.productName}</option>`)).select2()).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    saveProduct() {
        let param = {
            ProductDetailID: $('#hfdPDID').val(),
            ProductID: $('#ddlProduct').val(),
            BrandID: $('#ddlBrand').val(),
            VendorID: $('#ddlVendor').val(),
            MRP: $('#txtCost').val(),
            Discount: $('#txtDiscount').val(),
            DiscountType: $('#divB2C .DiscountType button.active').data()?.discountType,
            SellingPrice: $('#txtSellingPrice').val(),
            VendorPrice: $('#txtVendorPrice').val(),
            AdminProfit: $('#txtAdminCommission').val(),
            Commission: $('#txtCommission').val(),
            CommissionType: $('#divB2C .CommType button.active').data()?.commType,
            ProductCode: $('#txtProductCode').val(),
            Batch: $('#txtBatch').val(),
            Quantity: $('#txtQuantity').val(),
            FilterWithOption: this.Option,
            ShippingCharges: $('#txtShippingCharges').val(),
            ShippingMode: $('#selShippingMode').val(),
            Description: tinyMCE.get('txtDescription').getContent(),
            Specification: tinyMCE.get('txtSpecification').getContent(),
            DiscountCount: $('#txtDiscountCount').val() === '' ? 0 : $('#txtDiscountCount').val(),
            ShippingDiscount: $('#txtshippingDiscount').val() === '' ? 0 : $('#txtshippingDiscount').val(),
            WeightInKG: $('#txtWeight').val() === '' ? 0 : $('#txtWeight').val(),
            ReturnApplicable: $('#txtReturnApplicable').val() === '' ? 0 : $('#txtReturnApplicable').val(),
            IsTrending: $('#chkIsTrending').is(':checked'),
            B2BSellingPrice: $('#txtB2BSellingPrice').val(),
            B2BVendorPrice: $('#txtB2BVendorPrice').val(),
            B2BAdminProfit: $('#txtB2BAdminCommission').val(),
            B2BDiscount: $('#txtB2BDiscount').val(),
            B2BDiscountType: $('#divB2B').val() == undefined ? 0 : $('#divB2B .DiscountType button.active').data()?.discountType,
            B2BCommission: $('#txtB2BCommission').val(),
            B2BShippingCharges: $('#txtB2BShippingCharges').val(),
            B2BCommissionType: $('#divB2B').val() == undefined ? 0 : $('#divB2B .CommType button.active').data()?.commType,
            B2BShippingMode: $('#selB2BShippingMode').val(),
            B2BDiscountCount: $('#txtB2BDiscountCount').val(),
            B2BshippingDiscount: $('#txtB2BshippingDiscount').val(),
            AdditionalTitle: $('#txtAdTitle').val()
        },
            formData = new FormData();
        for (let i = 0; i < this.newFileList.length; i++) {
            formData.append('file', this.newFileList[i]);
        }
        formData.append('ImgName', $(".spnColor.active").data() !== undefined ? $(".spnColor.active").data().optionCode : '#000');
        formData.append('productDetail', JSON.stringify(param));
        $.ajax({
            type: 'POST',
            url: '/upload-ProductImage',
            processData: false,
            contentType: false,
            data: formData,
            success: result => {
                an.title = result.statuscode === an.type.success ? 'Well done' : 'Oops';
                an.content = result.msg;
                an.alert(result.statuscode);
                if (result.statuscode === an.type.success) {
                    $('span').removeClass('active');
                    $('#imagePreview').empty();
                    this.newFileList = [];
                    if (this.IsPageReload) {
                        window.location.href = '/AddProduct';
                    }
                }
            },
            error: xhr => {
                console.log(xhr);
                an.title = 'Oops';
                an.content = xhr.status === 404 ? "Requested path not find" : (xhr.status === 0 ? "Internet is not connected" : "Server error");
                an.alert(an.type.failed);
            }
        });
    };

    previewImage(e, divId = '') {
        try {
            let selectElement = $(e.currentTarget).attr('id'),
                count = $('#imagePreview img').length,
                totalFile = document.getElementById(selectElement).files.length;
            for (var i = 0; i < totalFile; i++) {
                this.newFileList.push($(`#${selectElement}`)[0].files[i]);
                let _src = URL.createObjectURL(event.target.files[i]);
                if (divId === '') {
                    $(`#${selectElement}`).parent('div').append(`<div class="previewImage float-left text-center cus-img">
                                                                    <span class="setDefault">
                                                                        <input type="checkbox" ${count === 0 && i === 0 ? 'checked="checked"' : ''} class="chkDefaultImg" onclick="pro.setDefaultIcon($(this))" data-toggle="tooltip" data-placement="top" data-original-title="Set as defult icon"/>
                                                                    </span>
                                                                    <span class="close fa fa-times"></span>
                                                                    <img src="${_src}" data-index-Id="${count === 0 ? i : count}" style="width: 150px!important;">
                                                                 </div>`);
                }
                else {
                    $(`#${divId}`).append(`<div class="previewImage float-left text-center cus-img">
                                             <span class="setDefault">
                                                <input type="checkbox" ${count === 0 && i === 0 ? 'checked="checked"' : ''} class="chkDefaultImg" onclick="pro.setDefaultIcon($(this))" data-toggle="tooltip" data-placement="top" data-original-title="Set as defult icon"/>
                                              </span>
                                             </span>
                                             <span class="close fa fa-times"></span>
                                             <img src="${_src}" data-index-Id="${count === 0 ? i : count}" style="width: 150px!important;">
                                           </div>`);
                }
                $('[data-toggle="tooltip"]').tooltip();
                this.deleteImage();
            }
            $(`#${selectElement}`).val('');
        }
        catch (xhr) {
            console.log(xhr)
        }
    };

    setDefaultIcon(sender) {
        event.preventDefault();
        $('input.chkDefaultImg').prop('checked', false);
        sender.prop('checked', true);
        $(sender.closest('div.previewImage').clone()).insertBefore($('div.previewImage:first'));
        sender.closest('div.previewImage').remove();
        let Defultfile = this.newFileList[$(sender).closest('div.previewImage').find('img').data().indexId];
        let arr = this.newFileList;
        this.newFileList.splice([$(sender).closest('div.previewImage').find('img').data().indexId], 1);
        this.newFileList = [];
        this.newFileList.push(Defultfile);
        for (let i = 0; i < arr.length; i++) {
            this.newFileList[i + 1] = arr[i];
        }
        $('div.previewImage>img').each(function (i) {
            $(this).attr('data-index-id', i);
        });
        $('.previewImage > span.close').unbind().click(e => {
            $(e.currentTarget).parent('div.previewImage').remove();
            this.newFileList.pop($(e.currentTarget).data().indexId);
            $('div.previewImage>img').each(function (i) {
                $(this).attr('data-index-id', i);
            });
        });
    }

    validateAlert(result, id) {
        an.id = "vAlert";
        an.content = result;
        an.title = "Warning:";
        an.alert(an.type.failed);
        $('#' + id).focus();
    };

    bindFilters() {
        $.post('/List-Filter-Option', { CID: $('#ddlCategory').val(), sid: $('#ddlLevel1').val() , sid2: $('#ddlLevel2').val() })/*$('#ddlLevel2').val() })*/
            .done(result => {
                $('#filterSection').html(result);
            }).fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    };

    preloadedImages(ProductDetailID) {
        $.post('/getImageOptionWise', { ProductID: $('#ddlProduct').val(), ProductDetailID: ProductDetailID })
            .done(result => {
                let files = [];
                for (let i = 0; i < result.length; i++) {
                    this.ToFile(`/Image/Products/${result[i].replace("-1x", "")}`, function (f) {
                        files.push(f);
                    });
                    $('#imagePreview').append(`<div class="previewImage float-left text-center">
                                             <span class="setDefault">
                                                <input type="checkbox" ${result.length === 0 && i === 0 ? 'checked="checked"' : ''} class="chkDefaultImg" onclick="pro.setDefaultIcon($(this))" data-toggle="tooltip" data-placement="top" data-original-title="Set as defult icon"/>
                                              </span>
                                             </span>
                                             <span class="close fa fa-times"></span>
                                             <img src="/Image/Products/${result[i].replace("-1x", "")}" data-index-Id="${result.length === 0 ? i : result.length}" style="width: 150px!important;">
                                           </div>`);
                }
                this.newFileList = files;
                this.deleteImage();
            })
            .fail(xhr => {
                an.title = 'Oops';
                an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
                an.alert(an.type.failed);
            }).always(() => preloader.remove());
    }

    resetAll() {
        $('#ddlCategory').focus();
        $("select").prop('selectedIndex', 0).change();
        $('input').val('');
        $('#filterSection,#imagePreview').html('');
        tinyMCE.activeEditor.setContent('');
        this.newFileList = [];
    };

    ToFile(filePath, callback) {
        var file;
        var xhr = new XMLHttpRequest();
        xhr.open('GET', filePath, true);
        xhr.onload = function (e) {
            if (this.status === 200) {
                file = new File([this.response], 'fileName.png');
                callback(file);
            }
        };
        xhr.send();
    }

    deleteImage = () => {
        $('.previewImage > span.close').unbind().click(e => {
            let _this = $(e.currentTarget).parents('.previewImage');
            let ImageURL = $(e.currentTarget).parents('.previewImage').find('img').attr('src');
            if (ImageURL.indexOf('blob') > -1) {
                $(e.currentTarget).parent('div.previewImage').remove();
                this.newFileList.pop($(e.currentTarget).data().indexId);
                $('div.previewImage>img').each(function (i) {
                    $(this).attr('data-index-id', i);
                });
            }
            else {
                preloader.load();
                $.post('/deleteProductImage', { ImagePath: ImageURL }).done((result) => {
                    if (result.statuscode === an.type.success) {
                        $(_this).remove()
                    }
                    else {
                        validateAlert(result.msg)
                    }
                }).fail(xhr => console.log(xhr)).always(() => preloader.remove());
            }
        });
    }
}

function CalculateSellingpricebyMRP(e) {
    var MRP = $('#txtCost').val();
    $('#txtB2BCost').val(MRP);
    OnSellingPrice(e);
}

function CalculateSellingpricebyDiscount(e) {
    OnSellingPrice(e);
}

function CalculateVendorpricebyadmincomm(e) {
    let minCommType = $('#ddlLevel2').find(':selected').data().commType;
    let minComm = $('#ddlLevel2').find(':selected').data().minComm;
    if (e.id == "txtCommission") {
        var SellingPrice = $('#txtSellingPrice').val();
        let minCommAmt = (minCommType == 0 ? minComm : minComm * 0.01 * SellingPrice);
        var AdminCom = $('#txtCommission').val();
        var AdminComType = $('#divB2C .CommType button.active').data().commType;
        var AdminCommAmt = (AdminComType == 0 ? AdminCom : AdminCom / 100 * SellingPrice);
        if (minCommAmt > AdminCommAmt) {
            $('#txtVendorPrice').val(0);
            $('#txtAdminCommission').val(0);
            pro.validateAlert("Admin Commission can not be less than &#8377;" + minCommAmt.toString());
        }
        else {
            OnSellingPrice(e);
        }
    }
    else {
        var SellingPrice = $('#txtB2BSellingPrice').val();
        let minCommAmt = (minCommType == 0 ? minComm : minComm * 0.01 * SellingPrice);
        var AdminCom = $('#txtB2BCommission').val();
        var AdminComType = $('#divB2B .CommType button.active').data().commType;
        var AdminCommAmt = (AdminComType == 0 ? AdminCom : AdminCom / 100 * SellingPrice);
        if (minCommAmt > AdminCommAmt) {
            $('#txtB2BVendorPrice').val(0);
            $('#txtB2BAdminCommission').val(0);
            pro.validateAlert("Admin Commission can not be less than &#8377;" + minCommAmt.toString());
        }
        else {
            OnSellingPrice(e);
        }
    }
}

function OnSellingPrice(e) {
   // console.log(e);
    //debugger;
    var mrp = $('#txtCost').val();
    $('#txtB2BCost').val(mrp);
    if (e.id.includes("B2B")) {
        var Discount = $('#txtB2BDiscount').val();
        var DiscountType = $('#divB2B .DiscountType button.active').data().discountType;
        var DiscountPrice = (DiscountType == "1" ? Discount / 100 * mrp : Discount);
        var SellingPrice = mrp - DiscountPrice;
        var AdminCom = $('#txtB2BCommission').val();
        var AdminComType = $('#divB2B .CommType button.active').data().commType;
        var AdminComm = (AdminComType == "1" ? AdminCom / 100 * SellingPrice : AdminCom);
        var VendorPrice = SellingPrice - AdminComm;
        $('#txtB2BSellingPrice').val(Math.round(SellingPrice * 100) / 100);
        $('#txtB2BAdminCommission').val(Math.round(AdminComm * 100) / 100);
        $('#txtB2BVendorPrice').val(Math.round(VendorPrice * 100) / 100);
    }
    else {
        var Discount = $('#txtDiscount').val();
        var DiscountType = $('#divB2C .DiscountType button.active').data().discountType;
        var DiscountPrice = (DiscountType == "1" ? Discount / 100 * mrp : Discount);
        var SellingPrice = mrp - DiscountPrice;
        var AdminCom = $('#txtCommission').val();
        var AdminComType = $('#divB2C .CommType button.active').data().commType;
        var AdminComm = (AdminComType == "1" ? AdminCom / 100 * SellingPrice : AdminCom);
        var VendorPrice = SellingPrice - AdminComm;
        $('#txtSellingPrice').val(Math.round(SellingPrice * 100) / 100);
        $('#txtAdminCommission').val(Math.round(AdminComm * 100) / 100);
        $('#txtVendorPrice').val(Math.round(VendorPrice * 100) / 100);
    }
}

function OnSameRateCheck(e) {
    console.log(e);
    console.log(e.id);
    if ($('#' + e.id).is(':checked')) {
        $('#txtB2BDiscount').prop('disabled', 'disabled');
        $('#txtB2BSellingPrice').prop('disabled', 'disabled');
        $('#txtB2BCommission').prop('disabled', 'disabled');
        $('#txtB2BAdminCommission').prop('disabled', 'disabled');
        $('#txtB2BVendorPrice').prop('disabled', 'disabled');
        $('#selB2BShippingMode').prop('disabled', 'disabled');
        $('#txtB2BShippingCharges').prop('disabled', 'disabled');
        $('#txtB2BDiscountCount').prop('disabled', 'disabled');
        $('#txtB2BshippingDiscount').prop('disabled', 'disabled');
        $('#txtB2BCost').val($('#txtCost').val());
        $('#txtB2BDiscount').val($('#txtDiscount').val());
        $('#txtB2BSellingPrice').val($('#txtSellingPrice').val());
        $('#txtB2BCommission').val($('#txtCommission').val());
        $('#txtB2BAdminCommission').val($('#txtAdminCommission').val());
        $('#txtB2BVendorPrice').val($('#txtVendorPrice').val());
        $('#selB2BShippingMode').val($('#selShippingMode').val());
        $('#txtB2BShippingCharges').val($('#txtShippingCharges').val());
        $('#txtB2BDiscountCount').val($('#txtDiscountCount').val());
        $('#txtB2BshippingDiscount').val($('#txtshippingDiscount').val());
        $('#divB2B .CommType button').removeClass('btn-dark active').addClass('btn-outline-dark');
        $('#divB2B .DiscountType button').removeClass('btn-dark active').addClass('btn-outline-dark');
        let disType = $('#divB2C .DiscountType button.active').data().discountType;
        let commType = $('#divB2C .CommType button.active').data().commType;
        if (disType == 1) {
            $('#btnDT1').removeClass('btn-outline-dark').addClass('btn-dark active');
        }
        else {
            $('#btnDT0').removeClass('btn-outline-dark').addClass('btn-dark active');
        }
        if (commType == 1) {
            $('#btnCT1').removeClass('btn-outline-dark').addClass('btn-dark active');
        }
        else {
            $('#btnCT0').removeClass('btn-outline-dark').addClass('btn-dark active');
        }
        $('#divB2B .DiscountType button.active').data().discountType
    }
    else {
        $('#txtB2BDiscount').removeAttr('disabled');
        $('#txtB2BSellingPrice').removeAttr('disabled');
        $('#txtB2BCommission').removeAttr('disabled');
        $('#txtB2BAdminCommission').removeAttr('disabled');
        $('#txtB2BVendorPrice').removeAttr('disabled');
        $('#selB2BShippingMode').removeAttr('disabled');
        $('#txtB2BShippingCharges').removeAttr('disabled');
        $('#txtB2BDiscountCount').removeAttr('disabled');
        $('#txtB2BshippingDiscount').removeAttr('disabled');
    }
}

let pro = new product();