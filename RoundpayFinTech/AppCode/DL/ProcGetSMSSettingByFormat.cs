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
    public class ProcGetSMSSettingByFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSMSSettingByFormat(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetSMSSettingByFormat";
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@FormatID",req.CommonInt),
                new SqlParameter("@WID",req.CommonInt2)
            };
            var _res = new SMSSetting();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if ((dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])) == 1)
                    {
                        _res.ID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        _res.SMSID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        _res.FormatID = Convert.ToInt32(dt.Rows[0]["_FormatID"]);
                        _res.Template = Convert.ToString(dt.Rows[0]["_Template"]);
                        _res.IsEnableSMS = dt.Rows[0]["_IsEnableSMS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableSMS"]);
                        _res.APIType = dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIType"]);
                        _res.URL = Convert.ToString(dt.Rows[0]["_URL"]);
                        _res.APIMethod = Convert.ToString(dt.Rows[0]["_APIMethod"]);
                    }
                }
            }
            catch(Exception ex)
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
