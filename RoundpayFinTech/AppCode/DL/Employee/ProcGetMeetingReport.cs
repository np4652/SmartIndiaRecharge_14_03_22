using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetMeetingReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetMeetingReport(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@Top", _req.CommonInt),
                new SqlParameter("@DtFrom", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@DtTill", _req.CommonStr2 ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@Criteria", _req.CommonInt2),
                new SqlParameter("@CValue", _req.CommonStr3)
            };
            var _alist = new List<MeetingReportModel>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow dr in dt.Rows)
                {
                    _alist.Add(new MeetingReportModel
                    {
                        //Statuscode = ErrorCodes.One,
                        //Msg = "Success",
                        //ServiceID = Convert.ToInt32(dr["_ServiceID"]),
                        //OPTypeID = Convert.ToInt32(dr["_OPTypeID"]),
                        Id = Convert.ToInt32(dr["_ID"]),
                        UserId = Convert.ToInt32(dr["_UserID"]),
                        TotalTravel = Convert.ToDecimal(dr["_TotalTravel"]),
                        TotalExpense = Convert.ToDecimal(dr["_OtherExpenditure"]),
                        MeetingCount = (dr["MeetingCount"]).ToString().Trim() == "" ? 0 : Convert.ToInt32(dr["MeetingCount"]),
                        IsClosed = (dr["IsClosed"]).ToString().Trim() == "" ? false : Convert.ToBoolean(dr["IsClosed"]),
                        EntryDate = Convert.ToDateTime(dr["_EntryDate"]),
                        UserName = Convert.ToString(dr["_Name"])
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetMeetingReport";
    }
}
