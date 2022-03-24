using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL.Shopping;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class DeliveryML : BaseML, IDeliveryML
    {
        private readonly IRequestInfo _rinfo;
        public DeliveryML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true) : base(accessor, env, IsInSession)
        {
            _rinfo = new RequestInfo(_accessor, _env);
        }

        #region DeliveryPersonnel
        public DeliveryPersonnelList GetDeliveryPersonnelList(bool ActiveOnly = true)
        {
            var res = new DeliveryPersonnelList
            {
                Status = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = new CommonRequest
            {
                LoginId = _lr != null ? _lr.UserID : 0,
                LT = _lr != null ? _lr.LoginTypeID : 0,
                CommonBool = ActiveOnly
            };
            IProcedure proc = new ProcGetDeliveryPersonnel(_dal);
            res = (DeliveryPersonnelList)proc.Call(req);
            return res;
        }

        public AUDeliverPersonnel GetDeliveryPersonnelById(int id)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new AUDeliverPersonnel
            {
                Status = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS,
                Cities = ml.Cities(0).ToList()
            };
            if (id > 0)
            {
                var personnel = GetDeliveryPersonnelList(false).DeliveryPersonnels.Where(i => i.ID == id).FirstOrDefault();
                res.ID = personnel.ID;
                res.Name = personnel.Name;
                res.Mobile = personnel.Mobile;
                res.Address = personnel.Address;
                res.Area = personnel.Area;
                res.CityId = personnel.CityId;
                res.Pincode = personnel.Pincode;
                res.Location = personnel.Location;
                res.Aadhar = personnel.Aadhar;
                res.VehicleNumber = personnel.VehicleNumber;
                res.DLId = personnel.DLId;
                res.IsActive = personnel.IsActive;
                res.Password = personnel.Password;
            }
            return res;
        }

        public ResponseStatus AUDeliveryPersonnel(AUDeliverPersonnel req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcAUDeliveryPersonnel(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public ResponseStatus DeliveryPersonnelStatus(AUDeliverPersonnel req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateDeliveryPersonnelStatus(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public ResponseStatus UpdateDeliveryPersonnelStatusLocation(int id, int status, string latitude, string longitude, int userid = 0, int OrderDetailId = 0)//status - 0 = Unavailbale, 1=Available, 2=DeliveryAssigned, 3 =DEliverycomplete
        {
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            CommonReq req = new CommonReq
            {
                LoginID = userid > 0 ? userid : _lr.UserID,
                LoginTypeID = userid > 0 ? 1 : _lr.LoginTypeID,
                CommonInt = id,
                CommonInt2 = status,
                CommonInt3 = OrderDetailId,
                CommonStr = latitude,
                CommonStr2 = longitude
            };
            IProcedure proc = new ProcUpdateDeliveryPersonnelStatusLocation(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public LoginDeliveryPersonnel LoginDeliveryPersonnel(LoginDeliveryPersonnelReq req)
        {
            req.IP = _rinfo.GetRemoteIP();
            req.RequestMode = RequestMode.APPS;
            req.Browser = req.SerialNo + "_" + req.Version;
            IProcedure proc = new ProcLoginDeliveryPersonnel(_dal);
            var response = (LoginDeliveryPersonnel)proc.Call(req);
            return response;
        }

        public ResponseStatus UpdateDeliveryPersonnelToken(DPToken dPToken)
        {
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            CommonReq req = new CommonReq
            {
                LoginID = dPToken.UserID,
                LoginTypeID = dPToken.LoginTypeID,
                CommonInt = dPToken.SessionID,
                CommonStr = dPToken.Token,
                CommonStr2 = dPToken.Session
            };
            IProcedure proc = new ProcUpdateDeliveryPersonnelToken(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public LoginDeliveryPersonnelResp ValidateLoginDeliveryPersonnel(LoginDetail req)
        {
            req.RequestIP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcValidateDeliverySession(_dal);
            var response = (LoginDeliveryPersonnelResp)proc.Call(req);
            return response;
        }

        public List<OrderDeliveryResp> GetOrderDeliveryList(int OrderDetailId, int UserId = 0) // Userid id to be passed in case of Adminreport filter
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = OrderDetailId,
                CommonInt2 = UserId
            };
            IProcedure proc = new ProcGetDeliveryOrderDetail(_dal);
            return (List<OrderDeliveryResp>)proc.Call(req);
        }

        public OrderDeliveryResp GetOrderDetailForDelivery(int LoginId, int OrderDetailId, int UserId = 0)//For App
        {
            var req = new CommonReq
            {
                LoginID = LoginId > 0 ? LoginId : _lr.UserID,
                LoginTypeID = LoginId > 0 ? 1 : _lr.LoginTypeID,
                CommonInt = OrderDetailId,
                CommonInt2 = UserId
            };
            IProcedure proc = new ProcGetOrderDetailForDelivery(_dal);
            return (OrderDeliveryResp)proc.Call(req);
        }

        public DeliveryDashboard GetDeliveryDashboard(int LoginId, int LT)
        {
            var req = new CommonReq
            {
                LoginID = LoginId > 0 ? LoginId : _lr.UserID,
                LoginTypeID = LoginId > 0 ? 1 : _lr.LoginTypeID
            };
            IProcedure proc = new ProcDeliveryDashboard(_dal);
            var response = (DeliveryDashboard)proc.Call(req);
            return response;
        }

        public DPLocationList GetDPLocationHistory(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr == null ? 0 : _lr.UserID,
                LoginTypeID = _lr == null ? 0 : _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetDPLocationHistory(_dal);
            var response = (DPLocationList)proc.Call(req);
            return response;
        }
        #endregion
    }
}
