using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveSMSResponse : IProcedure
    {
        protected readonly IDAL _dal;
        public ProcSaveSMSResponse(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (SMSResponse)obj;
            SqlParameter[] param = {
                new SqlParameter("@SMSID", _req.SMSID),
                new SqlParameter("@MobileNo", string.IsNullOrEmpty(_req.MobileNo)?"":_req.MobileNo),
                new SqlParameter("@SMS", _req.SMS),
                new SqlParameter("@WID", _req.WID),
                new SqlParameter("@Status", _req.Status),
                new SqlParameter("@TransactionID", _req.TransactionID),
                new SqlParameter("@Response", _req.Response ?? ""),
                new SqlParameter("@ResponseID", _req.ResponseID ?? ""),
                new SqlParameter("@ReqURL", _req.ReqURL ?? ""),
                new SqlParameter("@SocialAlertType", _req.SocialAlertType),
            };
            ResponseStatus _resp = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_SaveSMSResponse";
    }
}