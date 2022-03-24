using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCallbackUrlCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCallbackUrlCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UserCallBackModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@CallBackType",req.CallbackType),
                new SqlParameter("@URL",req.URL??""),
                new SqlParameter("@UpdateUrl",req.UpdateUrl??""),
                new SqlParameter("@Remark",req.Remark??""),
            };
            var res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt16(dt.Rows[0][0]);
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
                    LoginTypeID = 1,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_CallbackUrlCU";
    }
}
