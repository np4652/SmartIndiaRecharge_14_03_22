using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcASPaymentCollection : IProcedure
    {
        private readonly IDAL _dal;
        public ProcASPaymentCollection(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ASCollectionReq _req = (ASCollectionReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@Amount", _req.Amount),
                new SqlParameter("@CollectionMode", _req.CollectionMode),
                new SqlParameter("@BankName", _req.BankName),
                new SqlParameter("@UTR", _req.UTR??string.Empty),
                new SqlParameter("@Remark", _req.Remark ??string.Empty),
            };

            var _res = new ResponseStatus { 
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.UserID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ASPaymentCollection";
    }
}
