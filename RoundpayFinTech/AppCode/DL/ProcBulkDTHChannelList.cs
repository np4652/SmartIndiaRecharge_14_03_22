using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcBulkDTHChannelList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBulkDTHChannelList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BulkInsertionObj)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PackageID",req.PackageID),
                new SqlParameter("@tp_DTHChannelList",req.tp_Rechargeplans)
                
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_BulkDTHChannelList";
    }
}
