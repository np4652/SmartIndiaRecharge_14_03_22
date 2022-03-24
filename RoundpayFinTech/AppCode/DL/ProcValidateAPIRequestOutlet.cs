using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateAPIRequestOutlet : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateAPIRequestOutlet(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OutletID", _req.CommonInt),
                new SqlParameter("@IsValidate", _req.IsListType)
            };

            var _res = new ValidateAPIOutletResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.OutletMobile = dt.Rows[0]["OutletMobile"] is DBNull ? "" : dt.Rows[0]["OutletMobile"].ToString();
                        _res.IsOutletActive = dt.Rows[0]["IsOutletActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsOutletActive"]);
                        _res.OutletVerifyStatus = dt.Rows[0]["OutletVerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["OutletVerifyStatus"]);
                        _res.PAN = dt.Rows[0]["PAN"] is DBNull ? "" : dt.Rows[0]["PAN"].ToString();
                        _res.AADHAR = dt.Rows[0]["AADHAR"] is DBNull ? "" : dt.Rows[0]["AADHAR"].ToString();
                        _res.APICode = dt.Rows[0]["APICode"] is DBNull ? "" : dt.Rows[0]["APICode"].ToString();
                        _res.APIID = dt.Rows[0]["APIID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["APIID"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? "" : dt.Rows[0]["TransactionID"].ToString();
                        _res.APIOutletID = dt.Rows[0]["APIOutletID"] is DBNull ? "" : dt.Rows[0]["APIOutletID"].ToString();
                        _res.APIOutletVerifyStatus = dt.Rows[0]["APIOutletVerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["APIOutletVerifyStatus"]);
                        _res.APIOutletDocVerifyStatus = dt.Rows[0]["APIOutletDocVerifyStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["APIOutletDocVerifyStatus"]);
                        _res.Latlong = dt.Rows[0]["LatLong"] is DBNull ? "" : dt.Rows[0]["LatLong"].ToString();//@PinCode
                        _res.Pincode = dt.Rows[0]["PinCode"] is DBNull ? "" : dt.Rows[0]["PinCode"].ToString();
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
            return _res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_ValidateAPIRequestOutlet";
        }
    }
}
