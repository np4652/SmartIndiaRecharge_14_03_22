using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcICICIRequest : IProcedure
    {
        private readonly IDAL _dal;

        public ProcICICIRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ICICIModelReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UTR",req.UTR??string.Empty),
                new SqlParameter("@BankInternalTransactionNumber",req.BankInternalTransactionNumber??string.Empty),
                new SqlParameter("@CustomerCode",req.CustomerCode??string.Empty),
                new SqlParameter("@VirtualACCode",req.VirtualACCode??string.Empty),
                new SqlParameter("@SenderRemark",req.SENDERREMARK??string.Empty),
                new SqlParameter("@CustomerAccountNo",req.CustomerAccountNo??string.Empty),
                new SqlParameter("@AMT",req.AMT??string.Empty),
                new SqlParameter("@PayeeName",req.PayeeName??string.Empty),
                new SqlParameter("@PayeeAccountNumber",req.PayeeAccountNumber??string.Empty),
                new SqlParameter("@PayeeBankIFSC",req.PayeeBankIFSC??string.Empty),
                new SqlParameter("@PayeePaymentDate",req.PayeePaymentDate??string.Empty),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@IsAxisBank",req.IsAxisBank),
                new SqlParameter("@IsQRICICI",req.IsQRICICI),
                new SqlParameter("@IsIPAY",req.IsIPay),
                new SqlParameter("@IsRazorpay",req.IsRazorpay),
                new SqlParameter("@CollectType",req.CollectType),
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
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
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
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ICICIRequest";
    }
}
