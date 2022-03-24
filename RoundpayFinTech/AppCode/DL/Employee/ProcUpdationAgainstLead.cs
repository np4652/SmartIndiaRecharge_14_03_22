using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using RoundpayFinTech.AppCode.Model.Employee;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcUpdationAgainstLead : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdationAgainstLead(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (LeadDetail)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ContactUsID",req.Id),
                new SqlParameter("@RequestStatus",req.RequestStatus),
                new SqlParameter("@Remark",req.Remark),
                new SqlParameter("@Message",req.Message),
                new SqlParameter("@NextFollowup",req.NextFollowupDate),
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
        public string GetName() => "Proc_UpdationAgainstLead";
    }
}
