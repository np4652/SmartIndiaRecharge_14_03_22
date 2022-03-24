using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QRCoder;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        [HttpPost]
        public async Task<IActionResult> GetVADetail([FromBody] AppSessionReq appSessionReq)
        {
            var virtualAccountResponse = new VirtualAccountResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResp = appML.CheckAppSession(appSessionReq);
                virtualAccountResponse.IsAppValid = appResp.IsAppValid;
                virtualAccountResponse.IsVersionValid = appResp.IsVersionValid;
                virtualAccountResponse.IsPasswordExpired = appResp.IsPasswordExpired;
                virtualAccountResponse.Statuscode = appResp.Statuscode;

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        if (ApplicationSetting.IsAddMoneyEnable && ApplicationSetting.IsUPI)
                        {
                            IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
                            virtualAccountResponse.userQRInfo = paymentML.GetUPIQRBankDetail(appSessionReq.LoginTypeID, appSessionReq.UserID);
                        }
                        else
                        {
                            virtualAccountResponse.Statuscode = ErrorCodes.Minus1;
                            virtualAccountResponse.Msg = ErrorCodes.AuthError;
                        }
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetVADetail",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(virtualAccountResponse)
            });
            return Json(virtualAccountResponse);
        }
        [HttpGet]
        [Route("App/qrforupi.png")]
        public async Task<IActionResult> GetQRImage(QRCodeRequest appSessionReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                appResp = appML.CheckAppSession(appSessionReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
                    var res = paymentML.GetUPIQR(appSessionReq.LoginTypeID, appSessionReq.UserID, appSessionReq.Amount);
                    if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CommonStr4))
                    {

                        QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                        QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CommonStr4, QRCodeGenerator.ECCLevel.Q);
                        QRCode qRCode = new QRCode(QCD);
                        return File(ConverterHelper.BitmapToBytesCode(qRCode.GetGraphic(20)), "image/png");
                    }
                    else
                    {
                        appResp.Msg = res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                    }
                }
            }
            Bitmap b = new Bitmap(500, 500);
            Graphics g = Graphics.FromImage(b);
            g.DrawString(appResp.Msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
            return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
        }

        [HttpGet]
        [Route("App/qrFromShortURL.png")]
        public IActionResult UserUPIQRCodeShortURL(QRCodeRequest appSessionReq)
        {
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResp = appML.CheckAppSession(appSessionReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    appSessionReq.ShortURL = appSessionReq.ShortURL.Replace("\\", "");
                    return File(AppWebRequest.O.CallUsingHttpWebRequest_GETImageAsync(appSessionReq.ShortURL, null).Result, "image/png");
                }
            }
            return BadRequest("4O4 not found");
        }
        [HttpGet]
        [Route("App/qrforcoin.png")]
        public async Task<IActionResult> GetQRCoinImage(QRCodeRequest appSessionReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                appResp = appML.CheckAppSession(appSessionReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    ICoinML coinML = new CoinML(_accessor, _env);
                    var res = coinML.GenerateQR(string.Empty, appSessionReq.UserID, Convert.ToInt32(appSessionReq.spkey));
                    if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CoinAddress))
                    {

                        QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                        QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CoinAddress, QRCodeGenerator.ECCLevel.Q);
                        QRCode qRCode = new QRCode(QCD);
                        return File(ConverterHelper.BitmapToBytesCode(qRCode.GetGraphic(20)), "image/png");
                    }
                    else
                    {
                        string msg = res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                        Bitmap b1 = new Bitmap(500, 500);
                        Graphics g1 = Graphics.FromImage(b1);
                        g1.DrawString(msg, new Font("Arial", 28), Brushes.Red, new Point(10, 10));
                        return File(ConverterHelper.BitmapToBytesCode(b1), "image/png");
                    }
                }
            }
            Bitmap b = new Bitmap(500, 500);
            Graphics g = Graphics.FromImage(b);
            g.DrawString(appResp.Msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
            return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
        }

        [HttpGet]
        [Route("App/CoinAddress4Copy")]
        public async Task<IActionResult> CoinAddress4Copy(QRCodeRequest appSessionReq)
        {
            var appResp = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var resp = appML.CheckAppSession(appSessionReq);
                appResp.Statuscode = resp.Statuscode;
                appResp.Msg = resp.Msg;
                appResp.IsAppValid = resp.IsAppValid;
                appResp.IsVersionValid = resp.IsVersionValid;
                appResp.IsPasswordExpired = resp.IsPasswordExpired;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    ICoinML coinML = new CoinML(_accessor, _env);
                    var res = coinML.GenerateQR(string.Empty, appSessionReq.UserID, appSessionReq.OID);
                    appResp.Statuscode = res.Statuscode;
                    appResp.Msg = res.Msg;
                    appResp.data = res.CoinAddress;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "CoinAddress4Copy",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }

        [HttpPost]
        [Route("App/checkstatusCoin")]
        public async Task<IActionResult> CheckCoinBalanceStatus([FromBody] QRCodeRequest appSessionReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                appResp = appML.CheckAppSession(appSessionReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    ICoinML coinML = new CoinML(_accessor, _env);
                    var res = coinML.CheckBalance(string.Empty, appSessionReq.UserID, Convert.ToInt32(appSessionReq.spkey));
                    
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "checkstatusCoin",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }

        [HttpPost]
        [Route("App/CoinConversionRate")]
        public async Task<IActionResult> CoinConversionRate([FromBody] AppSessionReq appSessionReq)
        {
            var appResp = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
               var  resp = appML.CheckAppSession(appSessionReq);
                appResp.Statuscode = resp.Statuscode;
                appResp.Msg = resp.Msg;
                appResp.IsAppValid = resp.IsAppValid;
                appResp.IsVersionValid = resp.IsVersionValid;
                appResp.IsPasswordExpired = resp.IsPasswordExpired;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    ICoinML coinML = new CoinML(_accessor, _env);
                    appResp.data = coinML.GetRates(appSessionReq.OID);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "CoinConversionRate",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
    }
}
