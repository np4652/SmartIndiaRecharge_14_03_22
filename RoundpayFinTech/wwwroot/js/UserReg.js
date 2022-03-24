var Login = function () {
    //errorMsg.removeClass('hide');
    //var U = $('#txtUser'), P = $('#txtPassword'), T = $('#ddlLType');
    //U.removeClass('is-invalid');
    //P.removeClass('is-invalid');
    //var UserID = U.val(), Password = P.val(), LoginTypeID = T.val();
    //if (UserID === "") {
    //    U.addClass('is-invalid');
    //    U.focus();
    //    Alerts('Enter User ID', AlertStatus.RED);
    //    return false;
    //}
    //U.addClass('is-valid');
    //if (Password === "") {
    //    P.addClass('is-invalid');
    //    P.focus();
    //    Alerts(P.attr('type') === 'text' ? 'Enter OTP' : 'Enter Password', AlertStatus.RED);
    //    return false;
    //}
    //P.addClass('is-valid');
    var UserCreate = {};
    LoginDetail.LoginMobile = UserID;
    LoginDetail.LoginTypeID = LoginTypeID;
    if (P.attr('type') === 'text')
        LoginDetail.OTP = Password;
    else
        LoginDetail.Password = Password;
    var URL = P.attr('type') === 'text' ? 'Login/OTP' : 'Login';

    an.id = "myalert";
    an.autoClose = 5;
    preloader.load();
    $.ajax({
        type: 'POST',
        url: URL,
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(LoginDetail),
        success: function (result) {
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
                location.href = page;
            }
            else if (result.statuscode === LoginStatus.OTP) {
                P.attr({ placeholder: 'Enter OTP', type: 'text' });
                Alerts('Enter OTP', AlertStatus.BLUE);
                an.title = "Info";
                an.content = result.msg;
                an.alert(an.type.info);
                P.val('');
                P.focus();
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