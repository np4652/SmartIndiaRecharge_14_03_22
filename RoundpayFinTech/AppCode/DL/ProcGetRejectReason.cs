using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRejectReason : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRejectReason(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (MasterVendorModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
        };
            var res = new List<MasterRejectReason>();

            try
            {
                var dt = _dal.Get(GetName(), param);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var elist = new MasterRejectReason
                        {
                            ID = Convert.ToInt32(row["_ID"] is DBNull ? 0 : row["_ID"]),
                            Reason = row["_Reason"] is DBNull ? "" : row["_Reason"].ToString()
                        };
                        res.Add(elist);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "select _ID , _Reason from Master_Reject_Reason";
    }
}
