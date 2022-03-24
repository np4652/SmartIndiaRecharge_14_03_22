using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveWebNotification : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcSaveWebNotification(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (WebNotification)obj;
            var res = new WebNotification
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserId",req.UserID),
                new SqlParameter("@Title",req.Title),
                new SqlParameter("@Notification",req.Notification),
                new SqlParameter("@FormatID",req.FormatID),
                new SqlParameter("@Operator",req.Operator),
                new SqlParameter("@Img",req.Img??""),
                new SqlParameter("@RoleID",req.RoleId??""),
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_SaveWebNotification";
    }
}