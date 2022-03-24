$(document).ready(function () {
    GetSenderNoForService();
    $(".rotate").click(function () {
        $(this).toggleClass("down");
        $('.active').parent('a').click()
    })
    // _ContactSearch('', true);
    _load(0, '', 0).then(function () {
        _ContactSearch('', true);
    });
    $('body').on('keyup', '#txtfdconsearch', function () {
        _ForwardMessageContactSearch($('.ForwardMsgcontactSearch').val(), 0, 0);
    });

    //an.title ="Welcome";
    //an.content = "test";
    //an.alert(1); 

    //Close Send Popup Image starthere
    $('body').on('click', '.closeimg', function () {
        $(".alert").hide();
        $('#container').attr('src', '');
        $('input[type=file]').val('');

    })

    //Close Send Popup Image End here

   // lblSenderMobile

    //Refresh chat every 5 seconds start here
    function refresh() {
        var div = $('#messages'),
            divHtml = div.html();
        _load($("#hdnchatcontactid").val(), $("#lblapiid").text(), $("#lblSenderNoID").text()).then(function () {
            _ContactSearch($('#txtsearch').val(), false);
        });
        div.html();
    }
    setInterval(function () {
        var mobileno = $("#lblSenderMobile").text();
        var formData = new FormData();
        formData.append('Mobileno', mobileno);
        $.ajax({
            type: 'POST',
            url: '/WhatsappNewMessage',
            processData: false,
            contentType: false,
            data: formData,
            success: result => {
                if (result.statuscode == 1) {
                    _ContactSearch($('#txtsearch').val(), false);
                }
                else if (result.statuscode == 2) {
                    refresh();
                    _ContactSearch($('#txtsearch').val(), false);
                }
                else {
                    var number = $('.numberCircle').text();
                    if (number != '') {
                        if (parseInt(number) > 0) {
                            if ($('#hdnnewmsgid').val() != number) {
                                $('#hdnnewmsgid').val(number);
                                _ContactSearch($('#txtsearch').val(), false);
                            }
                        }
                    }
                }
            },
            error: xhr => {
                an.title = "Oops! Error";
                an.content = xhr.status == 404 ? "Requested path not find" : (xhr.status == 0 ? "Internet is not connected" : "Server error");
                an.alert(-1);
                btnLdr.Stop(element.btn);
            }
        })
    }, 10000); //5000 is 5seconds in ms
    //Refresh chat every 5 seconds End here

    //Chat contact search user contatct search Load Call start

    //Chat contact search user contatct search Load Call End

    //Chat Show Oncontact Click 
    //Chat Show Oncontact Click ends
    //$('#chkUnseenMsg').change(function () {
    //    _ContactSearch($('#txtsearch').val(), true);
    //    // $('#searchcontact').removeClass("h-100");
    //});
    //$('#chkWhatsappTasks').change(function () {
    //    _ContactSearch($('#txtsearch').val(), true);
    //    // $('#searchcontact').removeClass("h-100");
    //});

    $("#txtsearch").keyup(function () {
        _ContactSearch($('#txtsearch').val(), true);
    });
    $("#txtusercontacts").keyup(function () {
        _UserContactSearch($('#txtusercontacts').val());
    });
    $("#inputmsg").keypress(function (event) {

        var msg = $('#inputmsg').val().trim();
        var hdncopypaste = $('#baseimg').val();
        if ($("#watsapp-chat-send-file")[0].files.length === 0 && hdncopypaste == null) {
            if (msg != "") {
                if (event.keyCode == 13 && event.shiftKey) {
                    return;
                }
                else if (event.keyCode === 13) {
                    $("#btnSave").click();
                    event.preventDefault();
                }
            }
            else {
                if (event.keyCode === 13) {
                    event.preventDefault();
                }
            }
        }
        else if (event.keyCode === 13) {
            if (msg != "") {
                if (event.keyCode == 13 && event.shiftKey) {
                    return;
                }
                else if (event.keyCode === 13) {
                    $("#btnSave").click();
                    event.preventDefault();
                }
            }
            else {
                $("#btnSave").click();
                event.preventDefault();
            }
        }
    });
    $("#lnkimptusercon").click(function () {
        $('#txtusercontacts').val('');
        _UserContactSearch($('#txtusercontacts').val());
        $('.modalusercontact').modal('show');
    });
    function readURL(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#container').attr('src', e.target.result);
                $('.dvimgpreview').show();
                $('.dvvideopreview').hide();
                $('#alerts').show();
                $('#inputmsg').focus();
                //$('.modalfile').modal('show');
            }
            reader.readAsDataURL(input.files[0]);
        }
    }
    function readURLVideo(input) {

        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#videopreview').attr('src', e.target.result);
                $('.dvimgpreview').hide();
                $('.dvvideopreview').show();
                $('#inputmsg').focus();
                $('#alerts').show();
                //     $('.modalfile').modal('show');
            }
            reader.readAsDataURL(input.files[0]);
        }
    }

    $("#watsapp-chat-send-file").change(function () {
        $('.dvimgpreview').hide();
        $('.dvvideopreview').hide();
        var file_size = $('#watsapp-chat-send-file')[0].files[0].size;
        if (file_size > 16777216) {
            an.title = "Validation";
            an.content = "File size is must be less than 16 MB !";
            an.alert(-1);
            return;
        }
        var ext = $('#watsapp-chat-send-file').val().split('.').pop().toLowerCase();
        if ($.inArray(ext, ['gif', 'png', 'jpg', 'jpeg', 'mp4', 'm4v', 'gif']) == -1) {
            an.title = "Validation";
            an.content = "Invalid file upload only png,jpg,jpeg,gif,mp4,m4v file !";
            an.alert(-1);
            return;
        }
        else if (ext == 'mp4' || ext == 'm4v') {
            readURLVideo(this);
        }
        else {
            readURL(this);
        }
    });
    //send image video
    $('#btnSave').click(function () {
        let hdnvalue = $('#baseimg').val();
        let hdnapcde = $("#hdnapcde").val();
        if ($("#watsapp-chat-send-file")[0].files.length === 0 && hdnvalue == "") {
            if ($('#inputmsg').val().trim() == '') {
                return;
            }
        }
        if (hdnapcde == '') {
            return;
        }
        var snm = $('#lblSendername').text();
        var msg = $('#inputmsg').val().trim();
        var no = $('#lblSenderMobile').text();
        var Contatcno = $('#hdnchatcontactid').val();
        
        var fileupload = $("#watsapp-chat-send-file").get(0);
        var files = fileupload.files;
        var aid = $("#lblapiid").text();
        var SenderNoID = $("#lblSenderNoID").text();
        var senderno = $(".spnsenderno").text();
        //Quoted Reply param
        var conversationid = $('.quotemsgReply').attr("data-id") == 'undefined' || $('.quotemsgReply').attr("data-id") == undefined ? "" : $('.quotemsgReply').attr("data-id");
        var quotemsg = $('.quotemsgReply').attr("data-text") == 'undefined' || $('.quotemsgReply').attr("data-text") == undefined ? "" : $('.quotemsgReply').attr("data-text");
        var replyjid = $('.quotemsgReply').attr("data-waid") == 'undefined' || $('.quotemsgReply').attr("data-waid") == undefined ? "" : $('.quotemsgReply').attr("data-waid");
        //Quoted Reply param end here
        var formData = new FormData();
        formData.append("File", files[0]);
        formData.append('Text', $('#inputmsg').val().trim());
        formData.append('ContactId', no);
        formData.append('SenderName', $('#lblSendername').text().trim());
        formData.append('Screenshot', hdnvalue);
        formData.append('APICODE', hdnapcde);
        formData.append('SenderNo', $(".spnsenderno").text());
        formData.append('conversationId', conversationid);
        formData.append('QuoteMsg', quotemsg);
        formData.append('ReplyJID', replyjid);
        $('#chatbox').find('input').val('');
        $('#chatbox').find('textarea').val('');
        let _ReplyQuoted = $('.QuotedReply');
        _ReplyQuoted.empty();
        $.ajax({
            type: 'POST',
            url: '/SendWhatsappSessionMessage',
            processData: false,
            contentType: false,
            data: formData,
            success: result => {
                //hdnactcntct Used For Active Tab Contact
                $('#hdnactcntct').val('cid0');
                _ContactSearch($('#txtsearch').val(), false).then(function () {
                    _load(Contatcno, aid, SenderNoID).then(function () {
                        _load(Contatcno, aid, SenderNoID);
                    });
                });
                $("#loaderimage").hide();
            }
        })
    });
});
//#region Copy paste image to send user
document.onpaste = function (pasteEvent) {
    var item = pasteEvent.clipboardData.items[0];

    if (item.type.indexOf("image") === 0) {
        var file = item.getAsFile();

        var reader = new FileReader();
        reader.onload = function (event) {
            var x = document.getElementById("alerts")
            var y = document.getElementById("dvimgpreviewi")

            var baseimg = document.getElementById("baseimg")

            document.getElementById("baseimg").value = event.target.result;
            document.getElementById("container").src = event.target.result;
            x.style.display = "block";
            y.style.display = "block";
            document.getElementById("inputmsg").focus();

        };
        reader.readAsDataURL(file);
    }
}
//#endregion
function ChatArea(id, ad, mn, aitid, apcde, sndno, sndnoid, task) {

    $('#sec-service').hide();
    $("#loaderimage").show();

    $('#hdnactcntct').val("cid" + aitid);
    $("#hdnapcde").val(apcde);
    _load(id, ad, sndnoid).then(function () {
        _ContactSearch($('#txtsearch').val(), false);

        $('#MsgSendArea').show();
        $("#lblSendername").text($("#senderNm" + aitid).text());
        $("#lblSenderMobile").text(mn);
        $("#hdnchatcontactid").val(id);
        
        $("#lblapiid").text(ad);
        $("#lblSenderNoID").text(sndnoid);
        $(".spnsenderno").text(sndno);
        $('#message-area').show();
        $('.csb').hide();
        if ($('#hdncontactid').val() == 0) {
            $('#chkMarkTask').hide();
        }
        else {
            $('#chkMarkTask').show();
            $('#chkMarkTask').prop('checked', task === 1 ? true : false);
        }
        $('#bottomscroll').scrollTop($('#bottomscroll')[0].scrollHeight);
        if (apcde !== 'ALERTHUB') {
            if ($("#hdnremtime").val() > 1) {
                jQuery(function ($) {
                    createElement($("#hdnremtime").val());
                    $("#jsTimer").show();
                    $("#inputmsg").removeAttr("disabled");
                    $('#btnSave').prop('disabled', false);
                });
            }
            else {
                $("#hdnremtime").val('');
                $("#jsTimer").hide();
                $("#inputmsg").attr("disabled", "disabled");
                $('#btnSave').prop('disabled', true);
            }
        } else {
            $("#jsTimer").hide();
            $("#inputmsg").removeAttr("disabled");
            $('#btnSave').prop('disabled', false);
        }
    });
}
function syncUserContact(uid) {
    var formData = new FormData();
    formData.append('UID', uid);
    $.ajax({
        type: 'POST',
        url: '/SyncContacts',
        processData: false,
        contentType: false,
        data: formData,
        beforeSend: function () {
            $("#loaderimage").show();
        },
        success: result => {
            //  alert(result.msg)
            _UserContactSearch($('#txtusercontacts').val());
            $('.modalusercontact').modal('show');
            $("#loaderimage").hide();
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
        },
        error: xhr => {
            $("#loaderimage").hide();
            an.title = "Oops! Error";
            an.content = xhr.status == 404 ? "Requested path not find" : (xhr.status == 0 ? "Internet is not connected" : "Server error");
            an.alert(-1);
            btnLdr.Stop(element.btn);
        },
        complete: () => $(e.currentTarget).val('')
    })
}
var _ContactSearch = (searchvalue = '', isFirstCall = true, dvsearch = '') => new Promise((resolve, reject) => {

    var hdnsno = $('#hdnsnno').val() === '' ? 0 : $('#hdnsnno').val();
    dvsearch = dvsearch == '' ? $('ul#Ultab').find('a.active').data('interest') : dvsearch;
    $.post('/ContactSearch', { SearchValue: searchvalue, TabFill: dvsearch, sid: hdnsno, isFirstCall: isFirstCall })
        .done(result => {
            if ($('#hdntabdiv').val() != '') {
                let parentDivPrev = $('#' + $('#hdntabdiv').val()).find('.ContacstScroll');
                parentDivPrev.empty();
            }
            $('#hdntabdiv').val(dvsearch);
            if (typeof result === 'object') {
                let parentDiv = $('#' + dvsearch).find('.ContacstScroll');
                $('#hdntabdiv').val(dvsearch);
                parentDiv.empty();

                for (let i = 0; i < result.getWhatsappContactList.length; i++) {
                    let obj = result.getWhatsappContactList[i];
                    let count = parentDiv.find('a').length;
                    let dataChatNo = obj.apiid + "_" + obj.senderName + "_" + obj.senderMobileNo;
                    parentDiv.append(`<div data-chat-no="${dataChatNo}" class="wrapBox">
                                                <a href="javascript:void(0)" id="contactid2" onclick="ChatArea(${obj.id},${obj.apiid},'${obj.mobileNo}',${count},'${obj.apICode}','${obj.senderMobileNo}','${obj.senderNoID}',${obj.task})">
                                                   <div id="cid${count}" tabindex="210" class="chat-list-item d-flex flex-row w-100 p-2 border-bottom unread act">
                                                        <div id="profileImage">${obj.prefixName}</div>
                                                        <div id="pid${count}" class="w-100" style="margin-left:18px">
                                                        <div id="senderNm${count}" class="name SenderName">${obj.senderName} ${obj.role !== '' ? '(' + obj.role + ')' : ''}
                                                             <img height="14px" src="/whatsapp/img/${obj.apICode}.png">
                                                         </div>
                                                        <div id="cmob1" class="small last-message cmobile">${obj.mobileNo}</div>
                                                   </div>
                                                   <div class="numberCircle">${obj.newMsgs > 0 ? '<span>' + obj.newMsgs + '</span>' : ''}</div></div></div></a>`);
                }
            }
            else {
                $('#' + dvsearch).html(result);
            }
            // This Section IS Used For Active Tab Contact
            var txtsa = $("#txtsearch").val();
            if (txtsa === '' && $('#chkUnseenMsg').is(':checked') === false) {
                var hdnactcntct = $('#hdnactcntct').val();
                if ($('#hdnactcntct').val() !== '') {
                    $('.chat-list-item').removeClass('active');
                    $('#' + hdnactcntct).addClass('active');
                }
            }
            // This Section IS Used For Active Tab Contact finsihed
            resolve();
        })
        .catch(function (xhr, e, msg) {
            reject(xhr);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })

})
$('#ddlSenderNo').ddslick({
    onSelected: function (selectedData) {
        if (selectedData.selectedIndex > 0) {
            $('#hdnsnno').val(selectedData.selectedData.value);
        }
        else {
            $('#hdnsnno').val(0);
        }
        _ContactSearch($('#txtsearch').val(), true);
    }
});
//functions and method
var chklastconver = function () {
    var ac = $("#hdnapcde").val();
    if (ac == "") { return; }
    var c = $('#chklastconver').is(':checked') == true ? true : false;
    $.post('/ForwardContacts', { SearchValue: "", ac: ac, onedaychat: c })
        .done(function (result) {
            $("#modalpopup").html(result);
        })
}
var _ForwardMessageContactSearch = function (searchvalue, cid, aid) {
    var ac = $("#hdnapcde").val();
    if (ac == "") {
        an.title = "Error !";
        an.content = "Some thing wrong try again later";
        an.alert(-1);
        return;
    }
    $.post('/ForwardContacts', { SearchValue: searchvalue, ac: ac, onedaychat: 0 })
        .done(function (result) {
            $("#modalpopup").html(result);
            $("#exampleModalCenter").modal('show');
            var ctMember = { btnbtnForwardMessage: $('#btnForwardMessage'), btnclosedforward: $('#btnclosedforward') };
            //ctMember.chklastconver.change(function () {
            //});
            ctMember.btnbtnForwardMessage.click(function () {

                var numberNotChecked = $('input:checkbox:checked').length;
                if (numberNotChecked < 1) {
                    an.title = "Validation";
                    an.content = "Select at least one contatct to send message";
                    an.alert(-1);
                    return;
                }
                var jsonObj = {};
                var Contatcts = [];
                // var MessageText = $("#SentMsg" + aid).text().trim();
                var MessageText = $("#SentMsg" + aid).html();
                var snm = $('#lblSendername').text().trim();
                if (MessageText == "") {
                    an.title = "Error !";
                    an.content = "Some thing wrong try again later";
                    an.alert(-1);
                    return;
                }
                if (cid == "") {
                    an.title = "Error !";
                    an.content = "Some thing wrong try again later";
                    an.alert(-1);
                    return;
                }
                var mno = "";
                $('input:checkbox:checked:checked').each(function () {
                    mno = this.value;
                    Contatcts.push({
                        MT: this.value
                    });
                });
                jsonObj.MessageText = MessageText.trim();
                jsonObj.snm = snm;
                jsonObj.Mobile = Contatcts;
                jsonObj.MsgID = cid;
                jsonObj.APICODE = ac;
                jsonObj.SenderNo = $(".spnsenderno").text();
                var formData = new FormData();
                $("#exampleModalCenter").modal('hide');
                formData.append('CTS', JSON.stringify(jsonObj));
                $.ajax({
                    type: 'POST',
                    url: '/SendForwardMessage',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: result => {
                        _ContactSearch($('#txtsearch').val(), false);
                        $("#exampleModalCenter").modal('hide');
                        $('#hdnmessage').val("");
                        $('#hdnmsgid').val("");
                    }
                })
            })
        })
        .catch(function (xhr, e, msg) {
        })
}
var _ReplyQuoted = function (WAID, msgTxt, id) {

    let _ReplyQuoted = $('#input-area');
    let _ReplyQuotedtext = $('.QuotedReply');
    _ReplyQuotedtext.empty();
    _ReplyQuoted.prepend(`<div class="QuotedReply">
                                <div class="quotemsgReply" data-id=${id} data-waid=${WAID} data-text=${msgTxt}>
                                    <div class="qname p-1 m-1">
                                        <span>${WAID}</span>
                                    </div>
                                    <div class="qmsg p-1 m-2">
                                       <span style="color:black">${msgTxt == '' ? msgTxt : msgTxt.length > 100 ? msgTxt.substring(0, 100) + '<b> ... </b>' : msgTxt}    </span>
                                    </div>
                                </div>
                                <button type="button" id="btnCloseQuotedReply" style="position: relative; margin-top:-51px;" class="close" aria-label="Close">
                                    <i class="fas fa-times text-muted px-3" style="cursor:pointer;"></i>
                                </button>
                            </div>`);
    $('#inputmsg').focus();
    $('#btnCloseQuotedReply').click(function () {
        let _ReplyQuoted = $('.QuotedReply');
        _ReplyQuoted.empty();
    });
}
var _UserContactSearch = function (searchvalue) {

    //preloader.load();
    $.post('/UserContactSearch', { SearchValue: searchvalue })
        .done(function (result) {

            $('#SearchUserContatcts').html(result);
            // alert(result)
            // $('#__p tbody').html(result);
        })

        .fail(function () {
            $(this).attr('checked', Is == false);
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
        })
        .always(function () {

        });
}

var _load = (id, AIID, SndNoID) => new Promise((resolve, reject) => {
    $.post('/WpChatArea', { ID: id, AID: AIID, sndnoid: SndNoID })
        .done(function (result) {
            $('#messages').html(result);
            $("#loaderimage").hide();
            $('#bottomscroll').scrollTop($('#bottomscroll')[0].scrollHeight);
            $(".sngclck").dropdown();
            resolve();
        })
        .catch(function (xhr, e, msg) {
            reject(xhr);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
})
function WhatsappTask() {
    var hid = ($('#hdncontactid').val());
    $.post('/SaveWhatsappTask', { CID: hid, Task: $('#chkMarkTask').is(':checked') === true ? 1 : 0 })
        .done(function (result) {
            an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
            an.content = result.msg;
            an.alert(result.statuscode);
        })
        .catch(function (xhr, e, msg) {
            reject(xhr);
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
        })
};




var GetSenderNoForService = function () {
    $.post("/GetSenderNoForService", function (data) {
        let _serviceNumbert = $('#sec-service');
        _serviceNumbert.empty();
        console.log(data);
        if ($.trim(data)) {
            $.each(data, function (i, item) {
                _serviceNumbert.append(`<div class="row" style="margin-top:6%">
                    <div class="col-sm-4"></div>
                    <div class="col-sm-5">
                        <div>
                            <table class="table">
                                <tr>
                                    <td> ${data[i].senderNo} <img src="/whatsapp/img/AlertHub.png" height="20px" /></td>
                                    <td>${data[i].passedChatTime}m</td>
                                    <td><a  onclick="ActiveSenderService()" style="cursor:pointer;color:blue" >Restart</a></td>
                                    <td class="service-loader" style="display:none" ><img src="/whatsapp/img/preloader.gif" height="20px" /></td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    <div class="col-sm-3"></div>
                </div>`);
            });
            $('.circleimg').css({ 'margin-top': '4%' });
        }
    })
}

var ActiveSenderService = function () {
    $('.service-loader').show();
    $.post("/ActWhatsappSenderNoService", function (result) {
        an.title = result.statuscode == an.type.success ? "Welldone" : "Oops";
        an.content = result.msg;
        an.alert(result.statuscode);
        $('.service-loader').hide();
    })
        .catch(function (xhr, e, msg) {
            reject(xhr);
            $('.service-loader').hide();
        })
        .fail(function (xhr) {
            an.title = 'Oops';
            an.content = xhr.status === 0 ? 'Internet Connection was broken' : 'Server error';
            an.alert(an.type.failed);
            reject(xhr);
            $('.service-loader').hide();
        })
}

















