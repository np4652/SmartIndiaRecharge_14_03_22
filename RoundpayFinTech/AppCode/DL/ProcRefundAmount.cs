
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL

{
    public class ProcRefundAmount : IProcedureAsync
    {
        private readonly IDAL _dal;
        
        public ProcRefundAmount(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@UserID", _req.CommonInt),
                new SqlParameter("@TID", _req.CommonInt2),
                new SqlParameter("@ChargedAmount", _req.CommonDecimal),
                new SqlParameter("@WalletID", _req.CommonInt3),
                new SqlParameter("@ServiceID", _req.CommonInt4),
                new SqlParameter("@AccountNo", _req.str ?? ""),
                new SqlParameter("@IP", _req.CommonStr ?? ""),
                new SqlParameter("@Browser", _req.CommonStr2 ?? "")
        };
            
            var _res = new ResponseStatus{
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(),param);
                if (dt.Rows.Count > 0) {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_RefundAmount";
        }
    }
}
