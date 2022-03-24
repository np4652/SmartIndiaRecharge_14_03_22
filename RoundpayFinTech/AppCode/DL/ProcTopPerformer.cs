using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTopPerformer : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTopPerformer(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ServiceID", _req.CommonInt),
                new SqlParameter("@FromDate", _req.CommonStr2??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.CommonStr3??DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _list = new List<TopPerformer>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var _performer = new TopPerformer
                    {
                        UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                        UserName = Convert.ToString(dt.Rows[i]["_UserName"]),
                        MobileNo = Convert.ToString(dt.Rows[i]["_MobileNo"]),
                        Email = Convert.ToString(dt.Rows[i]["_EmailID"]),
                        Role = Convert.ToString(dt.Rows[i]["_Role"]),
                        TotalSale = Convert.ToDecimal(dt.Rows[i]["_TotalSale"]),
                    };
                    _list.Add(_performer);
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
            return _list;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_TopPerformer";
    }
}
