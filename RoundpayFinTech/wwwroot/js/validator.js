var $validator = {
    numRegExp: new RegExp('^[0-9]+$'),
    emailRegExp: /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/,
    decRegExp: /^[-+]?[0-9]*\.?[0-9]+$/,
    $numOnly: function (str) {
        return str.replace(/[^0-9]/g, '');
    },
    $decmalOnly: function (str) {
        return str.match(/^[-+]?[0-9]*\.?[0-9]{2}/);
    },
    $IsMob: function (str) {
        return this.numRegExp.test(str) && str.length === 10;
    },
    $IsPincode: function (str) {
        return this.numRegExp.test(str) && str.length === 6;
    },
    $IsNum: function (str) {
        return this.numRegExp.test(str);
    },
    $IsDeciaml: function (str) {
        return this.decRegExp.test(str);
    },
    $IsEmail: function (str) {
        return this.emailRegExp.test(str);
    },
    showErrorFor: function (sender, err, isE) {
        if (sender === undefined)
            return;
        var describBy = $('#' + sender.attr('aria-describedby'));
        sender.addClass(isE === false ? 'is-valid' : 'is-invalid').removeClass(isE === false ? 'is-invalid' : 'is-valid');
        if (isE === false) {
            describBy.addClass('d-none');
        } else {
            describBy.removeClass('d-none');
            describBy.html(err);
        }
    },
    $RemoveAllSpecials: function (str) {
        return str.replace(/([~!@#$%^&*()_+=`{}\[\]\|\\:;'<>,.\/? ])+/g, '-').replace(/^(-)+|(-)+$/g, '');
    }
}