using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateAPIErrCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateAPIErrCode(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (APIErrorCodeReq)obj;
            if (!string.IsNullOrEmpty(req.APIErrorCode.APICode))
            {
                req.APIErrorCode.APICode = req.APIErrorCode.APICode.Trim(',').Replace(" ","").ToUpper();
            }
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@EID",req.APIErrorCode.EID),
                new SqlParameter("@GroupID",req.APIErrorCode.GroupID),
                new SqlParameter("@APIErrCode",req.APIErrorCode.APICode??string.Empty),
                new SqlParameter("@IP",req.CommonStr??string.Empty),
                new SqlParameter("@Browser",req.CommonStr2??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateAPIErrCode";
    }
}
