using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckEKYCOperatorSwithing : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckEKYCOperatorSwithing(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@SPKey",req.CommonStr??string.Empty)
            };
            var res = new EKYCSwitchAPIResp
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One) { 
                        res.APIID= dt.Rows[0]["_APIID"] is DBNull ? 0 :Convert.ToInt32( dt.Rows[0]["_APIID"]);
                        res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        res.OID = dt.Rows[0]["_OID"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_OID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_CheckEKYCOperatorSwithing";
    }
}
