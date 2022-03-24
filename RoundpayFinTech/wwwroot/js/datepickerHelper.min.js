var DTPicker = function (ctrl,maxdate,callbackOnHide,callbackOnChange) {
    return ctrl.datetimepicker({ maxDate: (maxdate === '') ? new Date() : new Date(maxdate), format: 'DD MMM Y', ignoreReadonly: true }).on('dp.hide', function () { callbackOnHide(); }).on('dp.change', function () { callbackOnChange();}).data("DateTimePicker");
}

var MPicker = function (ctrl, maxdate, callbackOnHide, callbackOnChange) {
    return ctrl.datetimepicker({ maxDate: (maxdate === '') ? new Date() : new Date(maxdate), format: 'MMM Y', ignoreReadonly: true }).on('dp.hide', function () { callbackOnHide(); }).on('dp.change', function () { callbackOnChange(); }).data("DateTimePicker");
}



var TimePicker = function (ctrl, maxdate, callbackOnHide, callbackOnChange) {
    return ctrl.datetimepicker({ maxDate: (maxdate === '') ? new Date().getTime() : new Date(maxdate), format: 'hh mm A', ignoreReadonly: true }).on('dp.hide', function () { callbackOnHide(); }).on('dp.change', function () { callbackOnChange(); }).data("DateTimePicker");
}


var DatePicker = function (ctrl, maxdate, callbackOnHide, callbackOnChange) {
    return ctrl.datetimepicker({ format: 'DD MMM Y', ignoreReadonly: true }).on('dp.hide', function () { callbackOnHide(); }).on('dp.change', function () { callbackOnChange(); }).data("DateTimePicker");
}
