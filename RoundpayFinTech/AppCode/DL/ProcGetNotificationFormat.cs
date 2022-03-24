using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetNotificationFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNotificationFormat(IDAL dal) => _dal = dal;
        public string GetName() => "select * from tbl_MessageTemplate where _FormatID=@FormatID";
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@FormatID",req.CommonInt)
            };
            var _res = new ResponseStatus();
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.CommonBool = dt.Rows[0]["_IsEnableNotification"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableNotification"]);
                    _res.CommonStr = Convert.ToString(dt.Rows[0]["_AlertTemplate"]);
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
    }
}
