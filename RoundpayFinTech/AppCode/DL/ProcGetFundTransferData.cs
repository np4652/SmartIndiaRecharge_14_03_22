using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetFundTransferData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetFundTransferData(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@PaymentID", Convert.ToInt32(obj));
            var resp = new FundRequetResp()
            {
                StatusCode = ErrorCodes.Minus1,
                Description = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {

                    resp = new FundRequetResp
                    {
                        StatusCode = 1,
                        Description = "Report Found",
                        MobileNo = dt.Rows[0]["IMPSMobile"].ToString(),
                        MODE = dt.Rows[0]["_MODE"].ToString(),
                        Status = dt.Rows[0]["_Status"].ToString(),
                        TransactionId = dt.Rows[0]["_TransactionId"].ToString(),
                        UserId = Convert.ToInt32(dt.Rows[0]["_UserId"] is DBNull ? 0 : dt.Rows[0]["_UserId"]),
                        AccountHolder = dt.Rows[0]["_AccountHolder"].ToString(),
                        AccountNo = dt.Rows[0]["_AccountNo"].ToString(),
                        Amount = dt.Rows[0]["_Amount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[0]["_Amount"]),
                        Bank = dt.Rows[0]["_Bank"].ToString(),
                        CardNumber = dt.Rows[0]["_CardNumber"].ToString(),
                        ChequeNo = dt.Rows[0]["_ChequeNo"].ToString(),
                        EntryDate = dt.Rows[0]["_EntryDate"].ToString(),
                        UserName = dt.Rows[0]["_OutletName"].ToString(),
                        UserMobile = dt.Rows[0]["_MobileNo"].ToString(),
                        PaymentId = Convert.ToInt32(dt.Rows[0]["_ID"].ToString()),
                        WalletTypeID = Convert.ToInt32(dt.Rows[0]["_WalletTypeID"] is DBNull ? 0 : dt.Rows[0]["_WalletTypeID"])
                    };
                }
            }
            catch (Exception ex)
            { }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetFundTransferData";
    }
}
