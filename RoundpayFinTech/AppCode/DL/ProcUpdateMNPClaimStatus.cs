using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateMNPClaimStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMNPClaimStatus(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (MNPUser)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", _req.ID),
                new SqlParameter("@VerifyStatus", _req.VerifyStatus),
                new SqlParameter("@Amount", _req.Amount),
                new SqlParameter("@Remark", _req.Remark),
                new SqlParameter("@FRCDate", _req.FRCDate),
                new SqlParameter("@FRCDemoNumber", _req.FRCDemoNumber),
                new SqlParameter("@FRCType", _req.FRCType),
                new SqlParameter("@FRCDoneDate", _req.FRCDoneDate),
                new SqlParameter("@ModifyBy", _req.UserID)

        };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.CommonInt = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                    _resp.CommonStr = dt.Rows[0]["_Pin"] is DBNull ? string.Empty : dt.Rows[0]["_Pin"].ToString();
                    _resp.CommonInt2 = dt.Rows[0]["_Status"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Status"]);

                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_UpdateMNPClaimStatus";
        }
    }
}