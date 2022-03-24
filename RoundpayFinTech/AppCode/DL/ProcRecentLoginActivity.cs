using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRecentLoginActivity : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentLoginActivity(IDAL dal) => _dal = dal;
        public string GetName() => "proc_RecentUserLoginActivity";
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TopRows", _req.CommonInt)
            };
            var _alist = new List<LoginDetail>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new LoginDetail
                        {
                            LoginID = dt.Rows[i]["_LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_LoginID"].ToString()),
                            Prefix = dt.Rows[i]["_Prefix"] is DBNull ? string.Empty : dt.Rows[i]["_Prefix"].ToString(),
                            LoginMobile = dt.Rows[i]["_LoginMobile"] is DBNull ? string.Empty : dt.Rows[i]["_LoginMobile"].ToString(),
                            RequestIP = dt.Rows[i]["_RequestIP"] is DBNull ? string.Empty : dt.Rows[i]["_RequestIP"].ToString(),
                            CommonStr = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : dt.Rows[i]["_Remark"].ToString(),
                            CommonStr2= dt.Rows[i]["_EntryDate"] is DBNull ? string.Empty : dt.Rows[i]["_EntryDate"].ToString(),
                            Longitude = dt.Rows[i]["_Longitude"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Longitude"]),
                            Latitude = dt.Rows[i]["_Latitude"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Latitude"]),
                            IsActive = dt.Rows[i]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsActive"]),
                        };
                        _alist.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.CommonInt,
                    UserId = _req.CommonInt2
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return _alist;
        }
        public Task<object> Call() => throw new NotImplementedException();
    }
}
