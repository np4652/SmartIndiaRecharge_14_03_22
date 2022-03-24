const $AEPSDevice = Object.freeze({ "Mantra": 1, "Morpho": 2, "Tatvik": 3, "StarTech": 4 });
const $PIDOptions = Object.freeze({
    1: '<?xml version="1.0"?><PidOptions ver="1.0"> <Opts fCount="1" fType="0" iCount="0" pCount="0" pgCount="2" format="0" pidVer="2.0" timeout="10000" pTimeout="20000"  posh="UNKNOWN" env="P" /> <CustOpts><Param name="mantrakey" value=""/></CustOpts></PidOptions>', 2: '<PidOptions ver="1.0"><Opts fCount="1" fType="0" iCount="" iType="" pCount="" pType="" format="0" pidVer="2.0" timeout="10000" otp="" wadh="" posh=""/></PidOptions>', 3: '<PidOptions ver="1.0"><Opts env="S" fCount="1" fType="0" format="0" pType="0" pCount="0" pgCount="0" pTimeout="20000" pidVer="2.0"></Opts><Demo></Demo><CustOpts></CustOpts><Bios></Bios></PidOptions>', 4: ''
});
const $PIDOptionsLive = Object.freeze({
    1: '<?xml version="1.0"?><PidOptions ver="1.0"> <Opts fCount="1" fType="1" iCount="0" pCount="0" pgCount="2" format="0" pidVer="2.0" timeout="10000" pTimeout="20000" wadh="E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=" posh="UNKNOWN" env="P" /> <CustOpts><Param name="mantrakey" value=""/></CustOpts></PidOptions>', 2: '<PidOptions ver="1.0"><Opts fCount="1" fType="1" iCount="" iType="" pCount="" pType="" format="0" pidVer="2.0" timeout="10000" otp="" wadh="E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=" posh=""/></PidOptions>', 3: '<PidOptions ver="1.0"><Opts env="S" fCount="1" fType="1" format="0" pType="0" pCount="0" pgCount="0" pTimeout="20000" pidVer="2.0"></Opts><Demo></Demo><CustOpts></CustOpts><Bios></Bios></PidOptions>', 4: ''
});
const $DeviceMethod = Object.freeze({ 1: { "MSG_RD": "RDService found successfully for Mantra!", "MSG_DI": "Mantra device found successfully!", "MSG_DC": "Captured from Mantra device successfully!", "DEVICEINFO": { "Method": "DEVICEINFO", "URL": "/rd/info" }, "CAPTURE": { "Method": "CAPTURE", "URL": "/rd/capture" } }, 2: { "MSG_RD": "RDService found successfully for Morpho!", "MSG_DI": "Morpho device found successfully!", "MSG_DC": "Captured from Morpho device successfully!", "DEVICEINFO": { "Method": "DEVICEINFO", "URL": "/getDeviceInfo" }, "CAPTURE": { "Method": "CAPTURE", "URL": "/capture" } }, 3: { "MSG_RD": "RDService found successfully for Tatvik!", "MSG_DI": "Tatvik device found successfully!", "MSG_DC": "Captured from Tatvik device successfully!", "DEVICEINFO": { "Method": "DEVICEINFO", "URL": "/rd/info" }, "CAPTURE": { "Method": "CAPTURE", "URL": "/rd/capture" } }, 4: { "MSG_RD": "RDService found successfully for StarTech!", "MSG_DI": "StarTech device found successfully!", "MSG_DC": "Captured from StarTech device successfully!", "DEVICEINFO": { "Method": "DEVICEINFO", "URL": "/rd/info" }, "CAPTURE": { "Method": "CAPTURE", "URL": "/rd/capture" } } })
const $DeviceDPID = Object.freeze({ 1: { RD: "Mantra", info: "MFS" }, 2: { RD: "Morpho", info: "MSO" }, 3: { RD: "TMF20", info: "TMF20" }, 4: { RD: "FM220", info: "FM220" } });
const $Startport = Object.freeze({ 1: 11100, 2: 11100, 3: 11101, 4: 11100 })
const $Endport = Object.freeze({ 1: 11105, 2: 11120, 3: 11105, 4: 11105 })
an.autoClose = 0;
class RDServiceHelper {
    constructor(deviceid) {
        this._deviceid = deviceid;
        this._protocol = 'http:';
        this._domain = '127.0.0.1';
        this._port = $Startport[deviceid];
        this._IsPortFound = false;
        this._IsDInfoFound = false;
        this._outputData = "";
        this._PIDOptions = $PIDOptions[deviceid];
        this._AlertMsg = '';
        this._AlertType = -1;
        this._preloader = preloader;
    }
    ScanPort = function (port, btnObj, onCaptured) {
        this._IsDInfoFound = false;
        let _this = this;
        let _doc;
        let _rd;
        if (port > $Endport[this._deviceid]) {
            if (this._IsPortFound === false) {
                _this._AlertType = -1;
                _this._AlertMsg = "No RDService found for selected device!";
                console.warn(_this._AlertMsg);
                an.title = _this._AlertType === an.type.success ? 'Success' : 'Oops';
                an.content = _this._AlertMsg;
                an.alert(_this._AlertType);
            }
            btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
            this._preloader.remove();
            return true;
        }
        if (this._IsPortFound === true) {
            _this._AlertType = 1;
            _this._AlertMsg = $DeviceMethod[_this._deviceid].MSG_RD;
            an.title = 'Success';
            an.content = _this._AlertMsg;
            an.alert(_this._AlertType);
            btnLdr.StartWithAnyText(btnObj.btn, "Getting DeviceInfo");
            this.DeviceInfo(this._port, btnObj, onCaptured);
            return true;
        }
        $.ajax({
            url: _this._protocol + '//' + _this._domain + ':' + port,
            type: 'RDSERVICE',
            async: true,
            crossDomain: true,
            contentType: 'text/xml; charset=utf-8',
            processData: false,
            success: function (result) {
                _doc = $.parseXML(result);
                _doc = _doc === null ? result : _doc;
                _rd = $(_doc).find('RDService').attr('info');
                if (_rd !== undefined) {
                    if (_rd.indexOf($DeviceDPID[_this._deviceid].RD) > -1 && $DeviceDPID[_this._deviceid].RD !== '') {
                        _this._IsPortFound = true;
                        _this._outputData = result;
                        _this._port = port;
                    }
                }
            },
            error: function (xhr, result) {
                if (xhr.status === 404) {
                    console.warn("Requested path not find");
                } else if (xhr.status === 0) {
                    console.warn("Service Unavailable");
                } else {
                    console.warn("Server error");
                }
                //btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
                //_this._preloader.remove();
            },
            complete: function () {
                port++;
                _this.ScanPort(port, btnObj, onCaptured);
            }
        });
    }
    DeviceInfo = function (port, btnObj, onCaptured) {
        let _this = this;
        let _doc;
        let _mi;
        if (port > $Endport[this._deviceid]) {
            if (this._IsDInfoFound === false) {
                _this._AlertType = -1;
                _this._AlertMsg = "Selected device is not connected with system. Please check it again!";
                console.warn(_this._AlertMsg);
                //an.autoClose = 5;
                an.title = _this._AlertType === an.type.success ? 'Success' : 'Oops';
                an.content = _this._AlertMsg;
                an.alert(_this._AlertType);
            }
            btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
            this._preloader.remove();
            return true;
        }
        if (this._IsDInfoFound === true) {

            btnLdr.StartWithAnyText(btnObj.btn, "Capturing...");
            this._IsDCaptured = false;
            this.CaputureDevice(onCaptured, btnObj);
            return true;
        }
        $.ajax({
            url: _this._protocol + '//' + _this._domain + ':' + port + $DeviceMethod[_this._deviceid].DEVICEINFO.URL,
            type: $DeviceMethod[_this._deviceid].DEVICEINFO.Method,
            async: false,
            crossDomain: true,
            contentType: 'text/xml; charset=utf-8',
            processData: false,
            success: function (result) {
                _doc = $.parseXML(result);
                if (_doc === null) {
                    _doc = result;
                }
                _mi = $(_doc).find('DeviceInfo').attr('mi');
                if (_mi !== undefined) {
                    if ($DeviceDPID[_this._deviceid].info !== '' && _mi !== '') {
                        if (_mi.indexOf($DeviceDPID[_this._deviceid].info) > -1) {
                            if ($(_doc).find('DeviceInfo').attr('mc') !== '') {
                                _this._IsDInfoFound = true;
                                _this._outputData = result;
                                _this._AlertType = 1;
                                _this._AlertMsg = $DeviceMethod[_this._deviceid].MSG_DI;
                                an.title = 'Success';
                                an.content = _this._AlertMsg;
                                an.alert(_this._AlertType);
                            }
                        }
                    }
                }
            },
            error: function (xhr, result) {
                if (xhr.status === 404) {
                    console.warn("Requested path not find");
                } else if (xhr.status === 0) {
                    console.warn("Service Unavailable");
                } else {
                    console.warn("Server error");
                }
                //btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
                //_this._preloader.remove();
            },
            complete: function () {
                port++;
                _this.DeviceInfo(port, btnObj, onCaptured);
            }
        });
    }
    CaputureDevice = function (f, btnObj) {
        let _this = this;
        let _doc;
        let _ec = -1;
        let _em;
        _this._outputData = '';
        $.ajax({
            url: _this._protocol + '//' + _this._domain + ':' + _this._port + $DeviceMethod[_this._deviceid].CAPTURE.URL,
            data: _this._PIDOptions,
            type: $DeviceMethod[_this._deviceid].CAPTURE.Method,
            async: true,
            crossDomain: true,
            contentType: 'text/xml; charset=utf-8',
            processData: false,
            success: function (result) {
                _doc = $.parseXML(result);
                if (_doc === null) {
                    _doc = result;
                }
                _ec = parseInt($(_doc).find('Resp').attr('errCode'));
                _em = $(_doc).find('Resp').attr('errInfo');
                if (_ec === 0) {
                    _this._AlertType = 1;
                    _this._AlertMsg = $DeviceMethod[_this._deviceid].MSG_DC;
                    _this._outputData = _doc;
                    f(_this._outputData);
                } else {
                    _this._AlertType = -1;
                    _this._AlertMsg = "Capture Error: " + _em;
                    btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
                    _this._preloader.remove();
                }
                an.title = _this._AlertType === an.success ? 'Success' : 'Oops';
                an.content = _this._AlertMsg;
                an.alert(_this._AlertType);
            },
            error: function (xhr, result) {
                if (xhr.status === 404) {
                    console.warn("Requested path not find");
                } else if (xhr.status === 0) {
                    console.warn("Service Unavailable");
                } else {
                    console.warn("Server error");
                }
                //btnLdr.StopWithText(btnObj.btn, btnObj.btnText);
                //_this._preloader.remove();
            },
            complete: function () {
               
            }
        });
    }
}