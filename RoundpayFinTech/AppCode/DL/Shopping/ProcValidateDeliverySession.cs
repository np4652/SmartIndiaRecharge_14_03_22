using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcValidateDeliverySession : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateDeliverySession(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@SessID",_req.SessID),
                new SqlParameter("@SessionID",_req.SessionID??""),
                new SqlParameter("@IsOTPMatchUpdate",_req.IsOTPMatchUpdate),
                new SqlParameter("@CookieExpireTime",_req.CookieExpireTime==null?DateTime.Now.AddHours(24):_req.CookieExpireTime),
                new SqlParameter("@RequestMode",_req.RequestMode),
                new SqlParameter("@IP",_req.RequestIP??""),
                new SqlParameter("@Browser",_req.Browser??""),
                new SqlParameter("@IMEI",_req.CommonStr??"")
            };
            var _lr = new LoginDeliveryPersonnelResp();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _lr.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _lr.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_lr.Statuscode == ErrorCodes.One)
                    {
                        _lr.OTP = dt.Rows[0]["OTP"].ToString();
                        _lr.SessionID = dt.Rows[0]["SessionID"].ToString();
                        _lr.SessID = Convert.ToInt32(dt.Rows[0]["SessID"]);
                        _lr.UserID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        _lr.CookieExpire = Convert.ToString(dt.Rows[0]["CookieExpire"]);
                        _lr.ID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        _lr.Name = Convert.ToString(dt.Rows[0]["_Name"]);
                        _lr.Mobile = Convert.ToString(dt.Rows[0]["_Mobile"]);
                        _lr.Address = dt.Rows[0]["_Address"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Address"]);
                        _lr.Area = dt.Rows[0]["_Area"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Area"]);
                        _lr.CityId = dt.Rows[0]["_CityId"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CityId"]);
                        _lr.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_Pincode"]);
                        _lr.VehicleNumber = dt.Rows[0]["_VehicalNum"] is DBNull ? "" : Convert.ToString(dt.Rows[0]["_VehicalNum"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _lr;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_CheckDeliveryAppSession";
    }
}
