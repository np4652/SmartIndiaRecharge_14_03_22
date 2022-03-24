using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.SDK;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateSDKLoginRequest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateSDKLoginRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (SDKRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID??0),
                new SqlParameter("@PIN",HashEncryption.O.Encrypt(req.PIN??string.Empty)),
                new SqlParameter("@OutletID",req.OutletID??0),
                new SqlParameter("@PartnerID",req.PartnerID??0),
                new SqlParameter("@SPKey",req.SPKey??string.Empty),
                new SqlParameter("@Token",req.Token??string.Empty)
            };
            var res = new SDKResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? res.Msg : Convert.ToString(dt.Rows[0]["Msg"]);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.OID = dt.Rows[0]["_OID"] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0]["_OID"]);
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
                    UserId = req.UserID??0
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_ValidateSDKLoginRequest";
    }
}
