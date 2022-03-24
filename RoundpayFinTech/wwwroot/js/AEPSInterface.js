var StatusText = { PENDING: 1, SUCCESS: 2, FAILED: 3 };

(() => {
    $('#ddlBank').unbind().change(function () {
        if ($(this).val() === '0') {
            $('#opImg img').attr('src', '/Image/BankLogo/0.png')
        } else {
            var n = $('#ddlBank option:selected').text();
            $('#opImg img').attr('src', '/Image/BankLogo/' + n + '.png')
        }

    });
})();
var LocationUpdate = function () {
    $('#hidIsLoc').val("-1");
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            $('#hidLat').val(position.coords.latitude.toFixed(4))
            $('#hidLong').val(position.coords.longitude.toFixed(4))
        }, function (err) {
            $('#hidIsLoc').val(err.code);
        });

    }
}
function DeviceRegisterOpen() {
    $.post('/device-register', (resultDevice) => {
        mdlA.id = 'mdlMsgbx';
        mdlA.content = resultDevice;
        mdlA.alert(mdlA.size.large);
        $('#btnClose').click(() => mdlA.dispose());
    });
}

function DeviceRegister(id) {
    var btn = $('#saveDevice').id;
    $('#deviceSearching').html('Searching for Device...');
    $('#hfSelectedDevice').val(id);
    CaptureAvdm(btn, 'register');
}

var GetCustomDomName = '127.0.0.1';

var GetPIString = '';
var GetPAString = '';
var GetPFAString = '';
var DemoFinalString = '';

function discoverAvdm(btn) {
    GetCustomDomName = "127.0.0.1";
    $('#txtWadh').val('');
    $('#ContentPlaceHolder1_txtDeviceInfo').val('');
    $('#txtPidOptions').val('');
    $('#ContentPlaceHolder1_txtPidData').val('');
    var SuccessFlag = 0;
    var primaryUrl = "http://" + GetCustomDomName + ":";
    try {
        var protocol = window.location.href;
        if (protocol.indexOf("https") >= 0) {
            primaryUrl = "https://" + GetCustomDomName + ":";
        }
    }
    catch (e) {
        console.log(e);
    }
    url = "";
    SuccessFlag = 0;
    for (var i = 11100; i <= 11105; i++) {
        if (primaryUrl === "https://" + GetCustomDomName + ":" && OldPort === true) {
            i = "8005";
        }
        $("#lblStatus1").text("Discovering RD service on port : " + i.toString());
        var verb = "RDSERVICE";
        var res;
        $.support.cors = true;
        var httpStaus = false;
        $.ajax({
            type: verb,
            async: false,
            crossDomain: true,
            url: primaryUrl + i.toString(),
            contentType: "text/xml; charset=utf-8",
            processData: false,
            cache: false,
            success: function (data) {
                httpStaus = true;
                res = { httpStaus: httpStaus, data: data };
                finalUrl = primaryUrl + i.toString();
                var $doc = $.parseXML(data);
                var CmbData1 = $($doc).find('RDService').attr('status');
                var CmbData2 = $($doc).find('RDService').attr('info');
                MethodCapture = primaryUrl.replace('http:/', '') + i.toString() + "/capture"
                if (RegExp('\\b' + 'Mantra' + '\\b').test(CmbData2) === true) {

                    if ($($doc).find('Interface').eq(0).attr('path') === "/rd/capture") {
                        MethodCapture = $($doc).find('Interface').eq(0).attr('path');
                    }
                    if ($($doc).find('Interface').eq(1).attr('path') === "/rd/capture") {
                        MethodCapture = $($doc).find('Interface').eq(1).attr('path');
                    }
                    if ($($doc).find('Interface').eq(0).attr('path') === "/rd/info") {
                        MethodInfo = $($doc).find('Interface').eq(0).attr('path');
                    }
                    if ($($doc).find('Interface').eq(1).attr('path') === "/rd/info") {
                        MethodInfo = $($doc).find('Interface').eq(1).attr('path');
                    }

                    $("#ddlAVDM").append('<option value=' + i.toString() + '>(' + CmbData1 + '-' + i.toString() + ')' + CmbData2 + '</option>');
                    SuccessFlag = 1;
                    $('#deviceSearching').html('Place finger on device');
                }
            },
            error: function (jqXHR, ajaxOptions, thrownError) {
                if (i === "8005" && OldPort === true) {
                    OldPort = false;
                    i = "11099";
                }
            }
        });
        if (SuccessFlag === 1) {
            //break;
        }
    }
    if (SuccessFlag === 0) {
        $('#deviceSearching').html('Connection failed Please try again.');
    }
    else {
        $('#deviceSearching').html('Place finger on device');
    }
    return res;
}

function Demo() {
    var GetPIStringstr = '';
    var GetPAStringstr = '';
    var GetPFAStringstr = '';
    if (GetPI() === true) {
        GetPIStringstr = '<Pi ' + GetPIString + ' />';
    }
    else {
        GetPIString = '';
    }
    if (GetPA() === true) {
        GetPAStringstr = '<Pa ' + GetPAString + ' />';
    }
    else {
        GetPAString = '';
    }
    if (GetPFA() === true) {
        GetPFAStringstr = '<Pfa ' + GetPFAString + ' />';
    }
    else {
        GetPFAString = '';
    }
    if (GetPI() === false && GetPA() === false && GetPFA() === false) {
        DemoFinalString = '';
    }
    else {
        DemoFinalString = '<Demo>' + GetPIStringstr + ' ' + GetPAStringstr + ' ' + GetPFAStringstr + ' </Demo>';
    }
}

function GetPI() {
    var Flag = false;
    GetPIString = '';
    if ($("#drpMatchValuePI").val() > 0 && Flag) {
        Flag = true;
        GetPIString += " mv=" + "\"" + $("#drpMatchValuePI").val() + "\"";
    }

    if ($('#rdExactPI').is(':checked') && Flag) {
        Flag = true;
        GetPIString += " ms=" + "\"E\"";
    }
    else if ($('#rdPartialPI').is(':checked') && Flag) {
        Flag = true;
        GetPIString += " ms=" + "\"P\"";
    }
    else if ($('#rdFuzzyPI').is(':checked') && Flag) {
        Flag = true;
        GetPIString += " ms=" + "\"F\"";
    }
    if ($("#txtLocalNamePI").val().length > 0) {
        Flag = true;
        GetPIString += " lname=" + "\"" + $("#txtLocalNamePI").val() + "\"";
    }

    if ($("#txtLocalNamePI").val().length > 0 && $("#drpLocalMatchValuePI").val() > 0) {
        Flag = true;
        GetPIString += " lmv=" + "\"" + $("#drpLocalMatchValuePI").val() + "\"";
    }

    if ($("#drpGender").val() === "MALE") {
        Flag = true;
        GetPIString += " gender=" + "\"M\"";
    }
    else if ($("#drpGender").val() === "FEMALE") {
        Flag = true;
        GetPIString += " gender=" + "\"F\"";
    }
    else if ($("#drpGender").val() === "TRANSGENDER") {
        Flag = true;
        GetPIString += " gender=" + "\"T\"";
    }

    if ($("#txtDOB").val().length > 0) {
        Flag = true;
        GetPIString += " dob=" + "\"" + $("#txtDOB").val() + "\"";
    }

    if ($("#drpDOBType").val() !== "0") {
        Flag = true;
        GetPIString += " dobt=" + "\"" + $("#drpDOBType").val() + "\"";
    }

    if ($("#txtAge").val().length) {
        Flag = true;
        GetPIString += " age=" + "\"" + $("#txtAge").val() + "\"";
    }

    if ($("#txtPhone").val().length > 0 || $("#txtEmail").val().length > 0) {
        Flag = true;
        GetPIString += " phone=" + "\"" + $("#txtPhone").val() + "\"";
    }
    if ($("#txtEmail").val().length > 0) {
        Flag = true;
        GetPIString += " email=" + "\"" + $("#txtEmail").val() + "\"";
    }
    return Flag;
}


function GetPA() {
    var Flag = false;
    GetPAString = '';

    if ($("#txtCareOf").val().length > 0) {
        Flag = true;
        GetPAString += "co=" + "\"" + $("#txtCareOf").val() + "\"";
    }
    if ($("#txtLandMark").val().length > 0) {
        Flag = true;
        GetPAString += " lm=" + "\"" + $("#txtLandMark").val() + "\"";
    }
    if ($("#txtLocality").val().length > 0) {
        Flag = true;
        GetPAString += " loc=" + "\"" + $("#txtLocality").val() + "\"";
    }
    if ($("#txtCity").val().length > 0) {
        Flag = true;
        GetPAString += " vtc=" + "\"" + $("#txtCity").val() + "\"";
    }
    if ($("#txtDist").val().length > 0) {
        Flag = true;
        GetPAString += " dist=" + "\"" + $("#txtDist").val() + "\"";
    }
    if ($("#txtPinCode").val().length > 0) {
        Flag = true;
        GetPAString += " pc=" + "\"" + $("#txtPinCode").val() + "\"";
    }
    if ($("#txtBuilding").val().length > 0) {
        Flag = true;
        GetPAString += " house=" + "\"" + $("#txtBuilding").val() + "\"";
    }
    if ($("#txtStreet").val().length > 0) {
        Flag = true;
        GetPAString += " street=" + "\"" + $("#txtStreet").val() + "\"";
    }
    if ($("#txtPOName").val().length > 0) {
        Flag = true;
        GetPAString += " po=" + "\"" + $("#txtPOName").val() + "\"";
    }
    if ($("#txtSubDist").val().length > 0) {
        Flag = true;
        GetPAString += " subdist=" + "\"" + $("#txtSubDist").val() + "\"";
    }
    if ($("#txtState").val().length > 0) {
        Flag = true;
        GetPAString += " state=" + "\"" + $("#txtState").val() + "\"";
    }
    if ($('#rdMatchStrategyPA').is(':checked') && Flag) {
        Flag = true;
        GetPAString += " ms=" + "\"E\"";
    }
    return Flag;
}

function GetPFA() {
    var Flag = false;
    GetPFAString = '';

    if ($("#txtAddressValue").val().length > 0) {
        Flag = true;
        GetPFAString += "av=" + "\"" + $("#txtAddressValue").val() + "\"";
    }

    if ($("#drpMatchValuePFA").val() > 0 && $("#txtAddressValue").val().length > 0) {
        Flag = true;
        GetPFAString += " mv=" + "\"" + $("#drpMatchValuePFA").val() + "\"";
    }

    if ($('#rdExactPFA').is(':checked') && Flag) {
        Flag = true;
        GetPFAString += " ms=" + "\"E\"";
    }
    else if ($('#rdPartialPFA').is(':checked') && Flag) {
        Flag = true;
        GetPFAString += " ms=" + "\"P\"";
    }
    else if ($('#rdFuzzyPFA').is(':checked') && Flag) {
        Flag = true;
        GetPFAString += " ms=" + "\"F\"";
    }

    if ($("#txtLocalAddress").val().length > 0) {
        Flag = true;
        GetPFAString += " lav=" + "\"" + $("#txtLocalAddress").val() + "\"";
    }

    if ($("#drpLocalMatchValue").val() > 0 && $("#txtLocalAddress").val().length > 0) {
        Flag = true;
        GetPFAString += " lmv=" + "\"" + $("#drpLocalMatchValue").val() + "\"";
    }
    return Flag;
}

function CaptureAvdm(btn, flag) {
    if ($('#hidIsLoc').val() !== "-1" && $('#hidIsLoc').val() !== "1") {
        an.title = "Location Error! ";
        an.content = "Please allow location";
        an.alert(an.type.failed);
        LocationUpdate();
        return false;
    }
    GetCustomDomName = '127.0.0.1';
    $('#AadharHelp').addClass('d-none');
    $('#AmountHelp').addClass('d-none');
    if (flag !== 'register') {
        if ($('#txtAadhar').val().length < 12) {
            $('#AadharHelp').removeClass('d-none');
            return;
        }
        if ($('#ddlBank option').eq(0).prop('selected')) {
            alert('Please select bank');
            return;
        }
        if (flag === 'with' && $('#txtAmount').val() === "") {
            $('#AmountHelp').removeClass('d-none');
            return;
        }
    }
    var strWadh = "";
    var strOtp = "";
    Demo();
    if ($("#txtWadh").val() !== "") {
        strWadh = " wadh=\"" + $("#txtWadh").val() + '"';
    }
    if ($("#txtotp").val() !== "") {
        strOtp = " otp=\"" + $("#txtotp").val() + '"';
    }
    var XML = '<?xml version="1.0"?> <PidOptions ver="1.0"> <Opts fCount="' + $("#Fcount").val() + '" fType="' + $("#Ftype").val() + '" iCount="' + $("#Icount").val() + '" pCount="' + $("#Pcount").val() + '" pgCount="' + $("#pgCount").val() + '"' + strOtp + ' format="' + $("#Dtype").val() + '"   pidVer="' + $("#Pidver").val() + '" timeout="' + $("#Timeout").val() + '" pTimeout="' + $("#pTimeout").val() + '"' + strWadh + ' posh="UNKNOWN" env="' + $("#Env").val() + '" /> ' + DemoFinalString + '<CustOpts><Param name="mantrakey" value="' + $("#txtCK").val() + '" /></CustOpts> </PidOptions>';

    finalUrl = "http://" + GetCustomDomName + ":" + $("#ContentPlaceHolder1_ddlAVDM").val();

    try {
        var protocol = window.location.href;
        if (protocol.indexOf("https") >= 0) {
            finalUrl = "https://" + GetCustomDomName + ":" + $("#ContentPlaceHolder1_ddlAVDM").val();
        }
    } catch (e) {
        console.log(e);
    }



    var verb = "CAPTURE";
    var DeviceID = $('#hfSelectedDevice').val();
    var res;
    $.support.cors = true;
    var httpStaus = false;
    $('#' + btn).attr("disabled", "disabled");
    btnLdr.Start($('#' + btn), "Requesting...");
    setTimeout(function () {
        discoverAvdm();
        $.ajax({
            type: verb,
            async: false,
            crossDomain: true,
            url: finalUrl + MethodCapture,
            data: XML,
            contentType: "text/xml; charset=utf-8",
            processData: false,
            success: function (data) {
                DeviceID = 1;
                if (DeviceID === 1) {
                    if ($(data)[2]) {
                        var a = $(data)[2];
                        var Message = $(a).find('Resp').attr('errInfo');
                        var errCode = $(a).find('Resp').attr('errCode');
                        $('#deviceSearching').html(Message);
                        if (errCode === '0') {
                            PidData = data;
                        }
                        else {
                            alert(Message);
                            return;
                        }
                    }
                    else {
                        $('#deviceSearching').html("Device Not found");
                        return;
                    }
                }
                if (DeviceID === 2) {
                    if ($(data)[0]) {
                        a = $(data)[0];
                        Message = $(a).find('Resp').attr('errInfo');
                        errCode = $(a).find('Resp').attr('errCode');
                        $('#deviceSearching').html(Message);
                        if (errCode === '0') {
                            PidData = a;
                            data = $(a)[0].documentElement.outerHTML;
                        }
                        else {
                            alert(Message);
                            return;
                        }
                    }
                }
                httpStaus = true;
                res = { httpStaus: httpStaus, data: data };
                $('#ContentPlaceHolder1_txtPidData').val(data);
                $('#txtPidOptions').val(XML);
                a = $.parseXML(data);
                var $doc = $.parseXML(data);
                Message = $($doc).find('Resp').attr('errInfo');
                if (flag === 'register') {
                    $('#deviceSearching').html('');
                    $('#deviceInfo').show();
                    $('#deviceList').hide();
                    var res = $.parseXML(data);
                    if (!res) { res = data; }
                    var dpid = $(res).find('DeviceInfo').attr('dpId');
                    var ci = $(res).find('Skey').attr('ci');
                    $('#deviceModel').html(dpid);
                    $('#deviceID').html(ci);
                    $('#LocalDeviceID').val(DeviceID);

                    return;
                }
                if (flag === 'bal') {
                    $('#' + btn).html("Check Balance");
                    $('#' + btn).removeAttr("disabled");
                    btnLdr.Stop($("#" + btn));
                    preloader.remove();
                    CheckBalance(data);
                }
                if (flag === 'with') {
                    $('#' + btn).removeAttr("disabled");
                    btnLdr.Stop($("#" + btn));
                    preloader.remove();
                    Withdrawl(data);
                }
                else {
                    $('#' + btn).html("Check Balance");
                    $('#' + btn).removeAttr("disabled");
                }
                if (Message === "Success") {
                    $("#ContentPlaceHolder1_lblFlag").val(flag);
                    $('#static').modal('show');
                }
                else {
                    alert(Message);
                }

            },
            error: function (jqXHR, ajaxOptions, thrownError) {
                if (flag === 'bal') {
                    $('#' + btn).html("Balance Enquiry");
                    $('#' + btn).removeAttr("disabled");
                    $('#' + btn).attr("class", "btn btn-primary btn-round");
                }
                else {
                    $('#' + btn).html("Withdrawal");
                    $('#' + btn).removeAttr("disabled");
                    $('#' + btn).attr("class", "btn btn-primary btn-round");
                }
                alert(thrownError);
                res = { httpStaus: httpStaus, err: getHttpError(jqXHR) };
            },
            complete: function () {
                btnLdr.Stop($("#" + btn));
                preloader.remove();
            }
        });
        if (flag === 'bal') {
            $('#' + btn).removeAttr("disabled");
        }
        else {
            $('#' + btn).removeAttr("disabled");
        }

    }, 100);
    if (flag === 'bal') {
        $('#' + btn).removeAttr("disabled");
    }
    else {
        $('#' + btn).removeAttr("disabled");
    }
    return res;
}
var CheckBalance = function (PidData) {
    let _lat = $('#hidLat').val();
    let _long = $('#hidLong').val();
    $.post('/getAepsBalance', { PidData, aadhar: $('#txtAadhar').val(), bank: $('#ddlBank').val(), t, _lat, _long },
        function (resultaeps) {
            var aepsResp = `<div class="row">
            <div class="col-md-12">
                <button type="button" class="close" aria-label="Close"><span aria-hidden="true">×</span></button>
                <h5 class="text-info">Balance Info</h3>
                <hr />
            </div>
            <div class="col-md-12 col-sm-12 col-xs-12">
                <div class="form-group">
                    <label class="${resultaeps.statuscode === -1 ? `text-danger h4` : `text-success d-none`}"><i class="fas fa-info-circle"></i> ${resultaeps.msg} </label>
                </div>
        ${resultaeps.statuscode === 1 ? `<div class="row form-group">
                         <div class="col-sm-5">
                             <label>Available Balance</label>
                         </div>
                         <div class="col-sm-7">
                           <span style="font-size:17px"><i class="fas fa-rupee-sign text-success"></i> ${resultaeps.balance} </span>
                         </div>
                        </div>
                        <div class="row form-group">
                            <div class="col-sm-5">
                                <label>Bank RRN</label>
                            </div>
                            <div class="col-sm-7">
                                ${resultaeps.bankRRN === null ? '' : resultaeps.bankRRN}
                            </div>
                        </div>
                      </div>
                   </div>`: ``}`
            mdlA.id = 'mdlMsgbx';
            mdlA.content = aepsResp;
            mdlA.options.backdrop = 'static';
            mdlA.alert(mdlA.size.default);
            $('button.close').click(function () {
                mdlA.dispose();
            });
        })
}

function Withdrawl(PidData) {
    let _lat = $('#hidLat').val();
    let _long = $('#hidLong').val();
    var param = { PidData, aadhar: $('#txtAadhar').val(), bank: $('#ddlBank').val(), t, amount: $('#txtAmount').val(), _lat,_long };
    $.post('/AepsWithdraw', param)
        .done((resultaeps) => {
            var aepsResp = `<div class="row text-center">
                    <div class="col-md-12">
                        <button type="button" class="close" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h5 class="text-info">Transaction Info</h5><hr />
                    </div>
                    <div class="col-md-12">
                        <label class="${resultaeps.status === 3 ? `h5 text-danger` : `text-success`}">${getKeyByValue(StatusText, resultaeps.status)}</label >
                        <div class="form-group">
                            <label>${resultaeps.msg}</label>
                        </div>
                        ${resultaeps.statuscode === 1 ? `<div class="form-group">
                                                            <label>Bank Balance : ${resultaeps.balance}</label>
                                                         </div>
                                                     <div class="form-group">
                                                        <label>Transaction ID : ${resultaeps.transactionID}</label>
                                                     </div>
                                                     <div class="form-group">
                                                        <label>Live ID : ${resultaeps.liveID}</label>
                                                     </div>`: ``}
                    </div>
                </div>`;
            mdlA.id = 'mdlMsgbx';
            mdlA.content = aepsResp;
            mdlA.options.backdrop = 'static';
            mdlA.alert(mdlA.size.default);
            $('button.close').click(() => mdlA.dispose());
        });
}

function SaveDevice() {
    var data = {
        DeviceName: $('#deviceID').html(),
        ID: $('#LocalDeviceID').val()
    }
    $.post('/device-save', data, (response) => {
        console.log(response);
        if (response.status) {
            $('#deviceSaved').show();
            $('#deviceInfo').hide();
            $('#deviceList').hide();
            setTimeout(function () { $('#mdlMsgbx').modal('hide'); }, 5000)
        }
    });
}