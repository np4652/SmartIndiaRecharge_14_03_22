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
    public class ProcGetUserfromOutletID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserfromOutletID(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@ApiID", _req.CommonInt),
                new SqlParameter("@ApiOutletID", _req.CommonStr),
                new SqlParameter("@Ip", _req.CommonStr2),
                new SqlParameter("@ServiceTypeID", _req.CommonInt2),
            };
            var _res = new ResponseStatusBalnace
            {
                Status=RechargeRespType._FAILED,
                Msg=ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                   
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        _res.Status = RechargeRespType._SUCCESS;
                        _res.Balance = Convert.ToDecimal(dt.Rows[0]["BalanceAmount"]);
                        _res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"].ToString();
                    }
                }
            }
            catch(Exception ex) {
                
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
            return _res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "Proc_GetUserfromOutletID";
        }
    }
}
