using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTransactionServiceGI_Debit : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTransactionServiceGI_Debit(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GIUpdateRequestModel)obj;
            SqlParameter[] param = {
            new SqlParameter("@APICode",req.APICode??string.Empty),
            new SqlParameter("@APIOpCode",req.APIOpCode??string.Empty),
            new SqlParameter("@TransactionID",req.TransactionID??string.Empty),
            new SqlParameter("@OutletID",req.OutletID),
            new SqlParameter("@RechType",req.RechType??string.Empty),
            new SqlParameter("@VendorID",req.VendorID??string.Empty),
            new SqlParameter("@ActualAmount",req.ActualAmount),
            new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
            new SqlParameter("@RequestIP",req.RequestIP??string.Empty)
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
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog     
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    UserId = req.OutletID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_TransactionServiceGI_Debit";
    }
}
