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
    public class ProcGetSocialAlertFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSocialAlertFormat(IDAL dal) => _dal = dal;        
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@FormatID",req.CommonInt),
                new SqlParameter("@SocialAlertType",req.CommonInt2)
            };
            var _res = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.CommonBool = dt.Rows[0]["_IsSocialAlert"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsSocialAlert"]);
                    _res.CommonStr = dt.Rows[0]["_SocialAlertTemplate"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["_SocialAlertTemplate"]);
                    _res.CommonStr2 = dt.Rows[0]["_ScanNo"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["_ScanNo"]);
                    _res.CommonStr3 = dt.Rows[0]["_CountryCode"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["_CountryCode"]);
                    _res.CommonInt = dt.Rows[0]["_SocialAlertAPIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SocialAlertAPIID"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetSocialAlertFormat";
    }
}
