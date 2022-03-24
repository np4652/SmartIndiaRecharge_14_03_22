using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcVerySenderOTP: IProcedure
    {
        private readonly IDAL _dal;
        public ProcVerySenderOTP(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;

            var res = new SenderInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            SqlParameter[] param = {
                new SqlParameter("@OTP", req.CommonStr2),
                new SqlParameter("@userID",req.CommonInt),
                new SqlParameter("@SenderMobileNo", req.CommonStr),
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.CommonInt
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_VerySenderOTP";
    }
}
