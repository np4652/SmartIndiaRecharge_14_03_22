using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace RoundpayFinTech.AppCode.DL
{
   
    public class ProcUpdateLeadServiceRequest : IProcedure
    {
        private readonly IDAL _dal;
        List<LeadServiceRequest> LeadServicelst = null;
        public ProcUpdateLeadServiceRequest(IDAL dal)
        {
            _dal = dal;

        }
        public object Call(object obj)
        {
            var _req = (LeadServiceRequest)obj;
            SqlParameter[] param =
            {
                 new SqlParameter("@LT",_req.LT),
                new SqlParameter("@ID",_req.ID),
                new SqlParameter("@UserId",_req.ModifyBy),
                new SqlParameter("@Remark",_req.Remark),
                new SqlParameter("@LeadStatus",_req.LeadStatus),

            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.CommonInt = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]);

                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.EntryBy
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateLeadServiceRequest";
    }
}
