using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAgreementApprovalNotification : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAgreementApprovalNotification(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var UserID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", UserID),
               // new SqlParameter("@WID", req.CommonInt2)
                
            };
            var resp = new AgreementApprovalNotification();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                foreach(DataRow row in dt.Rows)
                {
                    resp = new AgreementApprovalNotification
                    {
                        // ID = Convert.ToInt32(row["_ID"]),
                        MobileNo = Convert.ToString(row["_MobileNo"]),
                        Date = Convert.ToString(row["_Date"]),
                        Status = row["_Status"] is DBNull ? false : Convert.ToBoolean(row["_Status"]),
                    };
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
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_AgreementApprovalNotification";
    }
}
