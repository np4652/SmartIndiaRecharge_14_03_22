using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateAPIStatusCheck : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateAPIStatusCheck(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (APISTATUSCHECK)obj;
            SqlParameter[] param = {
                new SqlParameter("@ErrCode", _req.ErrorCode??""),
                new SqlParameter("@Checks", _req.Checks),
                new SqlParameter("@Status", _req.Status),
                new SqlParameter("@VendorID", _req.VendorIDIndex == null ? -1 : _req.VendorIDIndex),
                new SqlParameter("@OperatorId", _req.OperatorIDIndex == null ? -1 : _req.OperatorIDIndex),
                new SqlParameter("@TransactionId", _req.TransactionIDIndex == null ? -1 : _req.TransactionIDIndex),
                new SqlParameter("@Balance", _req.BalanceIndex == null ? -1 : _req.BalanceIndex),
                new SqlParameter("@VendorIDReplace", _req.VendorIDReplace),
                new SqlParameter("@OperatorIdReplace", _req.OperatorIDReplace),
                new SqlParameter("@TransactionIdReplace", _req.TransactionIDReplace),
                new SqlParameter("@BalanceReplace", _req.BalanceReplace),
                new SqlParameter("@IndLength", _req.IndLength),
                new SqlParameter("@ID", _req.ID)
            };            
            var _resp = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex) {
                string s = ex.Message;
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateAPIStatusCheck";
    }
}
