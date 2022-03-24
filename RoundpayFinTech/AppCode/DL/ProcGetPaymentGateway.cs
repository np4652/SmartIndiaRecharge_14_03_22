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
    public class ProcGetPaymentGateway : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPaymentGateway(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@PGID", req.CommonInt),
                new SqlParameter("@WID", req.CommonInt2)
                
            };
            var _alist = new List<PaymentGateway>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(),param).ConfigureAwait(false);
                foreach(DataRow row in dt.Rows)
                {
                    _alist.Add(new PaymentGateway
                    {
                        ID = Convert.ToInt32(row["_ID"]),
                        PGID = Convert.ToInt32(row["_PGID"]),
                        Name = Convert.ToString(row["_Name"]),
                        MerchantID = Convert.ToString(row["_MerchantID"]),
                        MerchantKey = Convert.ToString(row["_MerchantKey"]),
                        SuccessURL = Convert.ToString(row["_SuccessURL"]),
                        FailedURL = Convert.ToString(row["_FailedURL"]),
                        IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetPaymentGateway";
    }
}
