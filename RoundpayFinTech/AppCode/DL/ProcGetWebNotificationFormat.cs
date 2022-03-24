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
    public class ProcGetWebNotificationFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWebNotificationFormat(IDAL dal) => _dal = dal;
        public string GetName() => "select ISNULL(_IsEnableWebNotification,0) _IsEnableWebNotification,ISNULL(_WebNotificationTemplate,'') _WebNotificationTemplate from tbl_MessageTemplate  where _FormatID=@FormatID";
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
                    _res.CommonBool = Convert.ToBoolean(dt.Rows[0]["_IsEnableWebNotification"]);
                    _res.CommonStr = Convert.ToString(dt.Rows[0]["_WebNotificationTemplate"]);
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
