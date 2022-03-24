using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        [HttpGet]
        public IActionResult UserUPIQRCode(int a)
        {
            IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
            var res = paymentML.GetUPIQR(_lr.LoginTypeID, _lr.UserID, a);
            if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CommonStr4))
            {

                QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CommonStr4, QRCodeGenerator.ECCLevel.Q);
                QRCode qRCode = new QRCode(QCD);
                return File(ConverterHelper.BitmapToBytesCode(qRCode.GetGraphic(20)), "image/png");
            }
            else
            {
                string msg = res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                Bitmap b = new Bitmap(500, 500);
                Graphics g = Graphics.FromImage(b);
                g.DrawString(msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
        }
        [HttpGet]
        public IActionResult UserUPIQRCodeShortURL(string ShortURL)
        {
            ShortURL = ShortURL.Replace("\\", "");
            return File(AppWebRequest.O.CallUsingHttpWebRequest_GETImageAsync(ShortURL, null).Result, "image/png");
        }

        [HttpPost]
        [Route("Report/UserVADetail")]
        [Route("UserVADetail")]
        public IActionResult UserVADetail()
        {
            IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
            var res = paymentML.GetUPIQRBankDetail(_lr.LoginTypeID, _lr.UserID);
            return Json(res);
        }
        #region CoinRelated
        [HttpPost]
        [Route("Report/CoinQRModalPopup")]
        [Route("CoinQRModalPopup")]
        public IActionResult CoinQRModalPopup()
        {
            IOperatorML OpML = new OperatorML(_accessor, _env);
            var res = new CoinQRViewModel
            {
                opList= OpML.GetOperatorsActive(OPTypes.Coin)
            };
            string spkey = res.opList.FirstOrDefault()?.OPID;
            ICoinML coinML = new CoinML(_accessor, _env);
            var res1 = coinML.GenerateQR(spkey ?? string.Empty, _lr.UserID);
            if (res1.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res1.CoinAddress))
            {
                res.QRAddress = res1.CoinAddress;
            }
            return PartialView("PartialView/_CoinQRModel", res);
        }
        public IActionResult CoinQRCode(string spkey)
        {
            ICoinML coinML = new CoinML(_accessor, _env);
            var res = coinML.GenerateQR(spkey, _lr.UserID);
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
                Bitmap b = new Bitmap(500, 500);
                Graphics g = Graphics.FromImage(b);
                g.DrawString(msg, new Font("Arial", 28), Brushes.Red, new Point(10, 10));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
        }
        [HttpPost]
        [Route("Report/CoinBalanceStatus")]
        [Route("CoinBalanceStatus")]
        public IActionResult CoinBalanceStatus(string spkey)
        {
            ICoinML coinML = new CoinML(_accessor, _env);
            var res = coinML.CheckBalance(spkey, _lr.UserID);
            return Json(res);
        }
        #endregion

    }
}
